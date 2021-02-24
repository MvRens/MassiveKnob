using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace MassiveKnob.Plugin.SerialDevice.Worker
{
    public class SerialWorker : IDisposable
    {
        private readonly IMassiveKnobDeviceContext context;

        private readonly object workerLock = new object();
        private string lastPortName;
        private int lastBaudRate;
        private Thread workerThread;
        private CancellationTokenSource workerThreadCancellation = new CancellationTokenSource();

        public SerialWorker(IMassiveKnobDeviceContext context)
        {
            this.context = context;
        }
        
        
        public void Dispose()
        {
            Disconnect();
        }
        
        
        public void Connect(string portName, int baudRate)
        {
            lock (workerLock)
            {
                if (portName == lastPortName && baudRate == lastBaudRate)
                    return;

                lastPortName = portName;
                lastBaudRate = baudRate;

                Disconnect();

                if (string.IsNullOrEmpty(portName) || baudRate == 0)
                    return;


                workerThreadCancellation = new CancellationTokenSource();
                workerThread = new Thread(() => RunWorker(workerThreadCancellation.Token, portName, baudRate))
                {
                    Name = "MassiveKnobSerialDevice Worker"
                };
                workerThread.Start();
            }
        }


        private void Disconnect()
        {
            lock (workerLock)
            {
                workerThreadCancellation?.Cancel();

                workerThreadCancellation = null;
                workerThread = null;
            }
        }



        private void RunWorker(CancellationToken cancellationToken, string portName, int baudRate)
        {
            context.Connecting();
            while (!cancellationToken.IsCancellationRequested)
            {
                SerialPort serialPort = null;
                DeviceSpecs specs = default;

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
                    if (!TryConnect(ref serialPort, portName, baudRate, out specs))
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


        private static bool TryConnect(ref SerialPort serialPort, string portName, int baudRate, out DeviceSpecs specs)
        {
            try
            {
                serialPort = new SerialPort(portName, baudRate)
                {
                    Encoding = Encoding.ASCII,
                    ReadTimeout = 1000, 
                    WriteTimeout = 1000,
                    DtrEnable = true // TODO make setting
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
    }
}
