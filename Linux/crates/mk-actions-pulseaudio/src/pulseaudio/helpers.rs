use libpulse_binding::volume::Volume;
use mk_core::types::AnalogValue;


pub fn into_volume(value: AnalogValue) -> Volume
{
    let range = Volume::NORMAL.0 as f64 - Volume::MUTED.0 as f64;
    Volume((Volume::MUTED.0 as f64 + (u8::from(value) as f64) * range / 100.0) as u32)
}
