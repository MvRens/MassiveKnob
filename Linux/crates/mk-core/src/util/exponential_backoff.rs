use std::time::Duration;
use std::time::Instant;


pub struct ExponentialBackoff
{
    start_timeout: Duration,
    max_timeout: Duration,

    status: ExponentialBackoffStatus
}


#[derive(Debug)]
pub enum ExponentialBackoffStatus
{
    Clear,
    Failed { when: Instant, count: u32, timeout: Duration }
}


impl ExponentialBackoff
{
    pub fn new(start_timeout: Duration, max_timeout: Duration) -> Self
    {
        Self
        {
            max_timeout,
            start_timeout,

            status: ExponentialBackoffStatus::Clear
        }
    }


    pub fn allowed(&self) -> bool
    {
        match self.status
        {
            ExponentialBackoffStatus::Clear => true,
            ExponentialBackoffStatus::Failed { when, count: _, timeout } =>
            {
                let now = Instant::now();
                (now - when) > timeout
            }
        }
    }


    pub fn fail(&mut self) -> Duration
    {
        let now = Instant::now();

        self.status = match self.status
        {
            ExponentialBackoffStatus::Clear => ExponentialBackoffStatus::Failed
            {
                when: now,
                count: 1,
                timeout: self.get_timeout(1)
            },

            ExponentialBackoffStatus::Failed { when: _, count, timeout: _ } => ExponentialBackoffStatus::Failed
            {
                when: now,
                count: count + 1,
                timeout: self.get_timeout(count + 1)
            }
        };

        match self.status
        {
            ExponentialBackoffStatus::Clear => Duration::ZERO,
            ExponentialBackoffStatus::Failed { when: _, count: _, timeout } => timeout,
        }
    }


    pub fn clear(&mut self)
    {
        self.status = ExponentialBackoffStatus::Clear;
    }


    fn get_timeout(&self, count: u32) -> Duration
    {
        match count
        {
            0..1 => self.start_timeout,
            _ => self.max_timeout.min(self.start_timeout * 2_u32.pow(count - 1))
        }
    }
}


impl Default for ExponentialBackoff
{
    fn default() -> Self
    {
        Self::new(Duration::from_millis(125), Duration::from_secs(4))
    }
}