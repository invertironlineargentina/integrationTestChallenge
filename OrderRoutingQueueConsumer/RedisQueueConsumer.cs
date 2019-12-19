using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilidadesCore;

namespace OrderRoutingQueueConsumer
{
    public class RedisQueueConsumer
    {
        private readonly RedisQueue _redisQueue;
        private readonly IInterfacePresenter _interfacePresenter;


        private readonly string _redisQueueName;

        public RedisQueueConsumer(IInterfacePresenter interfacePresenter, string redisQueueName)
        {
            var server = "127.0.0.1";
            int port = 6379;
            int redisDatabase = 10;
            _redisQueue = new RedisQueue(server, port, redisDatabase);

            _interfacePresenter = interfacePresenter;
            _redisQueueName = redisQueueName;
        }

        public IList<string> GetMessages()
        {
            string prefijoMensaje = "test";

            var messages = new List<string>();

            _interfacePresenter.MostrarMensaje("Empezando a leer redis");
            try
            {
                bool tieneDatos = false;
                while (true)
                {
                    var result = _redisQueue.Pop(_redisQueueName);
                    if (string.IsNullOrEmpty(result))
                        break;

                    messages.Add(result);
                    tieneDatos = true;

                    if (messages.Count >= 100)
                        break;
                };

                if (!tieneDatos)
                {
                    _interfacePresenter.MostrarMensaje("Cola Vacia");

                    return null;
                }

                return messages;
            }
            catch (Exception ex)
            {

                return messages.Any() ? messages : null;
            }
        }
    }
}
