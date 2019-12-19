using QuickFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRoutingFixClient.Connection
{
    public class FixEngineConnection : IFixEngineConnection
    {
        private ISessionWrapper _session;
        public IInitiator SocketInitiator { get; set; }
        public bool EstaLogueado => _session?.IsLoggedOn() ?? false;
        public bool EstaDesconectado => SocketInitiator?.IsStopped ?? true;
        
        public FixEngineConnection(ISessionWrapper session, IInitiator socketInitiator)
        {
            _session = session;
            SocketInitiator = socketInitiator;
        }

        public void SetNewSession(SessionID sessionID)
        {
            _session = _session.LookupSession(sessionID);
        }

        public void Conectarse()
        {
            SocketInitiator?.Start();
        }

        public void DetenerConexion()
        {
            SocketInitiator?.Stop();
        }

        public void DetenerSesion(string razon)
        {
            _session?.Disconnect(razon);
        }

        public void Desloguearse()
        {
            _session?.Reset("Deslogueándose.");

            if (EstaLogueado)
                _session.GenerateLogout();
        }

        public bool EnviarMensaje(Message mensaje)
        {
            bool ok = false;
            try
            {
                ok = _session.Send(mensaje);
            }
            catch (Exception ex)
            {
                string mensajeError = $"Error al enviar un mensaje: {Environment.NewLine}{mensaje}";
                Console.WriteLine(mensajeError);
            }

            return ok;
        }
    }
}
