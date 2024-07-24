use std::cell::RefCell;
use std::rc::Rc;
use gtk::glib::clone;
use gtk::prelude::*;
use relm4::prelude::*;

use crate::orchestrator::Orchestrator;
use crate::registry::RegistryItem;
use crate::util::unique_id::UniqueId;

pub struct MainWindow
{
    orchestrator: Rc<RefCell<Orchestrator>>,
    devices_sorted: Vec<SortedDevice>
}


pub struct MainWindowInit
{
    pub orchestrator: Rc<RefCell<Orchestrator>>
}


impl std::fmt::Debug for MainWindowInit
{
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result
    {
        f.debug_struct("MainWindowInit")
            // Skip orchestrator
            .finish()
    }
}


#[derive(Debug)]
pub enum MainWindowMsg
{
    DeviceChanged(usize)
}


pub struct MainWindowWidgets
{
    _device: MainWindowDeviceWidgets
}


pub struct MainWindowDeviceWidgets
{
    _devices_combobox: gtk::ComboBoxText
}


impl SimpleComponent for MainWindow
{
    type Init = MainWindowInit;
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


    fn init(data: Self::Init, window: Self::Root, sender: ComponentSender<Self>) -> ComponentParts<Self>
    {
        {
            let mut init_orchestrator = data.orchestrator.borrow_mut();
            init_orchestrator.initialize();
        }


        let orchestrator = data.orchestrator.borrow();

        let mut devices_sorted: Vec<SortedDevice> = orchestrator.devices()
            .map(|device| SortedDevice
            {
                unique_id: device.unique_id(),
                name: device.name()
            })
            .collect();

        devices_sorted.sort_by(|a, b| a.name.to_lowercase().cmp(&b.name.to_lowercase()));


        let model = MainWindow
        {
            orchestrator: data.orchestrator.clone(),
            devices_sorted
        };

        let widgets = MainWindowBuilder::new(&window, &model, &sender).build();

        ComponentParts { model, widgets }
    }


    fn update(&mut self, msg: Self::Input, _sender: ComponentSender<Self>)
    {
        match msg
        {
            MainWindowMsg::DeviceChanged(index) =>
            {
                let mut orchestrator = self.orchestrator.borrow_mut();
                let device = &self.devices_sorted[index];

                orchestrator.set_current_device_id(device.unique_id.clone());
            }
        }
    }
}


struct MainWindowBuilder<'a>
{
    window: &'a gtk::Window,
    model: &'a MainWindow,
    sender: &'a ComponentSender<MainWindow>
}


impl<'a> MainWindowBuilder<'a>
{
    fn new(window: &'a gtk::Window, model: &'a MainWindow, sender: &'a ComponentSender<MainWindow>) -> Self
    {
        Self
        {
            window,
            model,
            sender
        }
    }


    fn build(&self) -> MainWindowWidgets
    {
        let tabs = gtk::Notebook::builder().build();
        self.window.set_child(Some(&tabs));

        MainWindowWidgets
        {
            _device: self.init_device_tab(&tabs)
            //Self::new_box_tab(&tabs, "mainwindow.tab.analoginputs");
            //Self::new_box_tab(&tabs, "mainwindow.tab.digitalinputs");
            //Self::add_box_tab(&tabs, "mainwindow.tab.analogoutputs");
            //Self::add_box_tab(&tabs, "mainwindow.tab.digitaloutputs");
        }
    }


    fn init_device_tab(&self, tabs: &gtk::Notebook) -> MainWindowDeviceWidgets
    {
        let sender = self.sender;
        let tab = Self::new_box_tab(&tabs, "mainwindow.tab.device");

        let label = gtk::Label::builder()
            .label(t!("mainwindow.deviceType.label"))
            .halign(gtk::Align::Start)
            .build();

        tab.append(&label);



        let devices_combobox = gtk::ComboBoxText::builder()
            .build();

        let devices_combobox_cloned = devices_combobox.clone();

        devices_combobox.connect_changed(clone!(
            @strong sender => move |_|
            {
                if let Some(active_index) = devices_combobox_cloned.active()
                {
                    if let Ok(active_index_usize) = usize::try_from(active_index)
                    {
                        sender.input(MainWindowMsg::DeviceChanged(active_index_usize));
                    }
                }
            }));

        tab.append(&devices_combobox);


        let current_device_id = self.model.orchestrator.borrow().current_device_id();


        for (index, device) in self.model.devices_sorted.iter().enumerate()
        {
            devices_combobox.append_text(device.name.as_str());

            if let Some(device_id) = current_device_id.clone()
            {
                if device_id == device.unique_id
                {
                    devices_combobox.set_active(Some(index as u32));
                }
            }
        }


        MainWindowDeviceWidgets
        {
            _devices_combobox: devices_combobox
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