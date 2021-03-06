using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using MassiveKnob.Core;
using MassiveKnob.Plugin;

namespace MassiveKnob.ViewModel
{
    public class InputOutputViewModel : IDisposable, INotifyPropertyChanged
    {
        private readonly IMassiveKnobOrchestrator orchestrator;
        private readonly MassiveKnobActionType actionType;
        private readonly int index;

        private ActionViewModel selectedAction;
        private UserControl actionSettingsControl;


        // ReSharper disable UnusedMember.Global - used by WPF Binding
        public string DisplayName => string.Format(
            actionType == MassiveKnobActionType.OutputAnalog || actionType == MassiveKnobActionType.OutputDigital
                ? Strings.OutputHeader
                : Strings.InputHeader, 
            index + 1);

        public IList<ActionViewModel> Actions { get; }


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

                if (actionSettingsControl is IDisposable disposable)
                    disposable.Dispose();
                
                actionSettingsControl = value;
                OnPropertyChanged();
            }
        }
        // ReSharper restore UnusedMember.Global            


        public InputOutputViewModel(SettingsViewModel settingsViewModel, IMassiveKnobOrchestrator orchestrator, MassiveKnobActionType actionType, int index)
        {
            this.orchestrator = orchestrator;
            this.actionType = actionType;
            this.index = index;
            
            
            // For design-time support
            if (orchestrator == null)
                return;


            Actions = settingsViewModel.Actions.Where(a => a.RepresentsNull || a.Action.ActionType == actionType).ToList();

            var actionInfo = orchestrator.GetAction(actionType, index);
            
            selectedAction = actionInfo != null
                ? Actions.SingleOrDefault(a => !a.RepresentsNull && a.Action.ActionId == actionInfo.Info.ActionId)
                : Actions.Single(a => a.RepresentsNull);

            actionSettingsControl = actionInfo?.Instance.CreateSettingsControl();
        }

        
        public void Dispose()
        {
            if (ActionSettingsControl is IDisposable disposable)
                disposable.Dispose();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
