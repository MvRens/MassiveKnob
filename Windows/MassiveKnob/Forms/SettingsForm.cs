using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapplo.Windows.Devices;
using Dapplo.Windows.Devices.Enums;
using MassiveKnob.Hardware;
using MassiveKnob.Settings;
using MassiveKnob.UserControls;
using Nito.AsyncEx;

namespace MassiveKnob.Forms
{
    public partial class SettingsForm : Form, IMassiveKnobHardwareObserver, IObserver<DeviceNotificationEvent>
    {
        private readonly IAudioDeviceManager audioDeviceManager;
        private readonly IMassiveKnobHardwareFactory massiveKnobHardwareFactory;
        private readonly List<KnobDeviceControl> knobDeviceControls = new List<KnobDeviceControl>();

        private bool loading = true;
        private string lastConnectedPort = null;
        private IDisposable deviceSubscription;
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

            // Due to the form not being visible initially (see SetVisibleCore), we can't use the Load event
            AsyncLoad();
        }
        

        private async void AsyncLoad()
        {
            await LoadSettings();

            await Task.WhenAll(
                LoadSerialPorts(),
                LoadAudioDevices()
            );

            deviceSubscription = DeviceNotification.OnNotification.Subscribe(this);

            loading = false;
            await Connect();
        }


        private void RunInUIContext(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }
        
        
        private Task LoadSerialPorts()
        {
            var portNames = SerialPort.GetPortNames();
            
            RunInUIContext(() =>
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
            RunInUIContext(() => SetSettings(newSettings));
        }


        private async Task SaveSettings()
        {
            if (settings == null)
                return;

            using (await saveSettingsLock.LockAsync())
            { 
                await SettingsJsonSerializer.Serialize(settings);
            }
        }


        private async Task Connect()
        {
            string serialPort = null;
            
            RunInUIContext(() =>
            {
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
            RunInUIContext(() => SetDevices(newDevices));
        }



        private void SetSettings(Settings.Settings value)
        {
            if (value == null)
                return;

            SerialPortCombobox.SelectedItem = value.SerialPort;
            
            // No need to update the knob device user controls, as they are not loaded yet
            // (guaranteed by the order in AsyncLoad)

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

                    knobDeviceControl.OnDeviceChanged += async (sender, args) =>
                    {
                        while (settings.Knobs.Count - 1 < args.KnobIndex)
                            settings.Knobs.Add(new Settings.Settings.KnobSettings());

                        settings.Knobs[args.KnobIndex].DeviceId = args.DeviceId;
                        await SaveSettings();
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
            {
                startupVisibleCalled = true;

                // Make sure the underlying window is still created, otherwise Close won't work
                if (!IsHandleCreated)
                    CreateHandle();
            }
            else
                base.SetVisibleCore(value);
        }


        private void Settings()
        {
            Show();
        }
        
        
        private async Task Quit()
        {
            Hide();
            
            deviceSubscription?.Dispose();
            
            foreach (var knobDeviceControl in knobDeviceControls)
                knobDeviceControl.Dispose();

            knobDeviceControls.Clear();


            if (hardware != null)
            {
                hardware.DetachObserver(this);
                await hardware.Disconnect();
            }

            audioDeviceManager?.Dispose();

            closing = true;
            Close();
        }


        public void Connecting()
        {
            RunInUIContext(() =>
            {
                SerialPortStatusLabel.Text = Strings.StatusConnecting;
            });
        }
        

        public void Connected(int knobCount)
        {
            RunInUIContext(() =>
            {
                SerialPortStatusLabel.Text = Strings.StatusConnected;
                SetKnobCount(knobCount);
            });
        }
        

        public void Disconnected()
        {
            RunInUIContext(() =>
            {
                SerialPortStatusLabel.Text = Strings.StatusNotConnected;
                lastConnectedPort = null;
            });
        }


        public async void VolumeChanged(int knob, int volume)
        {
            if (knob >= settings.Knobs.Count)
                return;

            if (!settings.Knobs[knob].DeviceId.HasValue)
                return;

            var deviceId = settings.Knobs[knob].DeviceId.Value;

            using (await setVolumeLock.LockAsync())
            {
                var device = await audioDeviceManager.GetDeviceById(deviceId);
                if (device != null)
                    await device.SetVolume(volume);
            }
        }


        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closing) 
                return;
            
            Hide();
            e.Cancel = true;
        }

        
        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Settings();
        }

        
        private async void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await Quit();
        }

        
        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings();
        }

        
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        
        private async void SerialPortCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var newPort = (string) SerialPortCombobox.SelectedItem;
            if (loading || newPort == lastConnectedPort)
                return;

            lastConnectedPort = newPort;
            if (settings.SerialPort != newPort)
            {
                settings.SerialPort = (string) SerialPortCombobox.SelectedItem;
                await SaveSettings();
            }

            await Connect();
        }

        
        public async void OnNext(DeviceNotificationEvent value)
        {
            if ((value.EventType == DeviceChangeEvent.DeviceArrival ||
                 value.EventType == DeviceChangeEvent.DeviceRemoveComplete) &&
                value.Is(DeviceBroadcastDeviceType.DeviceInterface))
            {
                await LoadSerialPorts();
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}
