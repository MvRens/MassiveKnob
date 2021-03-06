namespace MassiveKnob.Plugin.SerialDevice.Settings
{
    public class SerialDeviceSettings
    {
        public string PortName { get; set; } = null;
        public int BaudRate { get; set; } = 115200;
        public bool DtrEnable { get; set; } = false;
    }
}
