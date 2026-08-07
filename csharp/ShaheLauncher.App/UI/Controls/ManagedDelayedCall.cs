using System;
using System.Threading;

namespace ShaheLauncher.App.UI.Controls
{
    public sealed class ManagedDelayedCall : IDisposable
    {
        private readonly object _sync = new object();
        private Action _callback;
        private Action<object> _onFinish;
        private Timer _timer;
        private bool _running = true;

        public ManagedDelayedCall(int delayMilliseconds, Action callback, Action<object> onFinish, string threadName)
        {
            Interval = Math.Max(0, delayMilliseconds);
            _callback = callback;
            _onFinish = onFinish;
            _timer = new Timer(Worker, null, Interval, Timeout.Infinite);
        }

        public int Interval { get; private set; }

        public void Stop()
        {
            lock (_sync)
            {
                _callback = null;
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }

                _running = false;
            }

            Finish();
        }

        public void CancelWithoutNativeStop()
        {
            lock (_sync)
            {
                _callback = null;
                _running = false;
            }

            Finish();
        }

        public bool IsRunning()
        {
            lock (_sync)
            {
                return _running;
            }
        }

        public int GetInterval()
        {
            return Interval;
        }

        public void Dispose()
        {
            Stop();
        }

        private void Worker(object state)
        {
            Action callback;
            lock (_sync)
            {
                callback = _callback;
            }

            try
            {
                try
                {
                    callback?.Invoke();
                }
                catch
                {
                }
            }
            finally
            {
                lock (_sync)
                {
                    _running = false;
                    _timer?.Dispose();
                    _timer = null;
                }

                Finish();
            }
        }

        private void Finish()
        {
            Action<object> finish;
            lock (_sync)
            {
                finish = _onFinish;
                _onFinish = null;
            }

            try
            {
                finish?.Invoke(this);
            }
            catch
            {
            }
        }
    }
}
