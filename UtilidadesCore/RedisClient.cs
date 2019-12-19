using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilidadesCore
{
    public class RedisClient
    {
        private const string REDIS_CLIENT = "Redis client";

        private int _redisDatabase;
        private ConfigurationOptions _configOptions;
        public readonly ConnectionMultiplexer RedisConnection;


        public RedisClient(string servers, int port, int redisDatabase, string redisAuthorizationName)
        {
          
            var configOptions = new ConfigurationOptions();
            foreach (var server in servers.Split(','))
                configOptions.EndPoints.Add(server, port);

            configOptions.Password = redisAuthorizationName;
            configOptions.ClientName = "SafeRedisConnection";
            configOptions.KeepAlive = 180;
            configOptions.SyncTimeout = int.MaxValue;
            configOptions.AbortOnConnectFail = false;
            _configOptions = configOptions;

            RedisConnection = ConnectionMultiplexer.Connect(_configOptions);

            if (!RedisConnection.IsConnected)
            {
                string mensaje = $"Cannot connect to redis server: Additional info {RedisConnection.GetStatus()}";

                EnviarMensajes(REDIS_CLIENT, mensaje);

                throw new ArgumentException(mensaje);
            }

            RedisConnection.ConnectionFailed += RedisConnection_ConnectionFailed;
            RedisConnection.ConnectionRestored += RedisConnection_ConnectionRestored;
            RedisConnection.ErrorMessage += RedisConnection_ErrorMessage;
        }

        private void RedisConnection_ErrorMessage(object sender, StackExchange.Redis.RedisErrorEventArgs e)
        {
            EnviarMensajes(REDIS_CLIENT, $"{nameof(RedisConnection_ErrorMessage) + " - El servidor notifica que hubo un error"}");
        }

        private void RedisConnection_ConnectionRestored(object sender, StackExchange.Redis.ConnectionFailedEventArgs e)
        {
            EnviarMensajes(REDIS_CLIENT, $"{nameof(RedisConnection_ConnectionRestored) + " - La conexión se restablecio"}");
        }

        private void RedisConnection_ConnectionFailed(object sender, StackExchange.Redis.ConnectionFailedEventArgs e)
        {
            EnviarMensajes(REDIS_CLIENT, $"{nameof(RedisConnection_ConnectionFailed) + " - Error en una conexión fisica a redis. No hay conexión, mucha lentitud o el servidor tiene algun inconveniente"}");
        }

        private void EnviarMensajes(string nombreServicio, string mensaje)
        {
            Console.WriteLine(mensaje);
        }
    }
}
