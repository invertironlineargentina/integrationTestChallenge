using IOLEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilidadesCore;

namespace OrderRoutingQueueConsumer
{
    public class MessageDecoder
    {
        private readonly IInterfacePresenter _interfacePresenter;
        private readonly TransaccionesServices TransaccionesServices;

        public MessageDecoder(IInterfacePresenter interfacePresenter)
        {
            _interfacePresenter = interfacePresenter;
            TransaccionesServices = new TransaccionesServices();
        }

        public NovedadFIXDTO DecodificarMensaje(string mensaje)
        {
            try
            {
                string[] campos = null;
                int idTransaccion = 0;

                campos = mensaje.Split('-');

                idTransaccion = ObtenerIdTransaccion(campos);

                var concertacion = new NovedadFIXDTO();
                var tipoAccionOrden = (TipoAccionOrden)Convert.ToInt32(campos[0]);

                switch (tipoAccionOrden)
                {
                    case TipoAccionOrden.CancelarOrden:
                        concertacion = IntentarCancelarOrden(idTransaccion, campos[2], mensaje, tipoAccionOrden);
                        break;
                    case TipoAccionOrden.ConfirmarRecepcion:
                        concertacion = IntentarConfirmarRecepcionOrden(idTransaccion, campos, tipoAccionOrden);
                        break;
                    case TipoAccionOrden.ConfirmarRecepcionYConcertar:
                    case TipoAccionOrden.Concertar:
                        concertacion = IntentarConfirmarRecepcionYConcertarOrden(idTransaccion, campos, mensaje, tipoAccionOrden);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return concertacion;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private int ObtenerIdTransaccion(string[] campos)
        {
            var idTransaccion = Convert.ToInt32(campos[1]);
            if (idTransaccion == 0)
            {
                var mensaje = "El id transaccion no puede ser 0";
                _interfacePresenter.MostrarMensaje(mensaje);

                throw new ArgumentException(mensaje);
            }

            return idTransaccion;
        }

        private NovedadFIXDTO IntentarConfirmarRecepcionYConcertarOrden(int idTransaccion, string[] campos, string mensaje, TipoAccionOrden tipoAccionOrden)
        {
            try
            {
                if (campos[6] == string.Empty || campos[7] == string.Empty)
                    throw new Exception("Los campos del mensaje son inválidos");

                return new NovedadFIXDTO
                {
                    FechaConcertacion = DateTime.Parse(campos[2]),
                    Cantidad = Convert.ToDecimal(campos[3]),
                    Precio = Convert.ToDecimal(campos[4]),
                    Partida = campos[5],
                    IdTransaccionContraparte = campos[6],
                    IdFix = campos[7],
                    TipoAccionOrden = tipoAccionOrden,
                    IdTransaccion = idTransaccion
                };
            }
            catch (Exception ex)
            {
                string mensajeError = $"Error al intentar parser un mensaje sqs y concertar-mensaje:{mensaje}";
                _interfacePresenter.MostrarMensaje(mensajeError);

                return null;
            }
        }

        private NovedadFIXDTO IntentarConfirmarRecepcionOrden(int idTransaccion, string[] campos, TipoAccionOrden tipoAccionOrden)
        {
            if (campos[2] == string.Empty || campos[3] == string.Empty)
                throw new Exception("Los campos del mensaje son inválidos");

            return new NovedadFIXDTO
            {
                IdTransaccion = idTransaccion,
                IdTransaccionContraparte = campos[2],
                IdFix = campos[3],
                TipoAccionOrden = tipoAccionOrden
            };
        }

        private Transaccion GetTransaccion(int idTransaccion)
        {
            var transaccion = TransaccionesServices.GetTransaccionByID(idTransaccion);
            return transaccion;
        }

        private NovedadFIXDTO IntentarCancelarOrden(int idTransaccion, string fechaCancelacionString, string mensaje, TipoAccionOrden tipoAccionOrden)
        {
            bool fueExitoso = DateTime.TryParse(fechaCancelacionString, out var fechaCancelacion);
            if (!fueExitoso)
            {
                return null;
            }

            return new NovedadFIXDTO
            {
                FechaCancelacion = fechaCancelacion,
                IdTransaccion = idTransaccion,
                TipoAccionOrden = tipoAccionOrden
            };
        }
    }
}
