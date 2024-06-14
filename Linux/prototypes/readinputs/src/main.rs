extern crate serial;
extern crate min_rs as min;

use std::str;
use std::time::Duration;
use std::thread;
use serial::prelude::*;
use serial::SystemPort;
use std::io::prelude::*;
use std::cell::RefCell;
use std::sync::{Arc, Mutex};
use log::{LevelFilter, debug, trace};
use env_logger;


const SERIAL_PORT: &str = "/dev/ttyACM1";
const BAUD_RATE: serial::BaudRate = serial::Baud115200;


struct Uart 
{
    port: RefCell<SystemPort>,
    name: String,
    tx_space_avaliable: u16,
    output: Arc<Mutex<String>>,
}

impl Uart 
{
    fn new(port: SystemPort, name: String, tx_space_avaliable: u16) -> Self 
    {
        Uart
        {
            port: RefCell::new(port),
            name: name,
            tx_space_avaliable: tx_space_avaliable,
            output: Arc::new(Mutex::new(String::from(""))),
        }
    }


    fn open(&self) 
    {
        const SETTINGS: serial::PortSettings = serial::PortSettings 
        {
            baud_rate: BAUD_RATE,
            char_size: serial::Bits8,
            parity: serial::ParityNone,
            stop_bits: serial::Stop1,
            flow_control: serial::FlowNone,
        };

        let mut port = self.port.borrow_mut();
        port.configure(&SETTINGS).unwrap();
        port.set_timeout(Duration::from_millis(1000)).unwrap();
        debug!(target: self.name.as_str(), "{}: Open uart.", self.name);
    }


    fn available_for_write(&self) -> u16 
    {
        self.tx_space_avaliable
    }


    fn tx(&self, byte: u8) 
    {
        let mut output = self.output.lock().unwrap();
        output.push_str(format!("0x{:02x} ", byte).as_str());
        let mut port = self.port.borrow_mut();

        match port.write(&[byte]) 
        {
            Ok(_) => {},
            Err(e) => 
            {
                debug!(target: self.name.as_str(), "{}", e);
            },
        }
    }


    fn read(&self, buf: &mut [u8]) -> Result<usize, ()> 
    {
        let mut port = self.port.borrow_mut();

        match port.read(&mut buf[..]) 
        {
            Ok(n) => Ok(n),
            _ => Err(()),
        }
    }
}


impl min::Interface for Uart 
{
    fn tx_start(&self) 
    {
        let mut output = self.output.lock().unwrap();
        output.clear();
        output.push_str(format!("send frame: [ ").as_str());
    }
    

    fn tx_finished(&self) 
    {
        let mut output = self.output.lock().unwrap();
        output.push_str(format!("]").as_str());
        trace!(target: self.name.as_str(), "{}", output);
    }


    fn tx_space(&self) -> u16 
    {
        self.available_for_write()
    }
    

    fn tx_byte(&self, _min_port: u8, byte: u8) 
    {
        self.tx(byte);
    }
}



#[derive(Copy, Clone)]
enum MassiveKnobHostToDeviceFrameID 
{
    Handshake = 42
    //AnalogOutput = 3,
    //DigitalOutput = 4,
    //Quit = 62,
}


enum MassiveKnobDeviceToHostFrameID 
{
    HandshakeResponse = 43,
    AnalogInput = 1,
    //DigitalInput = 2,
    Error = 63
}


struct MassiveKnobDeviceSpecs
{
    analog_inputs: u8,
    digital_inputs: u8,
    analog_outputs: u8,
    digital_outputs: u8
}


impl TryFrom<u8> for MassiveKnobDeviceToHostFrameID 
{
    type Error = ();

    fn try_from(v: u8) -> Result<Self, <MassiveKnobDeviceToHostFrameID as TryFrom<u8>>::Error> 
    {
        match v {
            x if x == MassiveKnobDeviceToHostFrameID::HandshakeResponse as u8 => Ok(MassiveKnobDeviceToHostFrameID::HandshakeResponse),
            x if x == MassiveKnobDeviceToHostFrameID::AnalogInput as u8 => Ok(MassiveKnobDeviceToHostFrameID::AnalogInput),
            x if x == MassiveKnobDeviceToHostFrameID::Error as u8 => Ok(MassiveKnobDeviceToHostFrameID::Error),
            _ => Err(()),
        }
    }
}


fn main() {
    log::set_max_level(LevelFilter::Debug);
    env_logger::init();

    //let tx_data: [u8; 3] = [1, 2, 3];
    let port = serial::open(SERIAL_PORT).unwrap();
    let uart = Uart::new(port, String::from("uart"), 128);

    let mut min = min::Context::new(
        String::from("min"),
        &uart,
        0,
        true,
    );    
    min.hw_if.open();


    min.reset_transport(true).unwrap_or(());

    // Send handshake
    let handshake: [u8; 2] = ['M' as u8, 'K' as u8];
    min.queue_frame(MassiveKnobHostToDeviceFrameID::Handshake as u8, &handshake[..], handshake.len() as u8).unwrap_or(());


    let mut buf: Vec<u8> = (0..255).collect();
    loop 
    {        
        min.poll(&[0][0..0], 0);

        if let Ok(n) = min.hw_if.read(&mut buf[..]) 
        {
            min.poll(&buf[0..n], n as u32);
        };

        if let Ok(msg) = min.get_msg() 
        {
            match MassiveKnobDeviceToHostFrameID::try_from(msg.min_id) 
            {
                Ok(MassiveKnobDeviceToHostFrameID::HandshakeResponse) => 
                {
                    if msg.len < 4
                    {
                        println!("Invalid handshake response length, expected 4, got {}", msg.len);
                    }

                    let specs = MassiveKnobDeviceSpecs
                    {
                        analog_inputs: msg.buf[0],
                        digital_inputs: msg.buf[1],
                        analog_outputs: msg.buf[2],
                        digital_outputs: msg.buf[3]
                    };

                    println!("Received handshake response:");
                    println!("  Analog inputs:   {}", specs.analog_inputs);
                    println!("  Digital inputs:  {}", specs.digital_inputs);
                    println!("  Analog outputs:  {}", specs.analog_outputs);
                    println!("  Digital outputs: {}", specs.digital_outputs);
                },

                Ok(MassiveKnobDeviceToHostFrameID::AnalogInput) => 
                {
                    if msg.len < 2
                    {
                        println!("Invalid analog input payload length, expected 2, got {}", msg.len);
                    }

                    println!("[Analog input #{}] {}", msg.buf[0], msg.buf[1]);
                }, 

                Ok(MassiveKnobDeviceToHostFrameID::Error) => 
                {
                    println!("[Device error] {}", match str::from_utf8(msg.buf.as_slice()) 
                    {
                        Ok(v) => v,
                        Err(_) => "(unable to parse device error message, invalid UTF-8 sequence)"
                    })
                },

                Err(_) => 
                {
                    println!("Unknown message ID: {}", msg.min_id);
                }
            }
        }

        thread::sleep(Duration::from_millis(10));
    }
}