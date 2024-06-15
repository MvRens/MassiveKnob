use env_logger::Env;
use relm4::prelude::*;

#[macro_use]
extern crate rust_i18n;

i18n!("locales");


pub mod devices;
pub mod actions;

pub mod config;
pub mod mainwindow;

fn main()
{
    env_logger::Builder::from_env(Env::default().default_filter_or("info"))
//        .format_timestamp(None)
        .init();

    devices::register();
    actions::register();

    relm4_icons::initialize_icons();

    load_config();

    let app = RelmApp::new("com.github.mvrens.massiveknob");
    app.run::<mainwindow::MainWindow>(());
}


fn load_config()
{
    //let config = config::Config::new();
    //config.get_reader(name)
}