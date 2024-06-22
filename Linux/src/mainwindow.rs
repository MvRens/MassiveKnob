use std::rc::Rc;
use gtk::prelude::*;
use relm4::prelude::*;

use crate::orchestrator::Orchestrator;

pub struct MainWindow
{ 
}


pub struct MainWindowViewModel
{
    pub orchestrator: Rc<Orchestrator>
}


impl std::fmt::Debug for MainWindowViewModel
{
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result 
    {
        f.debug_struct("MainWindowViewModel")
            // Skip orchestrator
            .finish()
    }
}


#[derive(Debug)]
pub enum MainWindowMsg
{
}


pub struct MainWindowWidgets
{
}


impl SimpleComponent for MainWindow
{
    type Init = MainWindowViewModel;
    type Input = MainWindowMsg;
    type Output = ();
    type Root = gtk::Window;
    type Widgets = MainWindowWidgets;


    fn init_root() -> Self::Root
    {
        // I prefer not to use the view! macro, as VSCode / rust-analyzer will not provide autocompletion
        gtk::Window::builder()
            .title(t!("mainwindow.title"))
            .default_width(500)
            .default_height(500)
            .build()   
    }


    fn init(_data: Self::Init, window: Self::Root, _sender: ComponentSender<Self>, ) -> ComponentParts<Self> 
    {
        let model = MainWindow {};

        let tabs = gtk::Notebook::builder().build();
        window.set_child(Some(&tabs));

        add_box_tab(&tabs, "mainwindow.tab.device");
        add_box_tab(&tabs, "mainwindow.tab.analoginputs");
        add_box_tab(&tabs, "mainwindow.tab.digitalinputs");
        //add_box_tab(&tabs, "mainwindow.tab.analogoutputs");
        //add_box_tab(&tabs, "mainwindow.tab.digitaloutputs");


        let widgets = MainWindowWidgets {};
        ComponentParts { model, widgets }
    }


    fn update(&mut self, msg: Self::Input, _sender: ComponentSender<Self>) 
    {
        match msg 
        {
        }
    }    
}


fn add_box_tab(notebook: &gtk::Notebook, title_key: &str) -> gtk::Box
{
    let tab = gtk::Box::builder()
        .orientation(gtk::Orientation::Vertical)
        .build();

    let tab_label = gtk::Label::builder()
        .label(t!(title_key))
        .build();

    notebook.append_page(&tab, Some(&tab_label));

    tab
}