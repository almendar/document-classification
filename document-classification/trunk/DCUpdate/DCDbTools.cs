using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Runtime.Serialization.Formatters.Binary;

namespace document_classification
{
    public class DCDbTools
    {
        private static readonly DCDbTools instance = new DCDbTools();
        public static DCDbTools Instance
        {
            get
            {
                return instance;
            }
        }
        static DCDbTools()
        {
        }
        private DCDbTools()
        {
        }
        private MySqlConnection conn;

        private const string connectionString = "Server=localhost;Database=dc;Uid=root;Pwd=1207pegazo;";
        private void connect()
        {
            if (conn == null)
                conn = new MySqlConnection();
            conn.ConnectionString = connectionString;
            conn.Open();
        }

        private void disconnect()
        {
            conn.Close();
        }

        public void sendDBRepresentation()
        {
            connect();
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            bf.Serialize(mem, Data.Instance.DBRepresentation);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc.dbrepresentation(date, data) values" +
             "('" + "2011-01-01 12:00:00" + "','" + str + "');";

            executeNonQuery(Query);
            disconnect();
        }
        public void getDBRepresentation()
        {
            connect();
            string Query = "SELECT * FROM dc.dbrepresentation;";
            DbDataReader rdr = executeQuery(Query);
            if (rdr.Read())
            {
                BinaryFormatter bf = new BinaryFormatter();
                System.IO.MemoryStream mem =
                new System.IO.MemoryStream(Convert.FromBase64String(rdr.GetString(2)));
                Data.Instance.DBRepresentation = (DBRepresentation)bf.Deserialize(mem);
            }
            disconnect();
        }
        private void executeNonQuery(String query)
        {
            DbCommand cmd = new MySqlCommand(query, conn);
            cmd.ExecuteNonQuery();
        }
        private DbDataReader executeQuery(String query)
        {
            DbCommand cmd = new MySqlCommand(query, conn);
            return (cmd.ExecuteReader());
        }
    }
}
