use gtk::glib::clone;
use gtk::prelude::*;
use relm4::prelude::*;

use crate::orchestrator::Orchestrator;
use crate::registry::RegistryItem;
use crate::ui::EmbeddedWidgetConnector;
use crate::util::unique_id::UniqueId;

#[tracker::track]
pub struct MainWindow
{
    #[do_not_track]
    orchestrator: Orchestrator,

    #[do_not_track]
    devices_sorted: Vec<SortedDevice>,

    #[no_eq]
    device_settings_widget: Option<Box<dyn EmbeddedWidgetConnector>>
}


#[derive(Debug)]
pub enum MainWindowMsg
{
    DeviceInitial(usize),
    DeviceChanged(usize)
}


pub struct MainWindowWidgets
{
    device: MainWindowDeviceWidgets
}


pub struct MainWindowDeviceWidgets
{
    settings_container: gtk::Box
}


impl SimpleComponent for MainWindow
{
    type Init = ();
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


    fn init(_data: Self::Init, window: Self::Root, sender: ComponentSender<Self>) -> ComponentParts<Self>
    {
        let orchestrator = Orchestrator::new();

        let mut devices_sorted: Vec<SortedDevice> = orchestrator.devices()
            .map(|device| SortedDevice
            {
                unique_id: device.unique_id.clone(),
                name: device.name()
            })
            .collect();

        devices_sorted.sort_by(|a, b| a.name.to_lowercase().cmp(&b.name.to_lowercase()));


        let model = MainWindow
        {
            orchestrator,
            devices_sorted,

            device_settings_widget: None,

            tracker: 0
        };

        let widgets = MainWindowBuilder::new(&window, &model, &sender).build();

        ComponentParts { model, widgets }
    }


    fn update(&mut self, msg: Self::Input, _sender: ComponentSender<Self>)
    {
        match msg
        {
            MainWindowMsg::DeviceInitial(index) => self.apply_device_settings_widget(index, true),
            MainWindowMsg::DeviceChanged(index) => self.apply_device_settings_widget(index, false)
        }
    }


    fn update_view(&self, widgets: &mut Self::Widgets, _sender: ComponentSender<Self>)
    {
        if self.changed(MainWindow::device_settings_widget())
        {
            let current_child = widgets.device.settings_container.last_child();
            if let Some(current_child) = &current_child
            {
                widgets.device.settings_container.remove(current_child);
            }

            if let Some(new_child) = &self.device_settings_widget
            {
                widgets.device.settings_container.append(new_child.as_ref().root());
            }
        }
    }
}


impl MainWindow
{
    fn apply_device_settings_widget(&mut self, index: usize, initial: bool)
    {
        let mut widget = None;
        {
            if initial
            {
                self.orchestrator.with_active_device(|device_instance, cookie| { widget = device_instance.create_settings_widget(cookie) });
            }
            else
            {
                let device_id = self.devices_sorted[index].unique_id.clone();
                self.orchestrator.set_active_device_id(&device_id, |device_instance, cookie| { widget = device_instance.create_settings_widget(cookie) });
            }
        }

        self.set_device_settings_widget(widget)
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
            device: self.init_device_tab(&tabs)
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



        let devices_combobox = gtk::ComboBoxText::builder().build();
        tab.append(&devices_combobox);


        let active_device_id = self.model.orchestrator.active_device_id();
        for (index, device) in self.model.devices_sorted.iter().enumerate()
        {
            devices_combobox.append_text(device.name.as_str());

            if let Some(device_id) = &active_device_id
            {
                if *device_id == device.unique_id
                {
                    devices_combobox.set_active(Some(index as u32));
                    sender.input(MainWindowMsg::DeviceInitial(index));
                }
            }
        }


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


        let settings_container = gtk::Box::builder().build();
        tab.append(&settings_container);


        MainWindowDeviceWidgets
        {
            settings_container
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


#[derive(Clone)]
struct SortedDevice
{
    unique_id: UniqueId,
    name: String
}