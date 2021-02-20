using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace MassiveKnob.Hardware
{
    public class SerialMassiveKnobHardware : AbstractMassiveKnobHardware
    {
        private readonly string portName;
        private Thread workerThread;
        
        private readonly CancellationTokenSource workerThreadCancellation = new CancellationTokenSource();
        private readonly TaskCompletionSource<bool> workerThreadCompleted = new TaskCompletionSource<bool>();
             

        public SerialMassiveKnobHardware(string portName)
        {
            this.portName = portName;
        }


        public override async Task TryConnect()
        {
            if (workerThread != null)
                await Disconnect();

            workerThread = new Thread(RunWorker)
            {
                Name = "SerialMassiveKnobHardware Worker"
            };
            workerThread.Start();
        }

        
        public override async Task Disconnect()
        {
            workerThreadCancellation.Cancel();
            await workerThreadCompleted.Task;

            workerThread = null;
        }


        private void RunWorker()
        {
            Observers.Connecting();
            
            while (!workerThreadCancellation.IsCancellationRequested)
            {
                SerialPort serialPort = null;

                void SafeCloseSerialPort()
                {
                    try
                    {
                        serialPort?.Dispose();
                    }
                    catch
                    {
                        // ignroed
                    }

                    serialPort = null;
                    Observers.Disconnected();
                    Observers.Connecting();
                }
                
                
                var knobCount = 0;

                while (serialPort == null && !workerThreadCancellation.IsCancellationRequested)
                {
                    try
                    {
                        serialPort = new SerialPort(portName, 115200);
                        serialPort.Open();

                        // Send handshake
                        serialPort.Write(new[] { 'H', 'M', 'K', 'B' }, 0, 4);

                        // Wait for reply
                        serialPort.ReadTimeout = 1000;
                        var response = serialPort.ReadByte();

                        if ((char) response == 'H')
                        {
                            knobCount = serialPort.ReadByte();
                            if (knobCount > -1)
                                break;
                        }

                        SafeCloseSerialPort();
                        Thread.Sleep(500);
                    }
                    catch
                    {
                        SafeCloseSerialPort();
                        Thread.Sleep(500);
                    }
                }

                if (workerThreadCancellation.IsCancellationRequested)
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

                    var senderPort = (SerialPort) sender;
                    processingMessage = true;
                    try
                    {
                        var message = (char) senderPort.ReadByte();
                        ProcessMessage(senderPort, message);
                    }
                    finally
                    {
                        processingMessage = false;
                    }
                };


                Observers.Connected(knobCount);
                try
                {
                    // This is where sending data to the hardware would be implemented
                    while (serialPort.IsOpen && !workerThreadCancellation.IsCancellationRequested)
                    {
                        Thread.Sleep(10);
                    }
                }
                catch
                {
                    // ignored
                }

                Observers.Disconnected();
                SafeCloseSerialPort();
                
                if (!workerThreadCancellation.IsCancellationRequested)
                    Thread.Sleep(500);
            }

            workerThreadCompleted.TrySetResult(true);
        }
        
        
        private void ProcessMessage(SerialPort serialPort, char message)
        {
            switch (message)
            {
                case 'V':
                    var knobIndex = (byte)serialPort.ReadByte();
                    var volume = (byte)serialPort.ReadByte();
                    
                    if (knobIndex < 255 && volume <= 100)
                        Observers.VolumeChanged(knobIndex, volume);
                    
                    break;
            }
        }
    }


    public class SerialMassiveKnobHardwareFactory : IMassiveKnobHardwareFactory
    {
        public IMassiveKnobHardware Create(string portName)
        {
            return new SerialMassiveKnobHardware(portName);
        }
    }
}
