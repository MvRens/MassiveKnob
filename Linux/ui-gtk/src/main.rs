use std::sync::Arc;
use std::sync::Mutex;

use env_logger::Env;
use gtk::glib;
use gtk::prelude::*;
use mainwindow::MainWindow;
use mainwindow::MainWindowInit;
use massiveknob_backend::orchestrator::Orchestrator;
use ui::uicomponent::UiComponent;


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


    let app = gtk::Application::builder()
        .application_id(APP_ID)
        .build();

    app.connect_activate(activate);
    app.run()
}


fn activate(app: &gtk::Application)
{
    let orchestrator = Arc::new(Mutex::new(Orchestrator::new()));
    let mainwindow = MainWindow::builder().build(MainWindowInit
    {
        app: app.clone(),
        orchestrator: orchestrator.clone()
    });

    mainwindow.root.present();
}