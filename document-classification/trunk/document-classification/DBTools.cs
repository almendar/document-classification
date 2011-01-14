using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Web;
using System.Data.Common;
using System.Data.SqlClient;

namespace document_classification
{
    public sealed class DBTools
    {
        static readonly DBTools instance = new DBTools();

        private MySqlConnection conn;
        private string lastUpdate;
        private const string connectionString = "Server=localhost;Database=amod;Uid=root;Pwd=1207pegazo;";

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static DBTools()
        {
        }

        DBTools()
        {
        }

        public static DBTools Instance
        {
            get
            {
                return instance;
            }
        }
        public void connect()
        {
            if(conn == null)
                conn = new MySqlConnection();
            conn.ConnectionString = connectionString;
            conn.Open();
        }
        public void disconnect()
        {
            conn.Close();
        }
        public Dictionary<string,int> extractDocument(String doc)
        {
            Dictionary<string, int> extractedDoc = new Dictionary<string, int>();
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            string[] words = doc.Split(delimiterChars);

            foreach(string s in words)
            {
                if (extractedDoc.ContainsKey(s))
                    extractedDoc[s]++;
                else
                    extractedDoc.Add(s, 1);
            }
            return extractedDoc;
        }
        public DbDataReader getData()
        {
            string ftsQuery = @"select *
                               from amod.ftsearchdata 
                                 order by ftsModified;";
            List<String> newData = new List<String>();

            DbCommand cmd = new MySqlCommand(ftsQuery, conn);
            return (cmd.ExecuteReader());
        }
        public List<Dictionary<string,int> > getNewData()
        {
            List<Dictionary<string, int>> newData = new List<Dictionary<string, int>>();
            DbDataReader rdr = getData();
            while (rdr.Read() && rdr["ftsModified"].ToString().CompareTo(lastUpdate) > 0)
            {
                newData.Add(extractDocument(rdr["ftsText"].ToString()));          
            }
            return newData;
        }
    }
}
