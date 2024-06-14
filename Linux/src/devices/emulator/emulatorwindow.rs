use gtk::prelude::*;
use relm4::prelude::*;

pub struct EmulatorWindow
{ 
}


#[derive(Debug)]
pub enum Msg
{
}


#[relm4::component(pub)]
impl SimpleComponent for EmulatorWindow
{
    type Init = ();
    type Input = Msg;
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
        ComponentParts { model, widgets }
    }


    fn update(&mut self, msg: Self::Input, _sender: ComponentSender<Self>) 
    {
        match msg 
        {
        }
    }    
}