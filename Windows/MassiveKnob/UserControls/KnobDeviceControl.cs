using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MassiveKnob.Hardware;

namespace MassiveKnob.UserControls
{
    public partial class KnobDeviceControl : UserControl
    {
        private int knobIndex;
        private Guid? deviceId;

        public event KnobDeviceChangedEventHandler OnDeviceChanged;


        public KnobDeviceControl()
        {
            InitializeComponent();

            DeviceCombobox.DisplayMember = @"DisplayName";
        }


        public void SetKnobIndex(int index)
        {
            knobIndex = index;
            KnobIndexLabel.Text = string.Format(Strings.KnobIndex, index + 1);
        }


        public void SetDeviceId(Guid? value)
        {
            deviceId = value;

            if (DeviceCombobox.Items.Count > 0)
                DeviceCombobox.SelectedItem = value.HasValue ? new DeviceItem(value.Value) : null;
        }

        
        public void SetDevices(IEnumerable<IAudioDevice> devices)
        {
            DeviceCombobox.BeginUpdate();
            try
            {
                DeviceCombobox.Items.Clear();

                if (devices == null)
                    return;

                var sortedDevices = devices.OrderBy(d => d.DisplayName);

                foreach (var device in sortedDevices)
                {
                    var itemIndex = DeviceCombobox.Items.Add(
                        new DeviceItem(device.Id)
                        {
                            DisplayName = device.DisplayName
                        });
                    
                    if (deviceId.HasValue && deviceId.Value == device.Id)
                        DeviceCombobox.SelectedIndex = itemIndex;
                }
            }
            finally
            {
                DeviceCombobox.EndUpdate();
            }
        }


        private void DeviceCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnDeviceChanged?.Invoke(this, new KnobDeviceChangedEventArgs
            {
                KnobIndex = knobIndex,
                DeviceId = ((DeviceItem)DeviceCombobox.SelectedItem)?.DeviceId
            });
        }
        

        private class DeviceItem : IEquatable<DeviceItem>
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local - used by ComboBox
            public Guid DeviceId { get; }
            public string DisplayName { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local

            
            public DeviceItem(Guid deviceId)
            {
                DeviceId = deviceId;
            }

            
            public bool Equals(DeviceItem other)
            {
                if (other == null) return false;
                
                return ReferenceEquals(this, other) || DeviceId.Equals(other.DeviceId);
            }

            
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (ReferenceEquals(this, obj)) return true;
                
                return obj is DeviceItem deviceItem && Equals(deviceItem);
            }

            
            public override int GetHashCode()
            {
                return DeviceId.GetHashCode();
            }
        }
    }
	
	
    public class KnobDeviceChangedEventArgs : EventArgs
    {
        public int KnobIndex { get; set; }
        public Guid? DeviceId { get; set; }
    }
    
    public delegate void KnobDeviceChangedEventHandler(object sender, KnobDeviceChangedEventArgs e);	
}
