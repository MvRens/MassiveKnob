use std::sync::Arc;
use std::sync::Mutex;

use futures::channel::oneshot;
use futures::channel::oneshot::Receiver;
use futures::channel::oneshot::Sender;


pub struct SharedSender<T>
{
    sender: Arc<Mutex<Option<Sender<T>>>>
}


impl<T> SharedSender<T>
{
    fn new(sender: oneshot::Sender<T>) -> Self
    {
        Self
        {
            sender: Arc::new(Mutex::new(Some(sender)))
        }
    }

    pub fn try_send(&self, result: T) -> Result<(), T>
    {
        match self.sender.lock()
        {
            Ok(ref mut sender) =>
            {
                match sender.take()
                {
                    Some(sender) => sender.send(result),
                    None => Err(result),
                }
            },

            Err(_) => Err(result)
        }
    }
}


pub fn channel<T>() -> (SharedSender<T>, Receiver<T>)
{
    let (sender, receiver) = oneshot::channel();
    (SharedSender::new(sender), receiver)
}