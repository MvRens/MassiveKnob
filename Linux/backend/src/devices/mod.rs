pub mod emulator;
pub mod serial_min;


use emulator::EmulatorDevice;
use serial_min::SerialMinDevice;
use uuid::Uuid;

use crate::util::unique_id::UniqueId;

// TODO use a procedural macro to build the registry based on attributes in the Device enum?

pub enum Device
{
    Emulator(emulator::EmulatorDevice),
    SerialMin(serial_min::SerialMinDevice)
}


pub struct DeviceInfo
{
    /// The unique ID of the item. This should remain stable across releases for
    /// the purpose of storing it in the user's configuration.
    pub unique_id: UniqueId,

    pub factory: fn(DeviceContext, Uuid) -> Device
}


pub struct DeviceRegistry
{
    info: Vec<DeviceInfo>
}


pub struct DeviceContext
{
    // Arc<ConfigManager>
}


impl DeviceRegistry
{
    #![allow(clippy::new_without_default)]
    pub fn new() -> Self
    {
        Self
        {
            info: vec!(
                DeviceInfo
                {
                    unique_id: UniqueId::from("emulator"),
                    factory: |context, instance_id| Device::Emulator(EmulatorDevice::new(context, instance_id))
                },

                DeviceInfo
                {
                    unique_id: UniqueId::from("serial_min"),
                    factory: |context, instance_id| Device::SerialMin(SerialMinDevice::new(context, instance_id))
                }
            )
        }
    }


    pub fn iter(&self) -> impl Iterator<Item = &DeviceInfo>
    {
        self.info.iter()
    }


    pub fn by_id(&self, id: &UniqueId) -> Option<&DeviceInfo>
    {
        self.info.iter().find(|v| &v.unique_id == id)
    }
}



impl DeviceInfo
{
    /// The name of the item for display purposes.
    pub fn name(&self) -> String
    {
        t!(format!("devices.{}.name", self.unique_id.as_str()).as_str()).to_string()
    }
}
