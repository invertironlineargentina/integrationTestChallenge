using IOLEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilidadesCore;

namespace OrderRoutingQueueConsumer
{
    public class TransaccionWrapperCRUDManager
    {
        public List<TransaccionWrapper> TransaccionWrapperList;

        private readonly TransaccionesServices _transaccionesServices;
        private readonly IConcertadorOrdenes _concertadorOrdenes;
        private readonly IInterfacePresenter _interfacePresenter;

        public TransaccionWrapperCRUDManager(IInterfacePresenter interfacePresenter)
        {
            _interfacePresenter = interfacePresenter;
            TransaccionWrapperList = new List<TransaccionWrapper>();
            _concertadorOrdenes = new ConcertadorOrdenesBD();
            _transaccionesServices = new TransaccionesServices();
        }

        public void RealizarAccionDeOrden(NovedadFIXDTO concertacion)
        {
            try
            {

                switch (concertacion.TipoAccionOrden)
                {
                    case TipoAccionOrden.CancelarOrden:
                        IntentarCancelarOrden(concertacion);
                        break;
                    case TipoAccionOrden.ConfirmarRecepcion:
                        IntentarConfirmarRecepcionOrden(concertacion);
                        break;
                    case TipoAccionOrden.Concertar:
                        IntentarConfirmarRecepcionYConcertarOrden(concertacion);
                        break;
                    case TipoAccionOrden.ConfirmarRecepcionYConcertar:
                        IntentarConfirmarRecepcionYConcertarOrden(concertacion);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void IntentarCancelarOrden(NovedadFIXDTO concertacion)
        {

            AñadirTransaccionWrapperAMemoria(concertacion);

            var transaccionWrapper = TransaccionWrapperList.SingleOrDefault(a => a.Transaccion.ID == concertacion.IdTransaccion);

            if (transaccionWrapper == null)
                return;

            _concertadorOrdenes.CancelarOrden(transaccionWrapper.Transaccion, concertacion);

        }

        private void IntentarConfirmarRecepcionOrden(NovedadFIXDTO concertacion)
        {
            AñadirTransaccionWrapperAMemoria(concertacion);

            var transaccionWrapper = TransaccionWrapperList.SingleOrDefault(a => a.Transaccion.ID == concertacion.IdTransaccion);

            if (transaccionWrapper == null)
                return;

            _concertadorOrdenes.ConfirmarRecepcionOrden(transaccionWrapper.Transaccion, concertacion);

        }

        private void IntentarConfirmarRecepcionYConcertarOrden(NovedadFIXDTO concertacion)
        {
            AñadirTransaccionWrapperAMemoria(concertacion);

            var transaccionWrapper = TransaccionWrapperList.SingleOrDefault(a => a.Transaccion.ID == concertacion.IdTransaccion);

            if (transaccionWrapper == null)
                return;

            bool existePartida = false;

            if (transaccionWrapper.Concertaciones != null)
            {
                existePartida = transaccionWrapper.Concertaciones.Any(a => a.Partida == concertacion.Partida);
            }

            if (existePartida)
            {
                return;
            }

            _concertadorOrdenes.ConfirmarRecepcionYConcertarOrden(transaccionWrapper.Transaccion, concertacion);

        }

        private string ConstruirMsjNumeroPartidas(NovedadFIXDTO concertacion, TransaccionWrapper transaccionWrapper)
        {
            if (transaccionWrapper.Concertaciones.Any(a => a.Partida == concertacion.Partida))
                return $"{concertacion.IdTransaccion} tiene las siguientes partidas en base de datos: {string.Join(",", transaccionWrapper.Concertaciones.Select(a => a.Partida))}";

            return $"{concertacion.IdTransaccion} no tiene partidas en base de datos";
        }

        private Transaccion ObtenerTransaccionEnBd(int idTransaccion)
        {
            var transaccion = _transaccionesServices.GetTransaccionByID(idTransaccion);

            return transaccion;
        }

        private List<NovedadFIXDTO> ObtenerPartidasEnBd(int idTransaccion)
        {
            var concertaciones = new List<NovedadFIXDTO>();

            var partidas = _transaccionesServices.ObtenerPartidas(idTransaccion);

            if (partidas == null)
                return null;

            foreach (var partida in partidas)
            {
                var concertacion = new NovedadFIXDTO();
                concertacion.Partida = partida;
                concertaciones.Add(concertacion);
            }

            return concertaciones;
        }

        private Transaccion ObtenerTransaccionEnBaseUltimoEstado(int idTransaccion, TipoAccionOrden tipoAccionOrden)
        {
            int ultimoEstado = (int)EstadoTransacciones.Iniciada;

            if (tipoAccionOrden == TipoAccionOrden.ConfirmarRecepcion && ultimoEstado != (int)EstadoTransacciones.Iniciada)
                return null;

            bool transaccionDebeActualizarse = false;

            switch (ultimoEstado)
            {
                case (int)EstadoTransacciones.Terminada:
                    break;
                case (int)EstadoTransacciones.Cancelada:
                    break;
                case (int)EstadoTransacciones.CanceladaPorVencimientoValidez:
                    break;
                default:
                    transaccionDebeActualizarse = true;
                    break;
            }

            if (!transaccionDebeActualizarse)
                return null;

            var transaccion = ObtenerTransaccionEnBd(idTransaccion);

            return transaccion;
        }

        private void AñadirTransaccionWrapperAMemoria(NovedadFIXDTO concertacion)
        {
            var transaccion = ObtenerTransaccionEnBaseUltimoEstado(concertacion.IdTransaccion, concertacion.TipoAccionOrden);

            if (transaccion == null)
                return;

            TransaccionWrapperList.RemoveAll(a => a.Transaccion.ID == concertacion.IdTransaccion);

            TransaccionWrapperList.Add(
                new TransaccionWrapper
                {
                    Transaccion = transaccion,
                    Concertaciones = ObtenerPartidasEnBd(concertacion.IdTransaccion)
                });
        }
    }
}
