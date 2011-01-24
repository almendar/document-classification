﻿using System;
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

        private void createNewVersion()
        {
            string Query = "INSERT INTO dc.versionHistory(imageDate) values" +
             "('" + System.DateTime.Now + "');" +
             "select LAST_INSERT_ID();";
            DbDataReader rdr = executeQuery(Query);
            rdr.Read();
            CurrentVersion = rdr.GetInt32(0);
            rdr.Close();
        }
        private void setCurrentVersion()
        {
            string Query = "SELECT MAX(idversionHistory) FROM versionhistory;";
            DbDataReader rdr = executeQuery(Query);
            rdr.Read();
            CurrentVersion = rdr.GetInt32(0);
            rdr.Close();
        }
        private void sendDBRepresentation()
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            bf.Serialize(mem, Data.Instance.DBRepresentation);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc.dbrepresentation(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            executeNonQuery(Query);
        }
        private void getDBRepresentation()
        {
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
            rdr.Close();
        }
        private void getAllCases()
        {
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
            rdr.Close();
        }
        private void sendAllCases()
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            mem.Position = 0;
            bf.Serialize(mem, Data.Instance.AllCases);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc.allcases(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            executeNonQuery(Query);
        }
        private void getAllProcedures()
        {
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
            rdr.Close();
        }
        private void sendAllProcedures()
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            bf.Serialize(mem, Data.Instance.AllProcedures);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc.allprocedures(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            executeNonQuery(Query);
        }
        private void sendAllDecisionsStatus()
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            bf.Serialize(mem, Data.Instance.AllDecisionsStatus);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc.alldecisionsstatus(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            executeNonQuery(Query);
        }
        private void getAllDecisionsStatus()
        {
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
            rdr.Close();
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
            connect();
            setCurrentVersion();
            getDBRepresentation();
            getAllCases();
            getAllDecisionsStatus();
            getAllProcedures();
            getAllDecisionsPeople();
            disconnect();
        }
        public void sendData()
        {
            connect();
            createNewVersion();
            startTransation();
            sendAllCases();
            sendAllDecisionsPeople();
            sendAllDecisionsStatus();
            sendAllProcedures();
            sendDBRepresentation();
            commit();
            disconnect();
        }

        private void commit()
        {
            string Query = "COMMIT;";
            executeNonQuery(Query);
        }

        private void startTransation()
        {
            string Query = "START TRANSACTION;";
            executeNonQuery(Query);
        }

        private void getAllDecisionsPeople()
        {
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
            rdr.Close();
        }
        private void sendAllDecisionsPeople()
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            bf.Serialize(mem, Data.Instance.AllDecisionsPeople);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc.alldecisionspeople(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            executeNonQuery(Query);
        }

    }
}
