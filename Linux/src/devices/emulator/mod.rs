use crate::registry::MkRegistry;
use crate::util::unique_id::UniqueId;
use super::MkDevice;


pub mod emulatorwindow;




pub fn register(registry: &mut MkRegistry<MkDevice>)
{
    registry.register(MkDevice {
        unique_id: UniqueId::new("emulator")
    });
}

/*
    let app = relm4::main_application();
    let builder = EmulatorWindow::builder();
    app.add_window(&builder.root);

    builder.launch(()).detach_runtime();
*/