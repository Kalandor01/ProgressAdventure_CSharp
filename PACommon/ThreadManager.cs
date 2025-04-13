using PACommon.Enums;

namespace PACommon
{
    public class ThreadManager
    {
        private object _isCanceledLock = new();

        private Task _threadTask;
        private bool _isCanceled;

        /// <summary>
        /// If the task is canceled.
        /// </summary>
        public bool IsCanceled
        {
            get => _isCanceled;
            private set
            {
                lock (_isCanceledLock)
                {
                    _isCanceled = value;
                }
            }
        }

        /// <summary>
        /// If the task has been started.
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        /// If the task ended.
        /// </summary>
        public bool Exited { get; private set; }

        public ThreadManager(string threadName, Action<ThreadManager> threadFunction)
        {
            _threadTask = new Task(() =>
            {
                Thread.CurrentThread.Name = threadName;
                try
                {
                    threadFunction(this);
                }
                finally
                {
                    Exited = true;
                }
            });
        }

        /// <summary>
        /// Starts running the task is it hasn's been started yet.
        /// </summary>
        public void Run()
        {
            if (Started)
            {
                return;
            }

            Started = true;
            _threadTask.Start();
        }

        /// <summary>
        /// Stops the task if it's running.
        /// </summary>
        /// <param name="wait">Whether to wait for the task to stop.</param>
        public void Stop(bool wait = true)
        {
            if (Exited || !Started)
            {
                return;
            }

            IsCanceled = true;
            if (wait)
            {
                _threadTask.Wait();
            }
        }

        /// <summary>
        /// Aborts the execution of the task.<br/>
        /// WARNING: DOESN'T ACTUALY ABORT THE TASK, JUST WAITS FOR IT TO STOP NATURALY!!!
        /// </summary>
        /// <param name="waitMs">The number of milliseconds to wait befor forciefully aborting the task.</param>
        public void Abort(int waitMs = 0)
        {
            Stop(false);
            Thread.Sleep(waitMs);
            // TODO: TRUE ABORT IMPOSIBLE!?
            _threadTask.Wait();
            Exited = true;
        }

        /// <summary>
        /// Creates a task with a function, with error handling.
        /// </summary>
        /// <param name="threadName">The name of the new thread.</param>
        /// <param name="threadFunction">The function to run in the thread.</param>
        public static ThreadManager CreateTaskWithErrorHandling(string threadName, Action<ThreadManager> threadFunction)
        {
            return new ThreadManager(
                threadName,
                (threadManager) => {
                    try
                    {
                        if (threadManager.IsCanceled)
                        {
                            return;
                        }
                        threadFunction(threadManager);
                    }
                    catch (Exception e)
                    {
                        PACSingletons.Instance.Logger.Log("Thread crashed", e.ToString(), LogSeverity.FATAL);
                        throw;
                    }
                }
            );
        }

        /// <summary>
        /// Creates a task with a looping function, with error handling.
        /// </summary>
        /// <param name="threadName">The name of the new thread.</param>
        /// <param name="threadFunction">The function to run in the thread. If it returns true, it ends the loop.</param>
        /// <param name="loopDelay">The number of milliseconds to suspent the thread inbetween loops.</param>
        public static ThreadManager CreateLoopingTaskWithErrorHandling(string threadName, Func<ThreadManager, bool> threadFunction, int loopDelay = 0)
        {
            return CreateTaskWithErrorHandling(threadName, (threadManager) => {
                while (!threadFunction(threadManager))
                {
                    if (threadManager.IsCanceled)
                    {
                        return;
                    }
                    if (loopDelay > 0)
                    {
                        Thread.Sleep(loopDelay);
                    }
                    if (threadManager.IsCanceled)
                    {
                        return;
                    }
                }
            });
        }
    }
}
