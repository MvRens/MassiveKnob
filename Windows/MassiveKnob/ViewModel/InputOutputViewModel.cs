using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using MassiveKnob.Model;
using MassiveKnob.Plugin;

namespace MassiveKnob.ViewModel
{
    public class InputOutputViewModel : INotifyPropertyChanged
    {
        private readonly SettingsViewModel settingsViewModel;
        private readonly IMassiveKnobOrchestrator orchestrator;
        private readonly MassiveKnobActionType actionType;
        private readonly int index;

        private ActionViewModel selectedAction;
        private UserControl actionSettingsControl;


        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public string DisplayName => actionType == MassiveKnobActionType.OutputAnalog || actionType == MassiveKnobActionType.OutputDigital
            ? $"Output #{index + 1}"
            : $"Input #{index + 1}";

        public IList<ActionViewModel> Actions => settingsViewModel.Actions;


        public ActionViewModel SelectedAction
        {
            get => selectedAction;
            set
            {
                if (value == selectedAction)
                    return;

                selectedAction = value == null || value.RepresentsNull ? null : value;
                var actionInfo = orchestrator.SetAction(actionType, index, selectedAction?.Action);

                OnPropertyChanged();

                ActionSettingsControl = actionInfo?.Instance.CreateSettingsControl();
            }
        }

        public UserControl ActionSettingsControl
        {
            get => actionSettingsControl;
            set
            {
                if (value == actionSettingsControl)
                    return;

                actionSettingsControl = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global            


        public InputOutputViewModel(SettingsViewModel settingsViewModel, IMassiveKnobOrchestrator orchestrator, MassiveKnobActionType actionType, int index)
        {
            this.settingsViewModel = settingsViewModel;
            this.orchestrator = orchestrator;
            this.actionType = actionType;
            this.index = index;

            var actionInfo = orchestrator.GetAction(actionType, index);
            
            selectedAction = actionInfo != null
                ? Actions.SingleOrDefault(a => !a.RepresentsNull && a.Action.ActionId == actionInfo.Info.ActionId)
                : Actions.Single(a => a.RepresentsNull);

            actionSettingsControl = actionInfo?.Instance.CreateSettingsControl();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
