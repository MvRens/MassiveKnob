pub mod uicomponent;


#[deprecated]
pub trait EmbeddedWidgetConnector
{
    fn root(&self) -> gtk::Widget;
}