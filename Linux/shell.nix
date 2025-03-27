{ pkgs ? import <nixpkgs> {} }:
  pkgs.mkShell {
    nativeBuildInputs = with pkgs; [
        pkg-config
        gtk4
        graphene
        gdk-pixbuf
        libudev-zero
        libpulseaudio
    ];

    # For a reason I'm yet to find out, my VSCode terminal sets GDK_BACKEND to x11.
    # This is empty in the regular terminal and I'm running wayland.
    # Comment this out if it's causing issues for you.
    GDK_BACKEND = "wayland";
  }