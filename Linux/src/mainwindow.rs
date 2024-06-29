use std::rc::Rc;
use gtk::prelude::*;
use relm4::prelude::*;

use crate::{devices::MkDevice, orchestrator::Orchestrator, registry::RegistryItem, util::unique_id::UniqueId};

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
    devices_sorted: Vec<SortedDevice>,
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


    fn init(data: Self::Init, window: Self::Root, _sender: ComponentSender<Self>, ) -> ComponentParts<Self> 
    {
        let orchestrator = data.orchestrator.as_ref();

        let model = MainWindow {};
        let widgets = Self::init_ui(&window, &orchestrator);

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
    fn init_ui(window: &gtk::Window, orchestrator: &Orchestrator) -> MainWindowWidgets
    {
        let tabs = gtk::Notebook::builder().build();
        window.set_child(Some(&tabs));

        MainWindowWidgets
        {
            device: Self::init_device_tab(&tabs, &orchestrator)
            //Self::new_box_tab(&tabs, "mainwindow.tab.analoginputs");
            //Self::new_box_tab(&tabs, "mainwindow.tab.digitalinputs");
            //Self::add_box_tab(&tabs, "mainwindow.tab.analogoutputs");
            //Self::add_box_tab(&tabs, "mainwindow.tab.digitaloutputs");
        }
    }


    fn init_device_tab(tabs: &gtk::Notebook, orchestrator: &Orchestrator) -> MainWindowDeviceWidgets
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


        let mut devices_sorted: Vec<SortedDevice> = orchestrator.devices()
            .map(|device| SortedDevice
            {
                unique_id: device.unique_id(),
                name: device.name()
            })
            .collect();

        devices_sorted.sort_by(|a, b| a.name.to_lowercase().cmp(&b.name.to_lowercase()));

        let current_device_id = orchestrator.current_device_id();


        for (index, device) in devices_sorted.iter().enumerate()
        {
            devices_combobox.append_text(device.name.as_str());

            if let Some(device_id) = current_device_id
            {
                if device_id == device.unique_id
                {
                    devices_combobox.set_active(Some(index as u32));
                }
            }
        }


        MainWindowDeviceWidgets
        {
            devices_sorted,
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


struct SortedDevice
{
    unique_id: UniqueId,
    name: String
}