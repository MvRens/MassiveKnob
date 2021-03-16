using MassiveKnob.View;
using MassiveKnob.View.Settings;
using MassiveKnob.ViewModel;
using MassiveKnob.ViewModel.Settings;
using SimpleInjector;

namespace MassiveKnob
{
    public static class ContainerBuilder
    {
        public static Container Create()
        {
            var container = new Container();
            container.Options.EnableAutoVerification = false;

            container.Register<App>();
            
            container.Register<SettingsWindow>();
            container.Register<SettingsViewModel>();

            container.Register<SettingsDeviceView>();
            container.Register<SettingsDeviceViewModel>();

            container.Register<SettingsAnalogInputsView>();
            container.Register<SettingsAnalogInputsViewModel>();

            container.Register<SettingsDigitalInputsView>();
            container.Register<SettingsDigitalInputsViewModel>();

            container.Register<SettingsAnalogOutputsView>();
            container.Register<SettingsAnalogOutputsViewModel>();

            container.Register<SettingsDigitalOutputsView>();
            container.Register<SettingsDigitalOutputsViewModel>();

            container.Register<SettingsLoggingView>();
            container.Register<SettingsLoggingViewModel>();

            container.Register<SettingsStartupView>();
            container.Register<SettingsStartupViewModel>();

            container.Register<SettingsPluginsView>();
            container.Register<SettingsPluginsViewModel>();

            return container;
        }
    }
}
