use massiveknob_backend::orchestrator::DeviceReference;

use crate::ui::uicomponent::UiComponent;
use crate::ui::uicomponent::UiComponentState;


pub struct EmulatorSettingsUiInit
{
    pub device: DeviceReference
}



pub struct EmulatorSettingsUi
{
}


pub struct EmulatorSettingsUiWidgets
{

}


impl UiComponent for EmulatorSettingsUi
{
    type Root = gtk::Box;
    type Widgets = EmulatorSettingsUiWidgets;
    type Init = EmulatorSettingsUiInit;
    type State = Self;


    fn build_root(_init: &Self::Init) -> Self::Root
    {
        gtk::Box::builder()
            .hexpand(true)
            .orientation(gtk::Orientation::Vertical)
            .spacing(8)
            .build()
    }

    fn build_widgets(_root: &Self::Root, _init: &Self::Init) -> Self::Widgets
    {
        Self::Widgets
        {
        }
    }

    fn init(_root: &Self::Root, _state: &std::rc::Rc<std::cell::RefCell<Self::State>>)
    {
    }
}


impl UiComponentState<EmulatorSettingsUi> for EmulatorSettingsUi
{
    fn new(_init: (), _widgets: EmulatorSettingsUiWidgets) -> Self
    {
        Self
        {
        }
    }
}


/*
#[relm4::component(pub)]
impl SimpleComponent for EmulatorWindow
{
    type Init = ();
    type Input = EmulatorWindowMessage;
    type Output = ();

    view!
    {
        gtk::Window
        {
            set_title: Some(&t!("emulatorwindow.title")),
            set_default_size: (300, 500)
        }
    }


    fn init(_data: Self::Init, root: Self::Root, _sender: ComponentSender<Self>, ) -> ComponentParts<Self>
    {
        let model = EmulatorWindow {};
        let widgets = view_output!();

        root.set_visible(true);
        ComponentParts { model, widgets }
    }


    fn update(&mut self, msg: Self::Input, _sender: ComponentSender<Self>)
    {
        match msg
        {
        }
    }
}
     */