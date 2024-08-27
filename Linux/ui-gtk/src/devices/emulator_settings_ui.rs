use std::cell::RefCell;
use std::rc::Rc;

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

    fn init(_root: &Self::Root, _widgets: &Rc<Self::Widgets>, _state: &Rc<RefCell<Self::State>>)
    {
    }
}


impl UiComponentState<EmulatorSettingsUi> for EmulatorSettingsUi
{
    fn new(_init: EmulatorSettingsUiInit) -> Self
    {
        Self
        {
        }
    }
}