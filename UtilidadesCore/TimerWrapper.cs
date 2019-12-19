using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace UtilidadesCore
{
    public class TimerWrapper : ITimer
    {
        private Timer _timer = new Timer();

        public void Start(double interval)
        {
            _timer.Interval = interval;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public event ElapsedEventHandler Elapsed
        {
            add { _timer.Elapsed += value; }
            remove { _timer.Elapsed -= value; }
        }

        public double Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }
        public bool Enabled
        {
            get { return _timer.Enabled; }
            set { _timer.Enabled = value; }
        }
    }
}
