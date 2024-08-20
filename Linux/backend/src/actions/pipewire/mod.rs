use crate::util::unique_id::UniqueId;

use super::ActionGroupInfo;

pub mod set_volume;



pub enum PipewireAction
{
    SetVolume(set_volume::PipewireSetVolumeActionInfo)
}


pub struct PipewireActionGroupInfo
{
}


impl ActionGroupInfo for PipewireActionGroupInfo
{
    fn unique_id(&self) -> UniqueId { UniqueId::from("pipewire") }
}