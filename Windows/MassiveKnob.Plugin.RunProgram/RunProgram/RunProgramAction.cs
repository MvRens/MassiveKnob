using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;

namespace MassiveKnob.Plugin.RunProgram.RunProgram
{
    public class RunProgramAction : IMassiveKnobAction
    {
        public Guid ActionId { get; } = new Guid("c3a79015-4b8f-414d-9682-02307de8639c");
        public MassiveKnobActionType ActionType { get; } = MassiveKnobActionType.InputDigital;
        public string Name { get; } = Strings.RunProgramName;
        public string Description { get; } = Strings.RunProgramDescription;


        public IMassiveKnobActionInstance Create(ILogger logger)
        {
            return new Instance(logger);
        }


        private class Instance : IMassiveKnobDigitalAction
        {
            private readonly ILogger logger;
            private IMassiveKnobActionContext actionContext;
            private RunProgramSettings settings;


            public Instance(ILogger logger)
            {
                this.logger = logger;
            }


            public void Initialize(IMassiveKnobActionContext context)
            {
                actionContext = context;
                settings = context.GetSettings<RunProgramSettings>();
            }


            public void Dispose()
            {
            }


            public UserControl CreateSettingsControl()
            {
                var viewModel = new RunProgramSettingsViewModel(settings);
                viewModel.PropertyChanged += (sender, args) =>
                {
                    actionContext.SetSettings(settings);
                };

                return new RunProgramSettingsView(viewModel);
            }
            

            public ValueTask DigitalChanged(bool on)
            {
                if (!on)
                    return default;

                if (string.IsNullOrEmpty(settings.Filename))
                    return default;
                
                logger.LogInformation("Run program: filename = {filename}, arguments = {arguments}", settings.Filename, settings.Arguments);
                Process.Start(new ProcessStartInfo
                {
                    FileName = settings.Filename,
                    Arguments = settings.Arguments,
                    UseShellExecute = true,
                    Verb = "open"
                });

                return default;
            }
        }
    }
}