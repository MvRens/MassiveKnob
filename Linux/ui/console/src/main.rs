use core::panic;
use std::env;
use std::io::stdin;
use std::io::stdout;
use std::io::Write;

use crossbeam_channel::unbounded;

use mk_core::device::DeviceEventMessage;
use mk_core::device::DeviceFactory;
use mk_device_serialmin::device::SerialMinDevice;
use mk_device_serialmin::device::SerialMinDeviceSettings;

fn main()
{
    let port = get_port();


    colog::basic_builder()
        .filter(None, log::LevelFilter::Trace)
        .init();

    log::info!("MassiveKnob starting for serial device on port {}", port);

    let (sender, receiver) = unbounded();
    let _device = SerialMinDevice::create(SerialMinDeviceSettings {
        port,
        baud_rate: 115200
    }, sender);

    log::info!("Waiting for events...");
    loop
    {
        if let Ok(event) = receiver.recv()
        {
            match event
            {
                DeviceEventMessage::AnalogInput { input, value } => println!("Analog input #{}: {}", input, value),
                DeviceEventMessage::DigitalInput { input, value } => println!("Digital input #{}: {}", input, value)
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
        println!("[{}] {}", i, port);
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
            if let Ok(input) = line.trim().parse::<usize>()
            {
                if input < available_ports.len()
                {
                    return available_ports[input].clone();
                }
            }
        }
        else
        {
            panic!("Failed to read from stdin, supply the port as a parameter instead");
        }
    }
}