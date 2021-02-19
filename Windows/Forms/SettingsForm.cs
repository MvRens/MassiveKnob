using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MassiveKnob.Hardware;
using MassiveKnob.Settings;
using MassiveKnob.UserControls;
using Nito.AsyncEx;

namespace MassiveKnob.Forms
{
    public partial class SettingsForm : Form, IMassiveKnobHardwareObserver
    {
        private readonly IAudioDeviceManager audioDeviceManager;
        private readonly IMassiveKnobHardwareFactory massiveKnobHardwareFactory;
        private readonly List<KnobDeviceControl> knobDeviceControls = new List<KnobDeviceControl>();

        private bool loading = true;
        private IMassiveKnobHardware hardware;
        private IAudioDevice[] devices;
        private Settings.Settings settings;
        
        private readonly AsyncLock saveSettingsLock = new AsyncLock();
        private readonly AsyncLock setVolumeLock = new AsyncLock();

        private bool startupVisibleCalled;
        private bool closing;


        public SettingsForm(IAudioDeviceManagerFactory audioDeviceManagerFactory, IMassiveKnobHardwareFactory massiveKnobHardwareFactory)
        {
            audioDeviceManager = audioDeviceManagerFactory.Create();
            this.massiveKnobHardwareFactory = massiveKnobHardwareFactory;
            
            InitializeComponent();

            SerialPortStatusLabel.Text = Strings.StatusNotConnected;

            Task.Run(async () =>
            {
                await LoadSettings();

                await Task.WhenAll(
                    LoadSerialPorts(),
                    LoadAudioDevices()
                );

                loading = false;
                await Connect();
            }).ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                    SafeCall(() => throw t.Exception);
            });
        }
        
        
        private void SafeCall(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }


        private Task LoadSerialPorts()
        {
            var portNames = SerialPort.GetPortNames();
            
            SafeCall(() =>
            {
                SerialPortCombobox.BeginUpdate();
                try
                {
                    SerialPortCombobox.Items.Clear();
                    foreach (var portName in portNames)
                    {
                        var itemIndex = SerialPortCombobox.Items.Add(portName);

                        if (portName == settings.SerialPort)
                            SerialPortCombobox.SelectedIndex = itemIndex;
                    }
                }
                finally
                {
                    SerialPortCombobox.EndUpdate();
                }
            });

            return Task.CompletedTask;
        }
        
        
        private async Task LoadSettings()
        {
            var newSettings = await SettingsJsonSerializer.Deserialize();
            SafeCall(() => SetSettings(newSettings));
        }


        private void SaveSettings()
        {
            if (settings == null)
                return;
            
            Task.Run(async () =>
            {
                using (await saveSettingsLock.LockAsync())
                { 
                    await SettingsJsonSerializer.Serialize(settings);
                }
            });
        }


        private async Task Connect()
        {
            string serialPort = null;
            
            SafeCall(() =>
            {
                SerialPortStatusLabel.Text = Strings.StatusConnecting;
                serialPort = (string)SerialPortCombobox.SelectedItem;
            });

            if (string.IsNullOrEmpty(serialPort))
                return;

            if (hardware != null)
            {
                hardware.DetachObserver(this);
                await hardware.Disconnect();
            }

            hardware = massiveKnobHardwareFactory.Create(serialPort);
            hardware.AttachObserver(this);

            await hardware.TryConnect();
        }


        private async Task LoadAudioDevices()
        {
            var newDevices = await audioDeviceManager.GetDevices();
            SafeCall(() => SetDevices(newDevices));
        }



        private void SetSettings(Settings.Settings value)
        {
            if (value == null)
                return;

            SerialPortCombobox.SelectedItem = value.SerialPort;
            
            // No need to update the knob device user controls, as they are not loaded yet

            settings = value;
        }

        private void SetDevices(IEnumerable<IAudioDevice> value)
        {
            devices = value.ToArray();
            
            foreach (var knobDeviceControl in knobDeviceControls)
                knobDeviceControl.SetDevices(devices);
        }
        
        
        private void SetKnobCount(int count)
        {
            if (count == knobDeviceControls.Count)
                return;

            SuspendLayout();
            try
            {
                DeviceCountUnknownLabel.Visible = count == 0;

                if (knobDeviceControls.Count > count)
                {
                    for (var i = count; i < knobDeviceControls.Count; i++)
                        knobDeviceControls[i].Dispose();

                    knobDeviceControls.RemoveRange(count, knobDeviceControls.Count - count);
                }

                for (var i = knobDeviceControls.Count; i < count; i++)
                {
                    var knobDeviceControl = new KnobDeviceControl
                    {
                        Left = 0,
                        Width = DevicesPanel.Width
                    };

                    knobDeviceControl.Top = i * knobDeviceControl.Height;
                    knobDeviceControl.Parent = DevicesPanel;

                    knobDeviceControl.SetKnobIndex(i);
                    knobDeviceControl.SetDevices(devices);

                    if (i < settings.Knobs.Count)
                        knobDeviceControl.SetDeviceId(settings.Knobs[i].DeviceId);

                    knobDeviceControl.OnDeviceChanged += (sender, args) =>
                    {
                        while (settings.Knobs.Count - 1 < args.KnobIndex)
                            settings.Knobs.Add(new Settings.Settings.KnobSettings());

                        settings.Knobs[args.KnobIndex].DeviceId = args.DeviceId;
                        SaveSettings();
                    };

                    knobDeviceControls.Add(knobDeviceControl);
                }


                var expectedHeight = knobDeviceControls.Count > 0
                    ? knobDeviceControls[0].Height * count
                    : DeviceCountUnknownLabel.Height;

                if (expectedHeight == DevicesPanel.Height)
                    return;

                var diff = expectedHeight - DevicesPanel.Height;
                Height += diff;
                Top -= diff / 2;
            }
            finally
            {
                ResumeLayout();
            }
        }


        protected override void SetVisibleCore(bool value)
        {
            // Prevent the form from showing at startup
            if (!startupVisibleCalled)
                startupVisibleCalled = true;
            else
                base.SetVisibleCore(value);
        }


        private void Settings()
        {
            Show();
        }
        
        
        private void Quit()
        {
            closing = true;
            Close();
        }


        public void Connected(int knobCount)
        {
            SafeCall(() =>
            {
                SerialPortStatusLabel.Text = Strings.StatusConnected;
                SetKnobCount(knobCount);
            });
        }
        

        public void Disconnected()
        {
            SafeCall(() =>
            {
                SerialPortStatusLabel.Text = Strings.StatusNotConnected;
            });
        }


        public void VolumeChanged(int knob, int volume)
        {
            if (knob >= settings.Knobs.Count)
                return;

            if (!settings.Knobs[knob].DeviceId.HasValue)
                return;

            var deviceId = settings.Knobs[knob].DeviceId.Value;

            Task.Run(async () =>
            {
                using (await setVolumeLock.LockAsync())
                {
                    var device = await audioDeviceManager.GetDeviceById(deviceId);
                    if (device != null)
                        await device.SetVolume(volume);
                }
            });
        }


        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closing) 
                return;
            
            Hide();
            e.Cancel = true;
        }


        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var knobDeviceControl in knobDeviceControls)
                knobDeviceControl.Dispose();

            knobDeviceControls.Clear();
            

            hardware?.DetachObserver(this);
            hardware?.Disconnect().GetAwaiter().GetResult();
            audioDeviceManager?.Dispose();
        }        
        
        
        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Settings();
        }

        
        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Quit();
        }

        
        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings();
        }

        
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        
        private void SerialPortCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading || (string)SerialPortCombobox.SelectedItem == settings.SerialPort)
                return;
            
            settings.SerialPort = (string) SerialPortCombobox.SelectedItem;
            SaveSettings();

            Task.Run(Connect);
        }
    }
}
