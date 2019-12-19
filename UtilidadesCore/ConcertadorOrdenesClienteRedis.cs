using IOLEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilidadesCore
{
    public class ConcertadorOrdenesClienteRedis : IConcertadorOrdenes
    {
        private readonly string _redisQueueName;
        private readonly RedisQueue _redisQueue;

        public ConcertadorOrdenesClienteRedis(string redisQueueName)
        {
            var server = "127.0.0.1";
            int port = 6379;
            int redisDatabase = 10;
            _redisQueue = new RedisQueue(server, port, redisDatabase);
            _redisQueueName = redisQueueName;
        }

        public void CancelarOrden(Transaccion transaccion, NovedadFIXDTO concertacion)
        {
            string mensaje = $"{(int)TipoAccionOrden.CancelarOrden}-{transaccion.ID}-{concertacion.FechaCancelacion.ToString()}";
            _redisQueue.Push(_redisQueueName, mensaje);
        }

        public void Concertar(Transaccion transaccion, NovedadFIXDTO concertacion)
        {
            string mensaje = $"{(int)TipoAccionOrden.Concertar}-{transaccion.ID}-{concertacion.FechaConcertacion.ToString()}-{concertacion.Cantidad}-{concertacion.Precio}-{concertacion.Partida}";
            _redisQueue.Push(_redisQueueName, mensaje);
        }

        public void ConfirmarRecepcionOrden(Transaccion transaccion, NovedadFIXDTO concertacion)
        {
            string mensaje = $"{(int)TipoAccionOrden.ConfirmarRecepcion}-{transaccion.ID}-{concertacion.IdTransaccionContraparte}-{concertacion.IdFix}";
            _redisQueue.Push(_redisQueueName, mensaje);
        }

        public void ConfirmarRecepcionYConcertarOrden(Transaccion transaccion, NovedadFIXDTO concertacion)
        {
            string mensaje = $"{(int)TipoAccionOrden.ConfirmarRecepcionYConcertar}-{transaccion.ID}-{concertacion.FechaConcertacion.ToString()}-{concertacion.Cantidad}-{concertacion.Precio}-{concertacion.Partida}-{concertacion.IdTransaccionContraparte}-{concertacion.IdFix}";
            _redisQueue.Push(_redisQueueName, mensaje);
        }
    }
}
