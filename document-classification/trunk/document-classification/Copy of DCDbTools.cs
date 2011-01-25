using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Runtime.Serialization.Formatters.Binary;

namespace DocumentClassification.Representation
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
        private int CurrentVersion;

        private const string connectionString = "Server=localhost;Database=dc;Uid=root;Pwd=riva87;";

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

        public void createNewVersion()
        {
            connect();
            string Query = "INSERT INTO dc.versionHistory(imageDate) values" +
             "('" + System.DateTime.Now + "');" +
             "select LAST_INSERT_ID();";
            DbDataReader rdr = executeQuery(Query);
            rdr.Read();
            CurrentVersion = rdr.GetInt32(0);
            disconnect();
        }
        public void setCurrentVersion()
        {
            connect();
            string Query = "SELECT MAX(idversionHistory) FROM versionhistory;";
            DbDataReader rdr = executeQuery(Query);
            rdr.Read();
            CurrentVersion = rdr.GetInt32(0);
            disconnect();
        }

        public void sendDBRepresentation()
        {
            connect();
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            bf.Serialize(mem, Data.Instance.DBRepresentation);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc.dbrepresentation(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            executeNonQuery(Query);
            disconnect();
        }
        public void getDBRepresentation()
        {
            connect();
            string Query = @"SELECT * FROM dc.dbrepresentation
                           where versionhistory_idversionHistory = " + CurrentVersion + ";";
            DbDataReader rdr = executeQuery(Query);
            if (rdr.Read())
            {
                BinaryFormatter bf = new BinaryFormatter();
                System.IO.MemoryStream mem =
                new System.IO.MemoryStream(Convert.FromBase64String(rdr.GetString(1)));
                Data.Instance.DBRepresentation = (DBRepresentation)bf.Deserialize(mem);
            }
            disconnect();
        }
        public void getAllCases()
        {
            connect();
            string Query = @"SELECT * FROM dc.allcases
                           where versionhistory_idversionHistory = " + CurrentVersion + ";";
            DbDataReader rdr = executeQuery(Query);
            if (rdr.Read())
            {
                BinaryFormatter bf = new BinaryFormatter();

                System.IO.MemoryStream mem =
                    new System.IO.MemoryStream(Convert.FromBase64String(rdr.GetString(1)));
                Data.Instance.AllCases = (AllCases)bf.Deserialize(mem);
            }
            disconnect();
        }
        public void sendAllCases()
        {
            connect();
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            mem.Position = 0;
            bf.Serialize(mem, Data.Instance.AllCases);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc.allcases(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            executeNonQuery(Query);
            disconnect();
        }
        public void getAllProcedures()
        {
            connect();
            string Query = @"SELECT * FROM dc.allprocedures
                           where versionhistory_idversionHistory = " + CurrentVersion + ";";
            DbDataReader rdr = executeQuery(Query);
            if (rdr.Read())
            {
                BinaryFormatter bf = new BinaryFormatter();
                System.IO.MemoryStream mem =
                    new System.IO.MemoryStream(Convert.FromBase64String(rdr.GetString(1)));
                Data.Instance.AllProcedures = (AllProcedures)bf.Deserialize(mem);
            }
            disconnect();
        }
        public void sendAllProcedures()
        {
            connect();
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            bf.Serialize(mem, Data.Instance.AllProcedures);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc.allprocedures(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            executeNonQuery(Query);
            disconnect();
        }
        public void sendAllDecisionsStatus()
        {
            connect();
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            bf.Serialize(mem, Data.Instance.AllDecisionsStatus);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc.alldecisionsstatus(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            executeNonQuery(Query);
            disconnect();
        }
        public void getAllDecisionsStatus()
        {
            connect();
            string Query = @"SELECT * FROM dc.alldecisionsstatus
                           where versionhistory_idversionHistory = " + CurrentVersion + ";";
            DbDataReader rdr = executeQuery(Query);
            if (rdr.Read())
            {
                BinaryFormatter bf = new BinaryFormatter();
                System.IO.MemoryStream mem =
                    new System.IO.MemoryStream(Convert.FromBase64String(rdr.GetString(1)));
                Data.Instance.AllDecisionsStatus = (AllDecisionsStatus)bf.Deserialize(mem);
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

        public void loadData()
        {
            setCurrentVersion();
            getDBRepresentation();
            getAllCases();
            getAllDecisionsStatus();
            getAllProcedures();
            getAllDecisionsPeople();
        }
        public void sendData()
        {
            
        }

        private void getAllDecisionsPeople()
        {
            connect();
            string Query = @"SELECT * FROM dc.alldecisionspeople
                           where versionhistory_idversionHistory = " + CurrentVersion + ";";
            DbDataReader rdr = executeQuery(Query);
            if (rdr.Read())
            {
                BinaryFormatter bf = new BinaryFormatter();
                System.IO.MemoryStream mem =
                    new System.IO.MemoryStream(Convert.FromBase64String(rdr.GetString(1)));
                Data.Instance.AllDecisionsPeople = (AllDecisionsPeople)bf.Deserialize(mem);
            }
            disconnect();
 
        }
        public void sendAllDecisionsPeople()
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            bf.Serialize(mem, Data.Instance.AllDecisionsPeople);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc.alldecisionspeople(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            connect();
            executeNonQuery(Query);
            disconnect();
        }

    }
}
