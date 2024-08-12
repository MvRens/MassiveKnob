use gtk::StringList;
use gtk::glib::clone;
use relm4::component::Connector;
use relm4::prelude::*;
use relm4::gtk::prelude::*;
use serialport::SerialPortInfo;

use crate::ui::EmbeddedWidgetConnector;


#[tracker::track]
pub struct SerialMinSettingsWidget
{
    #[do_not_track]
    ports: Vec<String>,

    custom_port: String,
    custom_port_visible: bool
}


#[derive(Debug)]
pub enum SerialMinSettingsWidgetMessage
{
    PortChanged(usize),
    CustomPortChanged(String)
}


pub struct SerialMinSettingsInit
{
    // this needs a good design - we need to be able to modify the device instance, but
    // we can't pass the reference or an Rc to the device due to the design of create_settings_widget.
    // Either seperate the UI from the Device, or pass a middle man (sender/receiver style perhaps?)
}


pub struct SerialMinSettingsWidgets
{
    custom_port_input: gtk::Entry
}


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
        let port_label = gtk::Label::builder()
            .label(t!("serial_min.settings.port.label"))
            .halign(gtk::Align::Start)
            .build();

        root.append(&port_label);


        let ports_list = serialport::available_ports().unwrap_or(Vec::<SerialPortInfo>::new());
        let ports: Vec<String> = ports_list.iter().map(|p| p.port_name.clone()).collect();

        let port_model_vec: Vec<&str> = ports.iter().map(|p| p.as_str()).collect();
        let port_model = StringList::new(&port_model_vec);

        port_model.append(t!("serial_min.settings.port.custom").as_ref());

        let port_select = gtk::DropDown::builder()
            .model(&port_model)
            .selected(ports.len().try_into().unwrap_or(gtk::ffi::GTK_INVALID_LIST_POSITION))
            .build();

        root.append(&port_select);


        let port_select_cloned = port_select.clone();
        port_select.connect_selected_notify(clone!(
            @strong sender => move |_|
            {
                let active_index = port_select_cloned.selected();
                if active_index == gtk::ffi::GTK_INVALID_LIST_POSITION { return };

                if let Ok(active_index_usize) = usize::try_from(active_index)
                {
                    sender.input(SerialMinSettingsWidgetMessage::PortChanged(active_index_usize));
                }
            }));


        let custom_port_input = gtk::Entry::builder()
            .hexpand(true)
            .placeholder_text(t!("serial_min.settings.custom_port_placeholder"))
            .build();

        root.append(&custom_port_input);


        let custom_port_input_cloned = custom_port_input.clone();
        custom_port_input.connect_changed(clone!(
            @strong sender => move |_|
            {
                sender.input(SerialMinSettingsWidgetMessage::CustomPortChanged(String::from(custom_port_input_cloned.text().as_str())));
            }
        ));


        let model = SerialMinSettingsWidget
        {
            ports,

            custom_port: String::new(),
            custom_port_visible: true,

            tracker: 0
        };


        // TODO load settings


        let widgets = SerialMinSettingsWidgets
        {
            custom_port_input
        };

        ComponentParts { model, widgets }
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
}