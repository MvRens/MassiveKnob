//use emulatorwindow::EmulatorWindow;
//use relm4::prelude::*;
//use relm4::gtk::prelude::GtkApplicationExt;


use super::registry::{MkDevice, register_device};


pub mod emulatorwindow;


pub fn register()
{
    register_device(MkDevice::new("Emulator"));
}

/*
    let app = relm4::main_application();
    let builder = EmulatorWindow::builder();
    app.add_window(&builder.root);

    builder.launch(()).detach_runtime();
*/