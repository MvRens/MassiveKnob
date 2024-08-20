// Much credit to Relm4 for the inspiration. I initially used it, but felt
// like I wasn't using much of the abstractions it provided. But some parts
// are still very useful and I recommend giving it a try for your project!

use gtk::prelude::*;
use std::cell::RefCell;
use std::marker::PhantomData;
use std::rc::Rc;


pub trait UiComponent : Sized
{
    type Root : IsA<gtk::Widget>;
    type Widgets;
    type Init;
    type State : UiComponentState<Self>;


    fn builder() -> UiComponentBuilder<Self>
    {
        UiComponentBuilder::<Self>::default()
    }


    fn build_root(init: &Self::Init) -> Self::Root;
    fn build_widgets(root: &Self::Root, init: &Self::Init) -> Self::Widgets;

    fn init(root: &Self::Root, widgets: &Rc<Self::Widgets>, state: &Rc<RefCell<Self::State>>);
}


pub trait UiComponentState<C: UiComponent>
{
    fn new(init: C::Init) -> Self;
}


pub struct UiComponentConnector<C: UiComponent>
{
    pub root: C::Root,
    pub state: Rc<RefCell<C::State>>
}


pub trait UiComponentConnectorWidget
{
    fn root(&self) -> gtk::Widget;
}


pub struct UiComponentBuilder<C: UiComponent>
{
    marker: PhantomData<C>
}


impl<C: UiComponent> Default for UiComponentBuilder<C>
{
    fn default() -> Self
    {
        Self
        {
            marker: PhantomData::<C>
        }
    }
}


impl<C: UiComponent> UiComponentBuilder<C>
{
    pub fn build(&self, init: C::Init) -> UiComponentConnector<C>
    {
        let root = C::build_root(&init);
        let widgets = Rc::new(C::build_widgets(&root, &init));
        let state = Rc::new(RefCell::new(C::State::new(init)));

        C::init(&root, &widgets, &state);


        UiComponentConnector::<C>
        {
            root,
            state
        }
    }
}


impl<C: UiComponent> From<UiComponentConnector<C>> for gtk::Widget
{
    fn from(val: UiComponentConnector<C>) -> Self
    {
        val.root.into()
    }
}


impl<C: UiComponent> UiComponentConnectorWidget for UiComponentConnector<C>
{
    fn root(&self) -> gtk::Widget
    {
        self.root.clone().into()
    }
}