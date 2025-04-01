use core::panic;
use std::env;
use std::io::stdin;
use std::io::stdout;
use std::io::Write;
use std::sync::Mutex;

use colog::format::CologStyle;
use crossbeam_channel::unbounded;

use mk_actions_pulseaudio::set_volume::SetVolumeActionSettings;
use mk_core::action::ActionFactory;
use mk_core::action::AnalogInputAction;
use mk_core::device::DeviceEventMessage;
use mk_core::device::DeviceFactory;
use mk_device_serialmin::device::SerialMinDevice;
use mk_device_serialmin::device::SerialMinDeviceSettings;
use mk_actions_pulseaudio::set_volume::SetVolumeAction;


pub struct ExtendedLogger
{
    longest_target: Mutex<usize>
}

impl ExtendedLogger
{
    fn new() -> Self
    {
        Self
        {
            longest_target: Mutex::new(0)
        }
    }
}

impl CologStyle for ExtendedLogger
{
    fn format(&self, buf: &mut env_logger::fmt::Formatter, record: &log::Record<'_>) -> Result<(), std::io::Error>
    {
        let sep = self.line_separator();
        let prefix = self.prefix_token(&record.level());
        let target = record.metadata().target();
        let mut target_length = target.len();

        if let Ok(mut longest_target) = self.longest_target.lock()
        {
            if target_length > *longest_target
            {
                *longest_target = target_length;
            }
            else
            {
                target_length = *longest_target;
            }
        }

        writeln!(
            buf,
            "{} {:>target_length$} | {}",
            prefix,
            target,
            record.args().to_string().replace('\n', &sep),
            target_length=target_length
        )
    }
}


#[tokio::main]
async fn main()
{
    colog::basic_builder()
        .filter(None, log::LevelFilter::Trace)
        .filter(Some("min_rs"), log::LevelFilter::Info)
        .filter(Some("serialmin"), log::LevelFilter::Info)
        .format(colog::formatter(ExtendedLogger::new()))
        .init();

    let port = get_port();
    let output_device_1 = get_output_device(1).await;
    let output_device_2 = get_output_device(2).await;


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

    println!("Select serial port:");
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



async fn get_output_device(number: u8) -> String
{
    let available_devices = mk_actions_pulseaudio::available_output_devices().await;

    println!();
    println!("Select audio device for analog input {}:", number);

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