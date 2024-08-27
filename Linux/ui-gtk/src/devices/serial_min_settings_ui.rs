use std::cell::RefCell;
use std::rc::Rc;

use gtk::glib;
use gtk::glib::clone;
use gtk::prelude::*;
use gtk::StringList;
use massiveknob_backend::orchestrator::DeviceReference;
use crate::ui::uicomponent::UiComponent;
use crate::ui::uicomponent::UiComponentState;


pub struct SerialMinSettingsUi
{
    ports: Vec<String>
}


pub struct SerialMinSettingsUiInit
{
    pub device: DeviceReference
}


pub struct SerialMinSettingsUiWidgets
{
    port_select: gtk::DropDown,
    custom_port_input: gtk::Entry
}



impl UiComponent for SerialMinSettingsUi
{
    type Root = gtk::Box;
    type Widgets = SerialMinSettingsUiWidgets;
    type Init = SerialMinSettingsUiInit;
    type State = Self;


    fn build_root(_init: &Self::Init) -> Self::Root
    {
        gtk::Box::builder()
            .hexpand(true)
            .orientation(gtk::Orientation::Vertical)
            .spacing(8)
            .build()
    }

    fn build_widgets(root: &Self::Root, _init: &Self::Init) -> Self::Widgets
    {
        let port_label = gtk::Label::builder()
            .label(t!("serial_min.settings.port.label"))
            .halign(gtk::Align::Start)
            .build();

        root.append(&port_label);


        let port_select = gtk::DropDown::builder()
            .build();

        root.append(&port_select);


        let custom_port_input = gtk::Entry::builder()
            .hexpand(true)
            .placeholder_text(t!("serial_min.settings.custom_port_placeholder"))
            .build();

        root.append(&custom_port_input);


        Self::Widgets
        {
            port_select,
            custom_port_input
        }
    }


    fn init(_root: &Self::Root, widgets: &Rc<Self::Widgets>, state: &Rc<RefCell<Self::State>>)
    {
        let port_model;
        {
            let state_borrowed = state.borrow();

            let port_model_vec: Vec<&str> = state_borrowed.ports.iter().map(|p| p.as_str()).collect();
            port_model = StringList::new(&port_model_vec);

            port_model.append(t!("serial_min.settings.port.custom").as_ref());

            widgets.port_select.set_model(Some(&port_model));
            widgets.port_select.set_selected(state_borrowed.ports.len().try_into().unwrap_or(gtk::ffi::GTK_INVALID_LIST_POSITION));
        }


        widgets.port_select.connect_selected_notify(clone!(
            #[weak]
            state,

            #[weak]
            widgets,

            move |_|
            {
                let active_index = widgets.port_select.selected();
                if active_index == gtk::ffi::GTK_INVALID_LIST_POSITION { return };

                let state = state.borrow();
                state.set_port(&widgets, active_index);
            }));

        /*
        widgets.custom_port_input.connect_changed(clone!(
            #[weak]
            state,

            #[weak(rename_to = custom_port_input)]
            widgets.custom_port_input,

            move |_|
            {
                let mut state = state.borrow_mut();
                state.set_custom_port(custom_port_input.text().into());
            }
        ));
        */
    }
}


impl UiComponentState<SerialMinSettingsUi> for SerialMinSettingsUi
{
    fn new(_init: SerialMinSettingsUiInit) -> Self
    {
        let ports_list = serialport::available_ports().unwrap_or_default();
        let ports: Vec<String> = ports_list.iter().map(|p| p.port_name.clone()).collect();


        // TODO read settings

        Self
        {
            ports
        }
    }
}


impl SerialMinSettingsUi
{
    fn set_port(&self, widgets: &Rc<SerialMinSettingsUiWidgets>, index: u32)
    {
        let Ok(index_usize) = usize::try_from(index) else { return };
        let custom_port_visible = index_usize == self.ports.len();

        widgets.custom_port_input.set_visible(custom_port_visible);
    }
}


/*
impl SimpleComponent for SerialMinSettingsWidget
{
    type Init = SerialMinSettingsInit;
    type Input = SerialMinSettingsWidgetMessage;
    type Output = ();
    type Root = gtk::Box;
    type Widgets = SerialMinSettingsWidgets;


    fn init_root() -> Self::Root
    {
        gtk::Box::builder()
            .hexpand(true)
            .orientation(gtk::Orientation::Vertical)
            .spacing(8)
            .build()
    }


    fn init(_data: Self::Init, root: Self::Root, sender: ComponentSender<Self>, ) -> ComponentParts<Self>
    {

    }


    fn update(&mut self, msg: Self::Input, _sender: ComponentSender<Self>)
    {
        match msg
        {
            SerialMinSettingsWidgetMessage::PortChanged(index) =>
            {
                self.set_custom_port_visible(index >= self.ports.len());
            },

            SerialMinSettingsWidgetMessage::CustomPortChanged(value) =>
            {
                self.set_custom_port(value);
            }
        }
    }


    fn update_view(&self, widgets: &mut Self::Widgets, _sender: ComponentSender<Self>)
    {
        if self.changed_custom_port_visible()
        {
            log::info!("Visible: {}", self.custom_port_visible);
            widgets.custom_port_input.set_visible(self.custom_port_visible);
        }

        if self.changed_custom_port()
        {
            // should this sync two-way or not?
            //widgets
        }
    }
}


impl EmbeddedWidgetConnector for Connector<SerialMinSettingsWidget>
{
    fn root(&self) -> &relm4::gtk::Widget
    {
        self.widget().as_ref()
    }
} */