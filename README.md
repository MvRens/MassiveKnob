
# Massive Knob

Control audio devices using physical knobs.

Inspired by [an article on Prusa's blog](https://blog.prusaprinters.org/3d-print-an-oversized-media-control-volume-knob-arduino-basics_30184/), this project has a slightly different set of goals:

**Must have**
1. Control multiple audio devices, one set of physical controls per device
2. Volume is set to an absolute value (potentiometer instead of a rotary encoder)

Because of the second requirement, a simple media keys HID device does not suffice and extra software is required on the desktop.

**Nice to have**
1. Physical buttons to switch the active device
a. by changing the Windows default output device
b. by running a VoiceMeeter macro
2. Corresponding LEDs to indicate the currently active device

## Developing
The hardware side uses an Arduino sketch to communicate the hardware state over the serial port.

The Windows software is written in C# using .NET Framework 4.7.2 and Visual Studio 2019.