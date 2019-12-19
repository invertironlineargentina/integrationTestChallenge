using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOLEntities;
using OrderRoutingFixClient.QuickFixExtensions;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX50;
using UtilidadesCore;

namespace OrderRoutingFixClient
{
    public partial class FixInitiator : MessageCracker
    {
        public void OnMessage(ExecutionReport reporteEjecucion, SessionID sessionID)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            _interfacePresenter.MostrarMensaje($"Nueva entrada execution report");

            var idTransaccion = ObtenerIdTransaccionOriginal(reporteEjecucion.ClOrdID);
            var transaccion = GetTransaccion(idTransaccion);

            var executionType = reporteEjecucion.ExecType.getValue();
            var fecha = reporteEjecucion.TransactTime.getValue();

            fecha = TimeZoneInfo.ConvertTimeFromUtc(fecha, _timeZoneInfo);

            _interfacePresenter.MostrarMensaje(@"========================");

            ProcesarExecutionReportParaEnvio(reporteEjecucion, transaccion, executionType, fecha);

            _interfacePresenter.MostrarMensaje($"ClOrdID: {reporteEjecucion.ClOrdID}");
            _interfacePresenter.MostrarMensaje($"NumericOrderId: {ObtenerIdTransaccionContraparte(reporteEjecucion)}");
            _interfacePresenter.MostrarMensaje($"Order status: {reporteEjecucion.OrdStatus}");

            _interfacePresenter.MostrarMensaje(@"========================");
        }

        private void ProcesarExecutionReportParaEnvio(ExecutionReport reporteEjecucion, Transaccion transaccion, char executionType, DateTime fecha)
        {
            switch (executionType)
            {
                case ExecType.NEW:
                    var idTransaccionContraparte = ObtenerIdTransaccionContraparte(reporteEjecucion);
                    var idFix = ObtenerIdFix(reporteEjecucion);
                    _interfacePresenter.MostrarMensaje($"ORDER NEW. ID Contraparte: {idTransaccionContraparte}");
                    ConcertadorOrdenes.ConfirmarRecepcionOrden(transaccion, new NovedadFIXDTO { IdTransaccionContraparte = idTransaccionContraparte, IdFix = idFix });
                    break;
                case ExecType.CANCELED:
                    _interfacePresenter.MostrarMensaje(
                        $"CANCELACIÓN PROCESADA PARA ORDEN {transaccion.ID}, ID Contraparte: {transaccion.IDFix}");
                    ConfirmarCancelacionOrden(transaccion, fecha);
                    break;
                case ExecType.REJECTED:
                    break;
                case ExecType.EXPIRED:
                    _interfacePresenter.MostrarMensaje($"OFERTA EXPIRADA PARA ORDEN {transaccion.ID}, ID Contraparte: {transaccion.IDFix}");
                    ConfirmarCancelacionOrden(transaccion, fecha);
                    break;
                case ExecType.TRADE:
                    var precio = reporteEjecucion.LastPx.getValue();
                    var cantidad = reporteEjecucion.LastQty.getValue();
                    var partida = ObtenerPartida(reporteEjecucion);
                    _interfacePresenter.MostrarMensaje($"NUEVA CONCERTACIÓN (partida {partida}.");
                    _interfacePresenter.MostrarMensaje($"TRADE: Transacción nº{transaccion.ID}, {cantidad} partes a ${precio}");

                    ConcertadorOrdenes.ConfirmarRecepcionYConcertarOrden(transaccion, new NovedadFIXDTO
                    {
                        FechaConcertacion = fecha,
                        Cantidad = cantidad,
                        Precio = precio,
                        Partida = partida,
                        IdTransaccionContraparte = ObtenerIdTransaccionContraparte(reporteEjecucion),
                        IdFix = ObtenerIdFix(reporteEjecucion)
                    });

                    break;
                case ExecType.TRADE_CANCEL:
                    var partesRevertidas = reporteEjecucion.LeavesQty.getValue();
                    var simbolo = reporteEjecucion.Symbol.getValue();
                    var mensaje = $"REVERSIÓN: Transacción nº{transaccion.ID}, {partesRevertidas} partes.";
                    _interfacePresenter.MostrarMensaje(mensaje);
                    break;
                case ExecType.ORDER_STATUS:
                    ProcesarEstadoOrden(reporteEjecucion);
                    break;
                case ExecType.RESTATED:
                    var cantidadRestated = reporteEjecucion.LastQty.getValue();
                    var simboloRestated = reporteEjecucion.Symbol.getValue();
                    break;
                default:
                    throw new NotImplementedException($"Tipo de ejecución {executionType} no soportado.");
            }
        }

        private string ObtenerPartida(QuickFix.FIX50.Message mensajeFix)
        {
            var partida = mensajeFix.GetField(new SecondaryTradeIDCustom(SecondaryTradeIDCustom.SecondaryTradeIDTag)).getValue().TrimStart('0');
            return partida;
        }


        public void OnMessage(TradeCaptureReport message, SessionID sessionID)
        {
               _interfacePresenter.MostrarMensaje("Llegó un Trade Capture Report!");

            var executionType = message.ExecType.getValue();
            var fecha = message.TransactTime.getValue();

            fecha = TimeZoneInfo.ConvertTimeFromUtc(fecha, _timeZoneInfo);

            if (executionType == ExecType.TRADE)
            {
                var numberOfSides = message.Get(new NoSides()).getValue();
                var group = new TradeCaptureReport.NoSidesGroup();
                for (var index = 1; index <= numberOfSides; index++)
                {
                    message.GetGroup(index, group);

                    var idFix = group.OrderID.getValue();
                    Transaccion transaccion;
                    if (Int32.TryParse(group.ClOrdID.getValue(), out int idTransaccion))
                        transaccion = GetTransaccion(idTransaccion, true);
                    else
                        transaccion = GetTransaccionByIdFix(idFix);

                    if (transaccion == null)
                    {
                        var mensaje = $"transaccion con id: {idTransaccion} o idfix: {idFix} no encontrada en BD al recibir un TradeCaptureReport";
                        _interfacePresenter.MostrarMensaje(mensaje);
                        _logger.LogMensaje(TipoLog.Fatal, mensaje);
                        return;
                    }
                    var precio = message.LastPx.getValue();
                    var cantidad = message.LastQty.getValue();
                    var partida = ObtenerPartida(message);

                    var idTransaccionContraparte = message.GetField(new NumericOrderID(NumericOrderID.NumericOrderIDTag)).getValue();


                    if (transaccion.Estado == EstadoTransacciones.Cancelada)
                    {
                        TransaccionesServices.VincularTransaccionConIdFix(transaccion, idFix);
                    }

                    _interfacePresenter.MostrarMensaje($"NUEVA CONCERTACIÓN (partida {partida}).");
                    _interfacePresenter.MostrarMensaje($"TRADE: Transacción nº{transaccion.ID}, {cantidad} partes a ${precio}");
                    ConcertadorOrdenes.ConfirmarRecepcionYConcertarOrden(transaccion, new NovedadFIXDTO
                    {
                        FechaConcertacion = fecha,
                        Cantidad = cantidad,
                        Precio = precio,
                        Partida = partida,
                        IdTransaccionContraparte = idTransaccionContraparte,
                        IdFix = idFix
                    });

                }
            }
            _interfacePresenter.MostrarMensaje(@"========================");
        }

        private static int ObtenerIdTransaccionOriginal(ClOrdID orderId)
        {
            return Convert.ToInt32(orderId.getValue().Replace("C", string.Empty));
        }

        private void ConfirmarCancelacionOrden(Transaccion transaccion, DateTime fechaCancelacion)
        {
            ConcertadorOrdenes.CancelarOrden(transaccion, new NovedadFIXDTO { FechaCancelacion = fechaCancelacion });
        }

        public void ProcesarEstadoOrden(ExecutionReport reporteEjecucion)
        {
            if (ObtenerIdTransaccionOriginal(reporteEjecucion.ClOrdID) == 0)
                return;

            var idTransaccionContraparte = ObtenerIdTransaccionContraparte(reporteEjecucion);
            var idTransaccion = ObtenerIdTransaccionOriginal(reporteEjecucion.ClOrdID);
            var idFix = ObtenerIdFix(reporteEjecucion);
            var transaccion = GetTransaccion(idTransaccion, true);

            //si o si debo pasarla a estado 2 y loguear id's de byma
            if (transaccion.Estado == EstadoTransacciones.Iniciada)
                ConcertadorOrdenes.ConfirmarRecepcionOrden(transaccion, new NovedadFIXDTO { IdTransaccionContraparte = idTransaccionContraparte, IdFix = idFix });

            if (string.IsNullOrEmpty(transaccion.IDFix))
            {
                TransaccionesServices.VincularTransaccionConIdFix(transaccion, idFix);
            }

            var orderStatus = reporteEjecucion.OrdStatus.getValue();
            switch (orderStatus)
            {
                case OrdStatus.NEW:
                    _interfacePresenter.MostrarMensaje($"Llegó estado de orden: NEW");
                    break;
                case OrdStatus.CANCELED:
                    if (transaccion.Estado == EstadoTransacciones.PendienteCancelacion)
                        ConfirmarCancelacionOrden(transaccion, DateTime.Now);

                    break;

                case OrdStatus.REJECTED:
                    if (transaccion.Estado != EstadoTransacciones.Cancelada)
                        ConfirmarCancelacionOrden(transaccion, DateTime.Now);
                    break;

                case OrdStatus.FILLED:
                case OrdStatus.PARTIALLY_FILLED:
                    break;

                default:
                    _interfacePresenter.MostrarMensaje($"Llegó estado de orden sin clasificar: {orderStatus}");
                    break;
            }

            _interfacePresenter.MostrarMensaje($"Llego Order Status para transacción nº{idTransaccion}, ID_Contraparte: {idTransaccionContraparte}");

        }
    }
}
