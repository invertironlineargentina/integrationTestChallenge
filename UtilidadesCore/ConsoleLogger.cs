using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilidadesCore
{
    public class ConsoleLogger : ILogger
    {

        private string GetTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt");
        }

        public void LogMensaje(TipoLog tipo, string mensaje)
        {
            string mensajeFinal = $"{GetTime()}- {tipo} - {mensaje}";
            Console.WriteLine(mensajeFinal);

        }

        public void LogMensaje(TipoLog tipo, Exception e, string mensaje)
        {
            if (e == null)
            {
                LogMensaje(tipo, mensaje);
                return;
            }

            string mensajeFinal = $"{GetTime()}- {tipo} - {mensaje} - {e.Message} - {e.StackTrace}";
            Console.WriteLine(mensajeFinal);
        }

        public void LogMensaje(TipoLog tipoLog, Exception ex)
        {
            LogMensaje(tipoLog, ex, ex.Message);
        }

        public void LogMensaje(TipoLog tipo, string mensaje, DateTime timeStamp)
        {
            LogMensaje(tipo, mensaje);
        }
    }
}
