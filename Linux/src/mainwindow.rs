use std::rc::Rc;
use gtk::prelude::*;
use relm4::prelude::*;

use crate::orchestrator::Orchestrator;

pub struct MainWindow
{ 
}


pub struct MainWindowViewModel
{
    pub orchestrator: Rc<Orchestrator>
}


impl std::fmt::Debug for MainWindowViewModel
{
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result 
    {
        f.debug_struct("MainWindowViewModel")
            // Skip orchestrator
            .finish()
    }
}


#[derive(Debug)]
pub enum MainWindowMsg
{
}


pub struct MainWindowWidgets
{
    device: MainWindowDeviceWidgets
}


pub struct MainWindowDeviceWidgets
{
    devices_combobox: gtk::ComboBoxText
}


impl SimpleComponent for MainWindow
{
    type Init = MainWindowViewModel;
    type Input = MainWindowMsg;
    type Output = ();
    type Root = gtk::Window;
    type Widgets = MainWindowWidgets;


    fn init_root() -> Self::Root
    {
        // I prefer not to use the view! macro, as VSCode / rust-analyzer will not provide autocompletion
        gtk::Window::builder()
            .title(t!("mainwindow.title"))
            .default_width(500)
            .default_height(500)
            .build()   
    }


    fn init(_data: Self::Init, window: Self::Root, _sender: ComponentSender<Self>, ) -> ComponentParts<Self> 
    {
        let model = MainWindow {};
        let widgets = Self::init_ui(&window);

        ComponentParts { model, widgets }
    }


    fn update(&mut self, msg: Self::Input, _sender: ComponentSender<Self>) 
    {
        match msg 
        {
        }
    }    
}



impl MainWindow
{
    fn init_ui(window: &gtk::Window) -> MainWindowWidgets
    {
        let tabs = gtk::Notebook::builder().build();
        window.set_child(Some(&tabs));

        MainWindowWidgets
        {
            device: Self::init_device_tab(&tabs)
            //Self::new_box_tab(&tabs, "mainwindow.tab.analoginputs");
            //Self::new_box_tab(&tabs, "mainwindow.tab.digitalinputs");
            //Self::add_box_tab(&tabs, "mainwindow.tab.analogoutputs");
            //Self::add_box_tab(&tabs, "mainwindow.tab.digitaloutputs");
        }
    }


    fn init_device_tab(tabs: &gtk::Notebook) -> MainWindowDeviceWidgets
    {
        let tab = Self::new_box_tab(&tabs, "mainwindow.tab.device");

        let label = gtk::Label::builder()
            .label(t!("mainwindow.deviceType.label"))
            .halign(gtk::Align::Start)
            .build();

        tab.append(&label);



        let devices_combobox = gtk::ComboBoxText::builder()                                
            .build();

        tab.append(&devices_combobox);


        // TEMP
        devices_combobox.append_text("Test");
        devices_combobox.append_text("Test 2");


        MainWindowDeviceWidgets
        {
            devices_combobox
        }
    }

    
    fn new_box_tab(notebook: &gtk::Notebook, title_key: &str) -> gtk::Box
    {
        let tab = gtk::Box::builder()
            .orientation(gtk::Orientation::Vertical)
            .spacing(8)
            .margin_start(8)
            .margin_end(8)
            .margin_top(8)
            .margin_bottom(8)
            .build();

        let tab_label = gtk::Label::builder()
            .label(t!(title_key))
            .build();

        notebook.append_page(&tab, Some(&tab_label));

        tab
    }
}