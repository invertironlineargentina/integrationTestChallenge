using QuickFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRoutingFixClient.Connection
{
    public interface IFixEngineConnection
    {
        bool EstaDesconectado { get; }
        bool EstaLogueado { get; }
        IInitiator SocketInitiator { get; set; }

        void Conectarse();
        void Desloguearse();
        void DetenerConexion();
        void DetenerSesion(string razon);
        bool EnviarMensaje(Message mensaje);
        void SetNewSession(SessionID sessionID);
    }
}
