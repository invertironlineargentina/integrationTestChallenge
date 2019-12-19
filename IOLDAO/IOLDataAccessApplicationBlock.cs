using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOLDAO
{
    public class IOLDataAccessApplicationBlock
    {

        public DataSet ExecuteQuery(string commandText)
        {
            DbCommand command = new SqlCommand(commandText);
            command.CommandType = CommandType.Text;
            return Execute(command);
        }

        public DataSet Execute(DbCommand command)
        {
            string databaseName = "IOL";

            DatabaseProviderFactory factory = new DatabaseProviderFactory();
            var db = factory.Create(databaseName);

            var dc = db.CreateConnection();
            dc.Open();
            command.Connection = dc;
            DataSet result = db.ExecuteDataSet(command);

            return result;

        }
    }
}
