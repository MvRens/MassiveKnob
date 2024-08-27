use std::cell::RefCell;
use std::rc::Rc;
use std::sync::Arc;
use std::sync::Mutex;

use env_logger::Env;
use gtk::glib;
use gtk::prelude::*;
use mainwindow::MainWindow;
use mainwindow::MainWindowInit;
use massiveknob_backend::orchestrator::Orchestrator;
use ui::uicomponent::UiComponent;
use ui::uicomponent::UiComponentConnector;


const APP_ID: &str = "com.github.mvrens.massiveknob";


#[macro_use]
extern crate rust_i18n;

i18n!("locales");


pub mod ui;
pub mod devices;

pub mod mainwindow;


fn main() -> glib::ExitCode
{
    env_logger::Builder::from_env(Env::default().default_filter_or("info"))
//        .format_timestamp(None)
        .init();


    let mainwindow: Rc<RefCell<Option<UiComponentConnector<MainWindow>>>> = Rc::new(RefCell::new(None));


    let app = gtk::Application::builder()
        .application_id(APP_ID)
        .build();

    {
        let mainwindow = mainwindow.clone();
        app.connect_activate(move |app|
        {
            let orchestrator = Arc::new(Mutex::new(Orchestrator::new()));
            let newmainwindow = MainWindow::builder().build("MainWindow", MainWindowInit
            {
                app: app.clone(),
                orchestrator: orchestrator.clone()
            });

            newmainwindow.root.present();
            mainwindow.borrow_mut().replace(newmainwindow);
        });
    }

    let result = app.run();

    log::debug!("So long and thanks for all the fish!");
    result
}