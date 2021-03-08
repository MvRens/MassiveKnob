using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MIN;
using MIN.Abstractions;
using MIN.SerialPort;

namespace MassiveKnob.Plugin.SerialDevice.Worker
{
    public class SerialWorker : IDisposable
    {
        private readonly IMassiveKnobDeviceContext context;
        private readonly ILogger logger;

        private readonly object minProtocolLock = new object();
        private IMINProtocol minProtocol;
        private string lastPortName;
        private int lastBaudRate;
        private bool lastDtrEnable;


        private enum MassiveKnobFrameID
        {
            Handshake = 42,
            HandshakeResponse = 43,
            AnalogInput = 1,
            DigitalInput = 2,
            AnalogOutput = 3,
            DigitalOutput = 4,
            Quit = 62,
            Error = 63
        }


        
        public SerialWorker(IMassiveKnobDeviceContext context, ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }
        
        
        public void Dispose()
        {
            Disconnect();
        }
        
        
        public void Connect(string portName, int baudRate, bool dtrEnable)
        {
            lock (minProtocolLock)
            {
                if (portName == lastPortName && baudRate == lastBaudRate && dtrEnable == lastDtrEnable)
                    return;

                lastPortName = portName;
                lastBaudRate = baudRate;
                lastDtrEnable = dtrEnable;

                Disconnect();
                context.Connecting();

                if (string.IsNullOrEmpty(portName) || baudRate == 0)
                    return;

                 
                minProtocol?.Dispose();
                minProtocol = new MINProtocol(new MINSerialTransport(portName, baudRate, dtrEnable: dtrEnable), logger);
                minProtocol.OnConnected += MinProtocolOnOnConnected;
                minProtocol.OnFrame += MinProtocolOnOnFrame;
                minProtocol.Start();
            }
        }


        public void SetAnalogOutput(int analogOutputIndex, byte value)
        {
            IMINProtocol instance;
            
            lock (minProtocolLock)
            {
                instance = minProtocol;
            }
            
            instance?.QueueFrame(
                (byte)MassiveKnobFrameID.AnalogOutput, 
                new [] { (byte)analogOutputIndex, value });
        }

        public void SetDigitalOutput(int digitalOutputIndex, bool on)
        {
            IMINProtocol instance;

            lock (minProtocolLock)
            {
                instance = minProtocol;
            }

            instance?.QueueFrame(
                (byte)MassiveKnobFrameID.DigitalOutput,
                new [] { (byte)digitalOutputIndex, on ? (byte)1 : (byte)0 });
        }


        private void MinProtocolOnOnConnected(object sender, EventArgs e)
        {
            IMINProtocol instance;

            lock (minProtocolLock)
            {
                if (minProtocol != sender as IMINProtocol)
                    return;
                
                instance = minProtocol;
            }
            
            if (instance == null)
                return;

            Task.Run(async () =>
            {
                await instance.Reset();
                await instance.QueueFrame((byte)MassiveKnobFrameID.Handshake, new[] { (byte)'M', (byte)'K' });
            });
        }
        

        private void MinProtocolOnOnFrame(object sender, MINFrameEventArgs e)
        {
            IMINProtocol instance;

            lock (minProtocolLock)
            {
                if (minProtocol != sender as IMINProtocol)
                    return;

                instance = minProtocol;
            }

            if (instance == null)
                return;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault - by design
            switch ((MassiveKnobFrameID)e.Id)
            {
                case MassiveKnobFrameID.HandshakeResponse:
                    if (e.Payload.Length < 4)
                    {
                        logger.LogError("Invalid handshake response length, expected 4, got {length}: {payload}",
                            e.Payload.Length, BitConverter.ToString(e.Payload));
                        
                        Disconnect();
                        return;
                    }

                    var specs = new DeviceSpecs(e.Payload[0], e.Payload[1], e.Payload[2], e.Payload[3]);
                    context.Connected(specs);
                    break;
                
                case MassiveKnobFrameID.AnalogInput:
                    if (e.Payload.Length < 2)
                    {
                        logger.LogError("Invalid analog input payload length, expected 2, got {length}: {payload}",
                            e.Payload.Length, BitConverter.ToString(e.Payload));
                        return;
                    }

                    context.AnalogChanged(e.Payload[0], e.Payload[1]);
                    break;
                
                case MassiveKnobFrameID.DigitalInput:
                    if (e.Payload.Length < 2)
                    {
                        logger.LogError("Invalid digital input payload length, expected 2, got {length}: {payload}",
                            e.Payload.Length, BitConverter.ToString(e.Payload));
                        return;
                    }

                    context.DigitalChanged(e.Payload[0], e.Payload[1] != 0);
                    break;

                case MassiveKnobFrameID.Error:
                    logger.LogError("Error message received from device: {message}", Encoding.ASCII.GetString(e.Payload));
                    break;

                default:
                    logger.LogWarning("Unknown frame ID received: {frameId}", e.Id);
                    break;
            }
        }


        private void Disconnect()
        {
            lock (minProtocolLock)
            {
                minProtocol?.Dispose();
                minProtocol = null;
            }
            
            context.Disconnected();
        }



        /*
                void SafeCloseSerialPort()
                {
                    try
                    {
                        serialPort?.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }

                    serialPort = null;
                    context.Connecting();
                }


                while (serialPort == null && !cancellationToken.IsCancellationRequested)
                {
                    if (!TryConnect(ref serialPort, settings, out specs))
                    {
                        SafeCloseSerialPort();
                        Thread.Sleep(500);
                    }
                    else
                        break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    SafeCloseSerialPort();
                    break;
                }

                var processingMessage = false;

                Debug.Assert(serialPort != null, nameof(serialPort) + " != null");
                serialPort.DataReceived += (sender, args) =>
                {
                    if (args.EventType != SerialData.Chars || processingMessage)
                        return;

                    var senderPort = (SerialPort)sender;
                    processingMessage = true;
                    try
                    {
                        var message = (char)senderPort.ReadByte();
                        ProcessMessage(senderPort, message);
                    }
                    finally
                    {
                        processingMessage = false;
                    }
                };


                context.Connected(specs);
                try
                {
                    // This is where sending data to the hardware would be implemented
                    while (serialPort.IsOpen && !cancellationToken.IsCancellationRequested)
                    {
                        Thread.Sleep(10);
                    }
                }
                catch
                {
                    // ignored
                }

                context.Disconnected();
                SafeCloseSerialPort();

                if (!cancellationToken.IsCancellationRequested)
                    Thread.Sleep(500);
            }
        }


        private static bool TryConnect(ref SerialPort serialPort, ConnectionSettings settings, out DeviceSpecs specs)
        {
            try
            {
                serialPort = new SerialPort(settings.PortName, settings.BaudRate)
                {
                    Encoding = Encoding.ASCII,
                    ReadTimeout = 1000, 
                    WriteTimeout = 1000,
                    DtrEnable = settings.DtrEnable
                };
                serialPort.Open();

                // Send handshake
                serialPort.Write(new[] { 'H', 'M', 'K', 'B' }, 0, 4);

                // Wait for reply
                var response = serialPort.ReadByte();

                if ((char) response == 'H')
                {
                    specs = new DeviceSpecs(serialPort.ReadByte(), serialPort.ReadByte(), serialPort.ReadByte(), serialPort.ReadByte());
                    if (specs.AnalogInputCount > -1 && specs.DigitalInputCount > -1 && specs.AnalogOutputCount > -1 && specs.DigitalOutputCount > -1)
                        return true;
                }
                else
                    CheckForError(serialPort, (char)response);

                specs = default;
                return false;
            }
            catch
            {
                specs = default;
                return false;
            }
        }


        private void ProcessMessage(SerialPort serialPort, char message)
        {
            switch (message)
            {
                case 'V':
                    var knobIndex = (byte)serialPort.ReadByte();
                    var volume = (byte)serialPort.ReadByte();

                    if (knobIndex < 255 && volume <= 100)
                        context.AnalogChanged(knobIndex, volume);

                    break;
            }
        }


        private static void CheckForError(SerialPort serialPort, char message)
        {
            if (message != 'E')
                return;

            var length = serialPort.ReadByte();
            if (length <= 0)
                return;

            var buffer = new byte[length];
            var bytesRead = 0;
            
            while (bytesRead < length)
                bytesRead += serialPort.Read(buffer, bytesRead, length - bytesRead);

            var errorMessage = Encoding.ASCII.GetString(buffer);
            Debug.Print(errorMessage);
        }


        private readonly struct ConnectionSettings
        {
            public readonly string PortName;
            public readonly int BaudRate;
            public readonly bool DtrEnable;

            public ConnectionSettings(string portName, int baudRate, bool dtrEnable)
            {
                PortName = portName;
                BaudRate = baudRate;
                DtrEnable = dtrEnable;
            }
        }
        */
    }
}
