using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilidadesCore
{
    public interface ILogger
    {
        void LogMensaje(TipoLog tipo, Exception ex);
        void LogMensaje(TipoLog tipo, Exception e, string mensaje);
        void LogMensaje(TipoLog tipo, string mensaje);
        void LogMensaje(TipoLog tipo, string mensaje, DateTime timeStamp);
    }
}
