using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Windows;
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
                OnDependantPropertyChanged(nameof(DigitalToAnalogVisibility));

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

        public Visibility DigitalToAnalogVisibility
        {
            get
            {
                // Design-time support
                if (orchestrator == null)
                    return Visibility.Visible;

                if (actionType != MassiveKnobActionType.OutputAnalog)
                    return Visibility.Collapsed;

                if (SelectedAction == null || SelectedAction.RepresentsNull)
                    return Visibility.Collapsed;

                return SelectedAction.Action.ActionType == MassiveKnobActionType.OutputDigital
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }


        private readonly Subject<bool> throttledDigitalToAnalogChanged = new Subject<bool>();
        private readonly IDisposable digitalToAnalogChangedSubscription;

        private byte digitalToAnalogOn;
        public byte DigitalToAnalogOn
        {
            get => digitalToAnalogOn;
            set
            {
                if (actionType != MassiveKnobActionType.OutputAnalog || value == digitalToAnalogOn)
                    return;

                digitalToAnalogOn = value;
                OnPropertyChanged();
                throttledDigitalToAnalogChanged.OnNext(true);
            }
        }


        private byte digitalToAnalogOff;
        public byte DigitalToAnalogOff
        {
            get => digitalToAnalogOff;
            set
            {
                if (actionType != MassiveKnobActionType.OutputAnalog || value == digitalToAnalogOff)
                    return;

                digitalToAnalogOff = value;
                OnPropertyChanged();
                throttledDigitalToAnalogChanged.OnNext(true);
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
            {
                DigitalToAnalogOn = 100;
                return;
            }


            bool AllowAction(ActionViewModel actionViewModel)
            {
                if (actionViewModel.RepresentsNull)
                    return true;
                        
                if (actionViewModel.Action.ActionType == actionType)
                    return true;

                // Allow digital actions to be assigned to analog outputs, extra conversion settings will be shown
                return actionType == MassiveKnobActionType.OutputAnalog &&
                       actionViewModel.Action.ActionType == MassiveKnobActionType.OutputDigital;
            }
            
            
            Actions = settingsViewModel.Actions.Where(AllowAction).ToList();

            var actionInfo = orchestrator.GetAction(actionType, index);
            
            selectedAction = actionInfo != null
                ? Actions.SingleOrDefault(a => !a.RepresentsNull && a.Action.ActionId == actionInfo.Info.ActionId)
                : Actions.Single(a => a.RepresentsNull);

            actionSettingsControl = actionInfo?.Instance.CreateSettingsControl();


            if (actionType != MassiveKnobActionType.OutputAnalog) 
                return;
            
            var digitalToAnalogSettings = orchestrator.GetDigitalToAnalogSettings(index);
            digitalToAnalogOn = digitalToAnalogSettings.OnValue;
            digitalToAnalogOff = digitalToAnalogSettings.OffValue;

            digitalToAnalogChangedSubscription = throttledDigitalToAnalogChanged
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Subscribe(b =>
                {
                    orchestrator?.UpdateDigitalToAnalogSettings(index, settings =>
                    {
                        settings.OnValue = digitalToAnalogOn;
                        settings.OffValue = digitalToAnalogOff;
                    });
                });
        }


        public void Dispose()
        {
            if (ActionSettingsControl is IDisposable disposable)
                disposable.Dispose();

            digitalToAnalogChangedSubscription?.Dispose();
            throttledDigitalToAnalogChanged.Dispose();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnDependantPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class InputOutputViewModelDesignTime : InputOutputViewModel
    {
        public InputOutputViewModelDesignTime() : base(null, null, MassiveKnobActionType.OutputAnalog, 0)
        {
        }
    }
}
