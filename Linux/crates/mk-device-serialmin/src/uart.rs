use std::cell::RefCell;
use std::time::Duration;

use serialport::SerialPort;


pub struct Uart
{
    port: String,
    baud_rate: u32,
    serial_port: RefCell<Option<Box<dyn SerialPort>>>,
    tx_space_available: u16
}


// TODO verbose loggging

impl Uart
{
    pub fn new(port: String, baud_rate: u32, tx_space_available: u16) -> Self
    {
        Uart
        {
            port,
            baud_rate,
            serial_port: RefCell::new(None),
            tx_space_available
        }
    }


    pub fn try_open(&self) -> Result<(), serialport::Error>
    {
        let mut serial_port = self.serial_port.borrow_mut();

        if serial_port.is_some() { return Ok(()); }

        match serialport::new(self.port.clone(), self.baud_rate)
            .data_bits(serialport::DataBits::Eight)
            .parity(serialport::Parity::None)
            .stop_bits(serialport::StopBits::One)
            .flow_control(serialport::FlowControl::None)
            .timeout(Duration::from_millis(10))
            .open()
        {
            Ok(new_serial_port) =>
            {
                serial_port.replace(new_serial_port);
                Ok(())
            },

            Err(e) => Err(e)
        }
    }


    pub fn close(&self)
    {
        self.serial_port.take();
    }


    pub fn available_for_write(&self) -> u16
    {
        self.tx_space_available
    }


    pub fn tx(&self, byte: u8)
    {
        let mut serial_port = self.serial_port.borrow_mut();
        if let Some(serial_port) = &mut *serial_port
        {
            match serial_port.write_all(&[byte])
            {
                Ok(_) => {},
                Err(_e) =>
                {
                    //debug!("{}", e);
                },
            }
        }
    }


    pub fn read(&self, buf: &mut [u8]) -> Result<usize, ()>
    {
        let mut serial_port = self.serial_port.borrow_mut();
        if let Some(serial_port) = &mut *serial_port
        {
            match serial_port.read(&mut buf[..])
            {
                Ok(n) => Ok(n),
                _ => Err(()),
            }
        }
        else
        {
            Err(())
        }
    }
}


impl min_rs::Interface for Uart
{
    fn tx_start(&self)
    {
    }


    fn tx_finished(&self)
    {
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
