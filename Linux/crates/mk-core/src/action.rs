use crate::types::AnalogValue;

pub trait Action
{
    fn display_name() -> String;
    fn description() -> String;
}


pub trait AnalogInputAction: Action
{
    fn update_analog(&self, value: AnalogValue) -> impl std::future::Future<Output = ()> + Send;
}


pub trait DigitalInputAction: Action
{
    fn update_digital(&self, value: bool) -> impl std::future::Future<Output = ()> + Send;
}


pub trait AnalogOutputAction<'a>: Action
{
    fn initialize(&self, context: &'a impl AnalogOutputContext);
}


pub trait DigitalOutputAction<'a>: Action
{
    fn initialize(&self, context: &'a impl DigitalOutputContext);
}



pub trait AnalogOutputContext
{
    fn set_analog(&self, value: AnalogValue) -> impl std::future::Future<Output = ()> + Send;
}


pub trait DigitalOutputContext
{
    fn set_digital(&self, value: bool) -> impl std::future::Future<Output = ()> + Send;
}