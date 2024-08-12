pub trait EmbeddedWidgetConnector
{
    fn root(&self) -> &relm4::gtk::Widget;
}