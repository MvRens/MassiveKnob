﻿using System;

namespace MassiveKnob.Plugin
{
    /// <summary>
    /// Determines the type of control this action can be assigned to.
    /// </summary>
    [Flags]
    public enum MassiveKnobActionType
    {
        /// <summary>
        /// Can be assigned to a potentiometer. The action instance must implement IMassiveKnobAnalogAction.
        /// </summary>
        InputAnalog = 1 << 0,
        
        /// <summary>
        /// Can be assigned to a button or switch. The action instance must implement IMassiveKnobDigitalAction.
        /// </summary>
        InputDigital = 1 << 1,
        
        /// <summary>
        /// Can be assigned to an output, like an LED or relay.
        /// </summary>
        OutputSignal = 1 << 2
    }
    
    
    /// <summary>
    /// Provides information about an action which can be assigned to a knob or button.
    /// </summary>
    public interface IMassiveKnobAction
    {
        /// <summary>
        /// Unique identifier for the action.
        /// </summary>
        Guid ActionId { get; }

        /// <summary>
        /// Determines the type of control this action can be assigned to.
        /// </summary>
        MassiveKnobActionType ActionType { get; }

        /// <summary>
        /// The name of the action as shown in the action list when assigning to a knob or button.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A short description of the functionality provided by the action.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Called when an action is bound to a knob or button to create an instance of the action.
        /// </summary>
        /// <param name="context">Provides an interface to the Massive Knob settings and device. Can be stored until the action instance is disposed.</param>
        IMassiveKnobActionInstance Create(IMassiveKnobActionContext context);
    }
}
