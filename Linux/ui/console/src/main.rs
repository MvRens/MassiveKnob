use core::panic;
use std::env;
use std::io::stdin;
use std::io::stdout;
use std::io::Write;

use crossbeam_channel::unbounded;

use mk_actions_pulseaudio::set_volume::SetVolumeActionSettings;
use mk_core::action::ActionFactory;
use mk_core::action::AnalogInputAction;
use mk_core::device::DeviceEventMessage;
use mk_core::device::DeviceFactory;
use mk_device_serialmin::device::SerialMinDevice;
use mk_device_serialmin::device::SerialMinDeviceSettings;
use mk_actions_pulseaudio::set_volume::SetVolumeAction;

#[tokio::main]
async fn main()
{
    colog::basic_builder()
        .filter(None, log::LevelFilter::Trace)
        .filter(Some("min_rs"), log::LevelFilter::Info)
        .filter(Some("serialmin"), log::LevelFilter::Info)
        .init();

    println!("Select serial port:");
    let port = get_port();

    println!();
    println!("Select audio device for analog input 1:");
    let output_device_1 = get_output_device().await;

    println!();
    println!("Select audio device for analog input 2:");
    let output_device_2 = get_output_device().await;


    log::info!("MassiveKnob starting for serial device on port {}", port);

    let (sender, receiver) = unbounded();
    let _device = SerialMinDevice::create(SerialMinDeviceSettings {
        port,
        baud_rate: 115200
    }, sender);


    let action_1 = SetVolumeAction::create(SetVolumeActionSettings
    {
        device_name: output_device_1
    });

    let action_2 = SetVolumeAction::create(SetVolumeActionSettings
    {
        device_name: output_device_2
    });

    log::info!("Waiting for events...");
    loop
    {
        if let Ok(event) = receiver.recv()
        {
            match event
            {
                DeviceEventMessage::Connected { specs } => log::info!("Connected: {:?}", specs),
                DeviceEventMessage::Disconnected => log::info!("Disconnected"),
                DeviceEventMessage::AnalogInput { input, value } =>
                {
                    match input
                    {
                        0 => action_1.update_analog(value).await,
                        1 => action_2.update_analog(value).await,
                        _ => {}
                    }

                    log::info!("Analog input #{}: {}", input, value);
                }

                DeviceEventMessage::DigitalInput { input, value } => log::info!("Digital input #{}: {}", input, value),
            }
        }
    }
}


fn get_port() -> String
{
    let args: Vec<String> = env::args().collect();
    if args.len() > 1 { return args[1].clone(); }

    let available_ports = SerialMinDevice::available_ports();

    for (i, port) in available_ports.iter().enumerate()
    {
        println!("[{}] {}", i, port.display_name);
    }

    let stdin = stdin();
    let mut stdout = stdout();
    let mut line = String::new();

    loop
    {
        println!();
        print!("Port: ");
        _ = stdout.flush();

        if stdin.read_line(&mut line).is_ok()
        {
            println!();

            if let Ok(input) = line.trim().parse::<usize>()
            {
                if input < available_ports.len()
                {
                    return available_ports[input].port.clone();
                }
            }
        }
        else
        {
            panic!("Failed to read from stdin, supply the port as a parameter instead");
        }
    }
}



async fn get_output_device() -> String
{
    let available_devices = mk_actions_pulseaudio::available_output_devices().await;

    for (i, device) in available_devices.iter().enumerate()
    {
        println!("[{}] {}", i, device.display_name);
    }

    let stdin = stdin();
    let mut stdout = stdout();
    let mut line = String::new();

    loop
    {
        println!();
        print!("Output device: ");
        _ = stdout.flush();

        if stdin.read_line(&mut line).is_ok()
        {
            println!();

            if let Ok(input) = line.trim().parse::<usize>()
            {
                if input < available_devices.len()
                {
                    return available_devices[input].name.clone();
                }
            }
        }
        else
        {
            panic!("Failed to read from stdin");
        }
    }
}