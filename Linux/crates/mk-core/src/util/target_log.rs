
#[macro_export]
macro_rules! target_log
{
    ($target:expr) =>
    {
        #[macro_export]
        macro_rules! trace
        {
            ($($arg:tt)*) =>
            {
                log::trace!(target: $target, $($arg)*);
            };
        }

        #[macro_export]
        macro_rules! debug
        {
            ($($arg:tt)*) =>
            {
                log::debug!(target: $target, $($arg)*);
            };
        }

        #[macro_export]
        macro_rules! info
        {
            ($($arg:tt)*) =>
            {
                log::info!(target: $target, $($arg)*);
            };
        }

        #[macro_export]
        macro_rules! warn
        {
            ($($arg:tt)*) =>
            {
                log::warn!(target: $target, $($arg)*);
            };
        }

        #[macro_export]
        macro_rules! error
        {
            ($($arg:tt)*) =>
            {
                log::error!(target: $target, $($arg)*);
            };
        }
    };
}