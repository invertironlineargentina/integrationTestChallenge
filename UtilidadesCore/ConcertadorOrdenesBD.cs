using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOLDAO;
using IOLEntities;

namespace UtilidadesCore
{
    public class ConcertadorOrdenesBD : IConcertadorOrdenes
    {
        private IOLDataAccessApplicationBlock _dao;
        private TransaccionesServices _transaccionesServices;

        public ConcertadorOrdenesBD()
        {
            _dao = new IOLDataAccessApplicationBlock();
            _transaccionesServices = new TransaccionesServices();
        }

        public void CancelarOrden(Transaccion transaccion, NovedadFIXDTO concertacion)
        {
            _dao.ExecuteQuery($"update ttr_transaccion set estado = 5 where id_transaccion = {transaccion.ID}");
        }

        public void Concertar(Transaccion transaccion, NovedadFIXDTO concertacion)
        {
            var transaccionDb = _transaccionesServices.GetTransaccionByID(transaccion.ID);
            int estado = 3;
            if (concertacion.Cantidad >= transaccionDb.Cantidad)
                estado = 4;

            _dao.ExecuteQuery($"update ttr_transaccion set estado = {estado} where id_transaccion = {transaccion.ID}");
        }

        public void ConfirmarRecepcionOrden(Transaccion transaccion, NovedadFIXDTO concertacion)
        {
            _dao.ExecuteQuery($"update ttr_transaccion set estado = 2, idFix = {concertacion.IdFix} where id_transaccion = {transaccion.ID}");
        }

        public void ConfirmarRecepcionYConcertarOrden(Transaccion transaccion, NovedadFIXDTO concertacion)
        {
            ConfirmarRecepcionOrden(transaccion, concertacion);
            Concertar(transaccion, concertacion);
        }
    }
}
