use gtk::prelude::*;
use relm4::prelude::*;

pub struct MainWindow
{ 
}


#[derive(Debug)]
pub enum Msg
{
}


#[relm4::component(pub)]
impl SimpleComponent for MainWindow
{
    type Init = ();
    type Input = Msg;
    type Output = ();

    view!
    {        
        gtk::Window
        {            
            set_title: Some(&t!("mainwindow.title")),
            set_default_size: (300, 100)
        }
    }


    fn init(_data: Self::Init, root: Self::Root, _sender: ComponentSender<Self>, ) -> ComponentParts<Self> 
    {
    // TEMP - device should not use GTK at register time
    crate::devices::register();

        let model = MainWindow {};

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