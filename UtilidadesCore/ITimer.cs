using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace UtilidadesCore
{
    public interface ITimer
    {
        bool Enabled { get; set; }
        double Interval { get; set; }

        event ElapsedEventHandler Elapsed;

        void Start(double interval);
        void Stop();
    }
}
