use crate::actions::ActionInfo;
use crate::util::unique_id::UniqueId;

pub struct PipewireSetVolumeActionInfo
{
}


impl ActionInfo for PipewireSetVolumeActionInfo
{
    fn unique_id(&self) -> crate::util::unique_id::UniqueId { UniqueId::from("set_volume") }
    fn group_id(&self) -> crate::util::unique_id::UniqueId { UniqueId::from("pipewire") }
}