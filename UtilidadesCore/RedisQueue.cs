using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilidadesCore
{
    public class RedisQueue
    {
        private readonly IDatabase _database;

        public RedisQueue(string servers, int port, int database, string auth = "")
        {
            var client = new RedisClient(servers, port, database, auth);
            var connection = client.RedisConnection;
            _database = connection.GetDatabase(database);
        }

        public void Push(string key, string value)
        {
            _database.ListRightPush(key, value);
        }

        public string Pop(string key)
        {
            var result = _database.ListLeftPop(key);
            return result;
        }

        public void DeleteKey(string key)
        {
            _database.KeyDelete(key);
        }
    }
}
