
# Massive Knob

Control audio devices using physical knobs. And more.

Inspired by [an article on Prusa's blog](https://blog.prusaprinters.org/3d-print-an-oversized-media-control-volume-knob-arduino-basics_30184/), this project has a slightly different set of goals. The original requirements were:

1. Control volume using a potentiometer (fixed position) instead of a rotary encoder (endless rotation)
2. Control specific audio devices, not the current default device
3. Provide means of switching the default device by pressing a button

Because of these requirements, a simple media keys HID device does not suffice and extra software is required on the desktop. This opens up a range of possibilities.

## Features

 - 🔊 Set the volume for specific devices / send the current volume to an analog output
 - 🔇 Mute / unmute specific devices / send the muted state to a digital output (*e.g. LED*)
 - 🎧 Set the default device / set a digital output based on the default device
 - 💬 Optional OSD (On-Screen Display)
 - 🔌 VoiceMeeter (Standard, Banana & Potato) plugin to execute macros or read the current state


Massive Knob is basically a host for plugins. A plugin can implement a device or actions which either process signals from the device to perform an action (for example, change the volume when a knob is turned) or send signals to the device based on the system state (for example, light up an LED to indicate the default device).

### Devices
A device can provide the following inputs and outputs, up to 255 for each type (unless you're Look Mum No Computer I assume this will be enough):

1. Analog input (*e.g. a potentiometer*)
2. Digital input (*e.g. a button or switch*)
3. Analog output (*e.g. a PWM output, though not yet supported by the reference Arduino implementation*)
4. Digital output (*e.g. an LED*)

#### Serial
Connects to a compatible device on a Serial port, probably a USB device like an Arduino. The device must implement the Massive Knob protocol which uses the [MIN protocol](https://github.com/min-protocol/min) to send and receive frames. An Arduino Sketch is included with this repository which can be customized to suit your hardware layout.

#### Emulator
Useful for development, this one emulates an actual device. The number of inputs and outputs are configurable, a popup allows changing the inputs and shows the state of the outputs.


## Developing
The hardware side uses an Arduino sketch to communicate the hardware state over the serial port.

The Windows software is written in C# using .NET Framework 4.7.2 and Visual Studio 2019.

Refer to the bundled plugins for examples.

Some icons courtesy of https://feathericons.com/