using IOLEntities;
using OrderRoutingFixClient.Connection;
using QuickFix;
using QuickFix.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UtilidadesCore;


namespace OrderRoutingFixClient
{
    public partial class FixInitiator : IApplication
    {
        public string RedisQueueName = "RUTEADOR_CONCERTACIONES";


        private readonly object _lockCicloDeEnvios = new object();
        private readonly object _lockOrderStatus = new object();

        private readonly ITimer _timerOrderStatus;
        public ITimer TimerPeticion;
        private IInterfacePresenter _interfacePresenter;
        private ILogger _logger;
        private IConcertadorOrdenes ConcertadorOrdenes;

        private readonly List<Transaccion> _transaccionesEnviadas;
        private readonly TituloServices _tituloServices;
        private readonly TimeZoneInfo _timeZoneInfo;
        private readonly TransaccionesServices TransaccionesServices;
        private readonly AutomatizacionServices _automatizacionService;

        private FixEngineConnection _fixEngineConnection { get; set; }
        public decimal Precio { get; private set; }
        public string NumeroCuenta { get; private set; }

        public const int APPLICATION_ID = 19;
        public const int DuplicatedOrderIdReason = 8001;
        private readonly int _limitePedidosOrderStatus;


        public FixInitiator()
        {
            _interfacePresenter = new InterfacePresenterConsole();

            _automatizacionService = new AutomatizacionServices();
            TransaccionesServices = new TransaccionesServices();
            _tituloServices = new TituloServices();
            _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

            _transaccionesEnviadas = new List<Transaccion>();

            TimerPeticion = new TimerWrapper();
            TimerPeticion.Elapsed += Pedido;
            TimerPeticion.Interval = 3000;

            _logger = new ConsoleLogger();
            ConcertadorOrdenes = new ConcertadorOrdenesClienteRedis("cola");

        }

        public void SetTimerTickValue(int value)
        {
            TimerPeticion.Interval = value;
        }

        #region Implementaciones IApplication

        public void FromApp(Message msg, SessionID sessionID)
        {
            try
            {
                Crack(msg, sessionID);
            }
            catch (Exception ex)
            {
                if (!(ex is UnsupportedMessageType))
                {
                    _logger.LogMensaje(TipoLog.Fatal, ex, "Error al parsear un mensaje");
                    _interfacePresenter.MostrarMensaje($"ERROR: {ex.Message}");
                }
            }
        }

        public void OnCreate(SessionID sessionID)
        {
            DetenerTimerEnvioOrdenes();
            DetenerTimersConsulta();
            _fixEngineConnection.SetNewSession(sessionID);
            _interfacePresenter.MostrarMensaje($"Sesión creada correctamente con ID {sessionID}.");
        }

        public void OnLogout(SessionID sessionID)
        {
            _interfacePresenter.MostrarMensaje(@"Deslogueado.");

            DetenerTimerEnvioOrdenes();
            DetenerTimersConsulta();
        }

        public void OnLogon(SessionID sessionID)
        {
            _interfacePresenter.MostrarMensaje(@"Logueado correctamente.");
            DetenerTimerEnvioOrdenes();
            DetenerTimersConsulta();

            _transaccionesEnviadas.Clear();

            IniciarTimerEnvioOrdenes();
            IniciarTimersConsulta();
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            if (message.Header.GetString(Tags.MsgType) != MsgType.LOGOUT) return;

            var razon = message.GetField(new Text()).getValue();
            var esErrorAutenticacion = razon.ToLower().Contains("error");
            _interfacePresenter.MostrarMensaje(@"Deslogueado con motivo: " + razon);

            if (esErrorAutenticacion)
            {
                _fixEngineConnection.DetenerSesion(razon);
                DetenerConexion();
                _interfacePresenter.MostrarMensaje("Reiniciar aplicación. Chequear que el usuario y la contraseña sean correctos.");
            }
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            // Logon
            if (message.Header.GetString(Tags.MsgType) == MsgType.LOGON)
            {
                _interfacePresenter.MostrarMensaje(@"Logueándose...");

                try
                {
                    var username = "fakeUser";
                    var password = "fakePass";
                    message.SetField(new Username(username));
                    message.SetField(new Password(password));
                    message.SetField(new HeartBtInt(100));
                }
                catch (Exception ex)
                {
                    var razon = $"Problema al loguearse. {ex.Message}";
                    _interfacePresenter.MostrarMensaje(razon);
                    _logger.LogMensaje(TipoLog.Fatal, ex, razon);
                }
            }
        }


        #endregion

        public void Pedido(object source, ElapsedEventArgs e)
        {
            if (_fixEngineConnection.EstaLogueado)
                lock (_lockCicloDeEnvios)
                {
                    try
                    {
                        _interfacePresenter.MostrarMensaje("================================");
                        _interfacePresenter.MostrarMensaje("Enviando órdenes");

                        DetenerTimerEnvioOrdenes();

                        var cantidadTransaccionesEnviadas = EnviarOrdenes();

                        _interfacePresenter.MostrarMensaje("Fin ciclo envío órdenes");
                        _interfacePresenter.MostrarMensaje("================================");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogMensaje(TipoLog.Fatal, ex, "ERROR en el envio de órdenes");
                        _interfacePresenter.MostrarMensaje($"ERROR en el envio de órdenes: {ex.Message}");
                    }

                    IniciarTimerEnvioOrdenes();
                }
            else
                _interfacePresenter.MostrarMensaje($"Can't send message: session not created. (Pedido Ordenes)");
        }


        public void IniciarTimerEnvioOrdenes()
        {
            TimerPeticion.Enabled = true;
        }

        public void DetenerTimerEnvioOrdenes()
        {
            TimerPeticion.Enabled = false;
        }

        public void IniciarTimersConsulta()
        {

            _timerOrderStatus.Enabled = true;

        }

        public void DetenerTimersConsulta()
        {

                _timerOrderStatus.Enabled = false;


        }

        public int EnviarOrdenes()
        {
            var transacciones = TransaccionesServices.RecibirTransacciones();
            var ordenesAEnviar = ObtenerOrdenesNuevasAEnviar(transacciones);
            var ordenesACancelar = transacciones.Where(
                                        x => x.Estado == EstadoTransacciones.PendienteCancelacion ||
                                             x.Estado == EstadoTransacciones.ParcialmenteTerminadaConPedidoCancelacion)
                                        .OrderBy(x => x.ID).ToList();

            EnviarOrdenesNuevas(ordenesAEnviar);
            return ordenesAEnviar.Count;
        }

        private List<Transaccion> ObtenerOrdenesNuevasAEnviar(IList<Transaccion> transacciones)
        {
            var ordenesAEnviar = transacciones.Where(t => !_transaccionesEnviadas.Select(x => x.ID).Contains(t.ID))
                                                                   .OrderBy(x => x.ID).ToList();
            ordenesAEnviar = ordenesAEnviar.Where(x => x.Estado == EstadoTransacciones.Iniciada).ToList();
            return ordenesAEnviar;
        }

        private void EnviarOrdenesNuevas(List<Transaccion> ordenesNuevas)
        {
            foreach (var transaccion in ordenesNuevas)
            {
                try
                {
                    var ok = EnviarNuevaOrden(transaccion);

                    if (ok)
                    {
                        AgregarTransaccionAListaEnviadas(transaccion);
                        _interfacePresenter.MostrarMensaje($"Orden nº{transaccion.ID} para {transaccion.Simbolo} enviada.");
                    }
                    else
                        _interfacePresenter.MostrarMensaje($"ERROR!!! Orden nº{transaccion.ID} para {transaccion.Simbolo} no puedo ser enviada.");
                }
                catch (Exception ex)
                {
                    string mensajeError = $"ERROR!!! Orden nº{transaccion.ID} para {transaccion.Simbolo} no puedo ser enviada. {ex.Message}";
                    _interfacePresenter.MostrarMensaje(mensajeError);
                }
            }
        }


        public void DetenerConexion()
        {
            _fixEngineConnection.Desloguearse();
            StopAutomatedMessages();
            _fixEngineConnection.DetenerConexion();
        }

        public void FlushListaTransaccionesEnviadas()
        {
            _transaccionesEnviadas.Clear();
        }

        private void StopAutomatedMessages()
        {
            TimerPeticion?.Stop();
        }

        public void SetFixEngineConnection(FixEngineConnection fixEngineConnection)
        {
            _fixEngineConnection = fixEngineConnection;
        }

        public void Start()
        {
            _fixEngineConnection.Conectarse();
        }

        public void ToApp(Message message, SessionID sessionId)
        {

        }

        public bool EnviarNuevaOrden(Transaccion transaccion)
        {
            var newOrderSingle = new QuickFix.FIX50.NewOrderSingle
            {
                OrderQty = new OrderQty(transaccion.Cantidad),
                Symbol = new Symbol(transaccion.Simbolo),
                ClOrdID = new ClOrdID(transaccion.ID.ToString()),
                TransactTime = new TransactTime(transaccion.FechaOrden),
                OrdType = new OrdType(OrdType.LIMIT),
                Price = new Price(Precio),
                TimeInForce = new TimeInForce(TimeInForce.DAY),
                OrderCapacity = new OrderCapacity(OrderCapacity.AGENCY),
                Account = new Account(NumeroCuenta)
            };

            bool ok = _fixEngineConnection.EnviarMensaje(newOrderSingle);
            return ok;
        }
    }
}