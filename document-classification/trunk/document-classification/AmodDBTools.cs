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
    public sealed class AmodDBTools
    {
        static readonly AmodDBTools instance = new AmodDBTools();

        private MySqlConnection conn;
        private string lastUpdate;
        private const string connectionString = "Server=localhost;Database=amod;Uid=root;Pwd=1207pegazo;";

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AmodDBTools()
        {
        }

        AmodDBTools()
        {
        }

        public static AmodDBTools Instance
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
        public Dictionary<string, int> extractDocument(String doc)
        {
            return extractDocument(doc, new Dictionary<string, int>());
        }
        private Dictionary<string,int> extractDocument(String doc, Dictionary<string, int> extractedDoc)
        {
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
        private DbDataReader getData()
        {
            string ftsQuery = @"select *
                               from amod.ftsearchdata 
                                 order by ftsModified;";
            return executeQuery(ftsQuery);
        }

        private DbDataReader executeQuery(String query)
        {
            DbCommand cmd = new MySqlCommand(query, conn);
            return(cmd.ExecuteReader());
        }

        private DbDataReader getNewData(String lastRecordDate)
        {
            string ftsQueryNewData = @"select *
                                from amod.ftsearchdata
                                where ftsModified > \''2010-11-00 12:00:00\''
                                order by ftsModified;";
            return executeQuery(ftsQueryNewData);
        }

        public Dictionary<int, Dictionary<string, int> > createDictionaryFromReader(DbDataReader rdr)
        {
            Dictionary<int, Dictionary<string, int>> data = new Dictionary<int, Dictionary<string, int>>();
            while (rdr.Read())
            {
                if (!data.ContainsKey((int)rdr["ftsCaseId"]))
                    data.Add((int)rdr["ftsCaseId"], extractDocument((string)rdr["ftsText"]));
                else
                    extractDocument((string)rdr["ftsText"], data[(int)rdr["ftsCaseId"]];
            }
            return data;
        }
        private void disconnect()
        {
            conn.Close();
        }

        public int checkProcedure(int caseId)
        {
            string checkProcedureQuery = @"select *
                               from amod.casedefinition 
                               where caseProcedurId =" + caseId.ToString +
                                 ";";
            return (int)executeQuery(checkProcedureQuery)[0];
        }

        public void update(DBRepresentation dBRepresentation)
        {
            connect();
            DbDataReader rdr = getNewData("");
            disconnect();
        }
    }
} 
