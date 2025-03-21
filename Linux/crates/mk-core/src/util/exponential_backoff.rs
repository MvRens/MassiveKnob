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
    Failed { when: Instant, count: u32 }
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
            ExponentialBackoffStatus::Failed { when, count } =>
            {
                let now = Instant::now();
                let timeout = self.max_timeout.min(self.start_timeout * 2_u32.pow(count));

                (now - when) > timeout
            }
        }
    }


    pub fn fail(&mut self)
    {
        let count = match self.status
        {
            ExponentialBackoffStatus::Clear => 0,
            ExponentialBackoffStatus::Failed { when: _, count } => count
        };

        self.status = ExponentialBackoffStatus::Failed { when: Instant::now(), count: count + 1 };
    }


    pub fn clear(&mut self)
    {
        self.status = ExponentialBackoffStatus::Clear;
    }
}


impl Default for ExponentialBackoff
{
    fn default() -> Self
    {
        Self::new(Duration::from_millis(500), Duration::from_secs(30))
    }
}