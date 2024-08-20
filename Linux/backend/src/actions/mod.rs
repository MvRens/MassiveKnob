pub mod pipewire;


use crate::util::unique_id::UniqueId;


pub enum ActionGroup
{
    Pipewire(pipewire::PipewireActionGroupInfo)
}



pub trait ActionGroupInfo
{
    /// The unique ID of the group. This should remain stable across releases for
    /// the purpose of storing it in the user's configuration.
    fn unique_id(&self) -> UniqueId;

    /// The name of the group for display purposes.
    fn name(&self) -> String
    {
        t!(format!("actions.{}.name", self.unique_id().as_str()).as_str()).to_string()
    }
}


pub trait ActionInfo
{
    /// The unique ID of the item. This should remain stable across releases for
    /// the purpose of storing it in the user's configuration.
    fn unique_id(&self) -> UniqueId;

    /// The unique ID of the group this action belongs to.
    fn group_id(&self) -> UniqueId;

    /// The name of the item for display purposes.
    fn name(&self) -> String
    {
        t!(format!("actions.{}.{}.name", self.group_id().as_str(), self.unique_id().as_str()).as_str()).to_string()
    }
}
