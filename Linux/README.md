# MassiveKnob - Linux GUI

This is a WIP port of the MassiveKnob Windows application to Rust. It may also run on Windows and is intended to replace the Windows version eventually so only one needs to be maintained. However, since this is my first proper project in Rust I am focusing on getting it to work on Linux first. I use NixOS btw. ;-)


## USB device access
If you're getting an access denied when accessing the USB port you should configure user access. On my system this is handled by udev, for which configuration is usually in ```/etc/udev/rules.d/```. The corresponding NixOS configuration is as follows:

```
  # Specific device
  services.udev.extraRules = ''
    KERNEL=="ttyACM1", MODE:="666"
  '';

  # Any Arduino Leonardo (check dmesg or lsusb for specs)
  services.udev.extraRules = ''
    ACTION=="add", SUBSYSTEMS=="usb", ATTRS{idVendor}=="2341", ATTRS{idProduct}=="8036", MODE="0666"
  '';
```


## Developing
### NixOS / Nix
Use the 'Nix Environment Selector' VSCode extension to get the dev dependencies for Rust Analyzer to work. In the terminal, use ```nix-shell``` to get the same environment for ```cargo run```.