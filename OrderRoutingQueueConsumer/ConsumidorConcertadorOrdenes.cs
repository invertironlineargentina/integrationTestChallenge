using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UtilidadesCore;

namespace OrderRoutingQueueConsumer
{
    public class ConsumidorConcertadorOrdenes
    {
        public ITimer timerLectorQueueRedis;
        public string RedisQueueName = "cola";

        private readonly RedisQueueConsumer _redisQueueConsumer;
        private readonly MessageDecoder _messageDecoder;
        private readonly TransaccionWrapperCRUDManager _transaccionWrapperCRUDManager;
        private readonly IInterfacePresenter _interfacePresenter;
        static object _pedidoLectorQueueRedisLock = new object();

        public ConsumidorConcertadorOrdenes(IInterfacePresenter interfacePresenter, string redisQueueName = null)
        {
            _interfacePresenter = interfacePresenter;

            AsignarColaRedis(redisQueueName);
            _redisQueueConsumer = new RedisQueueConsumer(_interfacePresenter, RedisQueueName);
            _messageDecoder = new MessageDecoder(_interfacePresenter);
            _transaccionWrapperCRUDManager = new TransaccionWrapperCRUDManager(_interfacePresenter);
        }

        public void Iniciar()
        {
            timerLectorQueueRedis = new TimerWrapper();
            timerLectorQueueRedis.Elapsed += PedidoLectorQueueRedis;
            timerLectorQueueRedis.Interval = 1000;
            timerLectorQueueRedis.Enabled = true;
        }

        public void PedidoLectorQueueRedis(object source, ElapsedEventArgs e)
        {
            lock (_pedidoLectorQueueRedisLock)
            {
                try
                {
                    timerLectorQueueRedis.Enabled = false;
                    _interfacePresenter.MostrarMensaje("Leyendo cola de concertación");

                    var messages = _redisQueueConsumer.GetMessages();

                    if (messages == null)
                    {
                        timerLectorQueueRedis.Enabled = true;

                        return;
                    }

                    var redisMessagesToProcess = messages.ToArray();

                    _interfacePresenter.MostrarMensaje("Fin de lectura de cola de concertación");
                    _interfacePresenter.MostrarMensaje("Iniciando la decodificación de los mensajes...");

                    var msjsDecoded = new NovedadFIXDTO[redisMessagesToProcess.Length];

                    for (int i = 0; i < redisMessagesToProcess.Length; i++)
                    {
                        if (redisMessagesToProcess[i] == null)
                            continue;

                        msjsDecoded[i] = _messageDecoder.DecodificarMensaje(redisMessagesToProcess[i]);
                    }

                    _interfacePresenter.MostrarMensaje("Fin de la decodificación de los mensajes.");
                    _interfacePresenter.MostrarMensaje("Empezando a tomar acciones en base al tipo de ordenes recibidas...");

                    for (int i = 0; i < msjsDecoded.Length; i++)
                    {
                        if (msjsDecoded[i] == null)
                            continue;

                        _transaccionWrapperCRUDManager.RealizarAccionDeOrden(msjsDecoded[i]);
                    }

                    _interfacePresenter.MostrarMensaje("Acciones terminadas.");

                    timerLectorQueueRedis.Enabled = true;
                }
                catch (Exception ex)
                {
                
                }
            }
        }

        public void LimpiarCache()
        {
            _transaccionWrapperCRUDManager.TransaccionWrapperList.Clear();
        }

        private void AsignarColaRedis(string redisQueueName)
        {
            if (redisQueueName != null)
                RedisQueueName = redisQueueName;
        }
    }
}
