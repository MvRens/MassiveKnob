#[derive(Copy, Clone)]
pub enum MassiveKnobHostToDeviceFrameID
{
    Handshake = 42,
    AnalogOutput = 3,
    DigitalOutput = 4,
    Quit = 62,
}


pub enum MassiveKnobDeviceToHostFrameID
{
    HandshakeResponse = 43,
    AnalogInput = 1,
    DigitalInput = 2,
    KeepAlive = 61,
    Error = 63
}


pub struct MassiveKnobDeviceSpecs
{
    pub analog_inputs: u8,
    pub digital_inputs: u8,
    pub analog_outputs: u8,
    pub digital_outputs: u8
}


impl TryFrom<u8> for MassiveKnobDeviceToHostFrameID
{
    type Error = ();

    fn try_from(v: u8) -> Result<Self, <MassiveKnobDeviceToHostFrameID as TryFrom<u8>>::Error>
    {
        match v {
            x if x == MassiveKnobDeviceToHostFrameID::HandshakeResponse as u8 => Ok(MassiveKnobDeviceToHostFrameID::HandshakeResponse),
            x if x == MassiveKnobDeviceToHostFrameID::AnalogInput as u8 => Ok(MassiveKnobDeviceToHostFrameID::AnalogInput),
            x if x == MassiveKnobDeviceToHostFrameID::DigitalInput as u8 => Ok(MassiveKnobDeviceToHostFrameID::DigitalInput),
            x if x == MassiveKnobDeviceToHostFrameID::Error as u8 => Ok(MassiveKnobDeviceToHostFrameID::Error),
            x if x == MassiveKnobDeviceToHostFrameID::KeepAlive as u8 => Ok(MassiveKnobDeviceToHostFrameID::KeepAlive),
            _ => Err(()),
        }
    }
}