use env_logger::Env;
use relm4::prelude::*;

#[macro_use]
extern crate rust_i18n;

i18n!("locales");


pub mod util;
pub mod ui;
pub mod registry;
pub mod devices;
pub mod actions;

pub mod config;
pub mod orchestrator;
pub mod mainwindow;

fn main()
{
    env_logger::Builder::from_env(Env::default().default_filter_or("info"))
//        .format_timestamp(None)
        .init();

    relm4_icons::initialize_icons();

    let app = RelmApp::new("com.github.mvrens.massiveknob");
    app.run::<mainwindow::MainWindow>(());
}