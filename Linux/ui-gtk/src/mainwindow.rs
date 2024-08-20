use std::cell::RefCell;
use std::rc::Rc;
use std::sync::Arc;
use std::sync::Mutex;

use gtk::glib;
use gtk::glib::clone;
use gtk::prelude::*;
use massiveknob_backend::orchestrator::Orchestrator;
use massiveknob_backend::util::unique_id::UniqueId;

use crate::devices::DeviceSettingsUiBuilder;
use crate::ui::uicomponent::UiComponent;
use crate::ui::uicomponent::UiComponentConnectorWidget;
use crate::ui::uicomponent::UiComponentState;



pub struct MainWindowInit
{
    pub app: gtk::Application,
    pub orchestrator: Arc<Mutex<Orchestrator>>
}


pub struct MainWindow
{
    orchestrator: Arc<Mutex<Orchestrator>>,
    devices_sorted: Vec<SortedDevice>,

    device_settings_widget: Option<Box<dyn UiComponentConnectorWidget>>
}


pub struct MainWindowWidgets
{
    device: MainWindowDeviceWidgets
}


pub struct MainWindowDeviceWidgets
{
    devices_dropdown: gtk::DropDown,
    settings_container: gtk::Box
}


impl UiComponent for MainWindow
{
    type Root = gtk::ApplicationWindow;
    type Widgets = MainWindowWidgets;
    type Init = MainWindowInit;
    type State = Self;


    fn build_root(init: &Self::Init) -> Self::Root
    {
        gtk::ApplicationWindow::builder()
            .application(&init.app)
            .title(t!("mainwindow.title"))
            .default_width(500)
            .default_height(500)
            .build()
    }

    fn build_widgets(root: &Self::Root, _init: &Self::Init) -> Self::Widgets
    {
        let tabs = gtk::Notebook::builder().build();
        root.set_child(Some(&tabs));

        Self::Widgets
        {
            device: Self::build_device_tab(&tabs)
        }
    }


    fn init(_root: &Self::Root, widgets: &Rc<Self::Widgets>, state: &Rc<RefCell<Self::State>>)
    {
        {
            let state_borrowed = state.borrow();
            let devices_dropdown = widgets.device.devices_dropdown.clone();
            let orchestrator = state_borrowed.orchestrator.lock().unwrap();

            let active_device_id = orchestrator.active_device_id();
            let mut active_device_index = gtk::ffi::GTK_INVALID_LIST_POSITION;
            let devices_dropdown_list = gtk::StringList::default();

            for (index, device) in state_borrowed.devices_sorted.iter().enumerate()
            {
                devices_dropdown_list.append(device.name.as_str());

                if let Some(device_id) = &active_device_id
                {
                    if *device_id == device.unique_id
                    {
                        active_device_index = index as u32;
                    }
                }
            }

            devices_dropdown.set_model(Some(&devices_dropdown_list));
            devices_dropdown.set_selected(active_device_index);

            devices_dropdown.connect_selected_notify(clone!(
                #[weak]
                state,

                #[weak]
                widgets,

                move |_|
                {
                    let mut state = state.borrow_mut();
                    state.update_active_device(&widgets, true);
                }
            ));
        }

        let mut state = state.borrow_mut();
        state.update_active_device(&widgets, false);
    }
}


impl UiComponentState<MainWindow> for MainWindow
{
    fn new(init: MainWindowInit) -> Self
    {
        let mut devices_sorted: Vec<SortedDevice>;
        {
            let orchestrator = init.orchestrator.lock().unwrap();

            devices_sorted = orchestrator.devices()
                .map(|device| SortedDevice
                {
                    unique_id: device.unique_id.clone(),
                    name: device.name()
                })
                .collect();

            devices_sorted.sort_by(|a, b| a.name.to_lowercase().cmp(&b.name.to_lowercase()));
        }


        Self
        {
            orchestrator: init.orchestrator.clone(),
            devices_sorted,

            device_settings_widget: None
        }
    }
}


impl MainWindow
{
    fn build_device_tab(tabs: &gtk::Notebook) -> MainWindowDeviceWidgets
    {
        //let sender = self.sender;
        let tab = Self::build_box_tab(tabs, "mainwindow.tab.device");

        let label = gtk::Label::builder()
            .label(t!("mainwindow.deviceType.label"))
            .halign(gtk::Align::Start)
            .build();

        tab.append(&label);


        let devices_dropdown = gtk::DropDown::builder().build();
        tab.append(&devices_dropdown);


        let settings_container = gtk::Box::builder().build();
        tab.append(&settings_container);


        MainWindowDeviceWidgets
        {
            devices_dropdown,
            settings_container
        }
    }


    fn build_box_tab(notebook: &gtk::Notebook, title_key: &str) -> gtk::Box
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


    fn update_active_device(&mut self, widgets: &Rc<MainWindowWidgets>, set_active: bool)
    {
        let active_index = widgets.device.devices_dropdown.selected();
        if active_index == gtk::ffi::GTK_INVALID_LIST_POSITION { return };

        let Ok(active_index_usize) = usize::try_from(active_index) else { return };
        let selected_device = &self.devices_sorted[active_index_usize];
        let device;

        {
            let mut orchestrator = self.orchestrator.lock().unwrap();

            if set_active
            {
                device = Some(orchestrator.set_active_device_id(&selected_device.unique_id));
            }
            else
            {
                device = orchestrator.active_device();
            }
        }

        if let Some(prev_widget) = &self.device_settings_widget
        {
            widgets.device.settings_container.remove(&prev_widget.root());
        }

        if let Some(device) = device
        {
            let widget = DeviceSettingsUiBuilder::build(device.clone());

            widgets.device.settings_container.append(&widget.root());
            self.device_settings_widget = Some(widget);
        }
        else
        {
            self.device_settings_widget = None;
        }
    }
}



#[derive(Clone)]
struct SortedDevice
{
    unique_id: UniqueId,
    name: String
}