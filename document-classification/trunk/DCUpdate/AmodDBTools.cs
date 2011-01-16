using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Web;
using System.Data.Common;
using System.Data.SqlClient;
using System.Math;
using document_classification.Case;
using document_classification.AllCases;

namespace document_classification
{
    public sealed class AmodDBTools
    {
        static readonly AmodDBTools instance = new AmodDBTools();

        private MySqlConnection conn;
        private string lastUpdate;
        private string lastRecord;
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
                lastRecord = rdr["ftsModified"].ToString();
            }
            return data;
        }
        /** return procedure Id for particular case */
        public int checkProcedure(int caseId)
        {
            string checkProcedureQuery = @"select *
                               from amod.casedefinition
                               where caseProcedurId =" + caseId.ToString() +
                                  ";";
            return (int)executeQuery(checkProcedureQuery)[0];
        }
        public void update(DBRepresentation dBRepresentation)
        {
            connect();
            DbDataReader rdr = getNewData("");
            disconnect();
        }
        public void update()
        {
            DbDataReader rdr = getNewData(lastUpdate);
            Dictionary<int, Dictionary<string, int> > data = createDictionaryFromReader(rdr);
            
            // splitting data
            Dictionary<int, Dictionary<string, int>> newCases = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> oldCasesNewWords = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> oldCasesOldWords = new Dictionary<int, Dictionary<string, int>>();
            foreach (int caseId in data.Keys)
            {
                if(CassesTF.Instance.ContainsKey(caseId))
                {
                    foreach(string word in data[caseId].Keys)
                    {
                        if(CassesTF.Instance[caseId].ContainsKey(word))
                            oldCasesOldWords.Add(caseId, data[caseId]);
                        else
                            oldCasesNewWords.Add(caseId, data[caseId]);
                    }
                }
                else
                    newCases.Add(caseId, data[caseId]);
            }

            // updating DBRepresentation
            updateDBRepresentation(newCases);
            updateDBRepresentation(oldCasesNewWords);

            // recalculating IDF
            IDFcalculaction.Instance.calcuateIDF(
            // update AllCasses
        }
        private void updateDBRepresentation(Dictionary<int, Dictionary<string, int> > data)
        {
            foreach(Dictionary<string, int> tempCase in data.Values)
            {
                foreach(string word in tempCase.Keys)
                {
                    if(DBRepresentation.Instance.ContainsKey(word))
                    {
                        DBRepresentation.Instance[word]++;
                        IDFcalculaction.Instance.dfChanged(word, DBRepresentation.Instance[word]);
                    }
                    else
                    {
                        DBRepresentation.Instance.Add(word, 1);
                        IDFcalculaction.Instance.Add(word, new IDFData(1));
                    }
                }
            }
        }
    }
    public class CassesTF : Dictionary<int, Dictionary<string, int>
    {
        static readonly CassesTF instance = new CassesTF();
        private CassesTF()
        {
        }
        public static CassesTF Instance
        {
            get
            {
                return instance;
            }
        }
    }
    public class IDFcalculaction : Dictionary<string, IDFData>
    {
        private static IDFcalculaction instance = new IDFcalculaction();
        public static IDFcalculaction Instance
        {
            get
            {
                return instance;
            }
        }
        private IDFcalculaction()
        {
        }
        private int D;
        private double logD;
        public void dfChanged(string word, int newDf)
        {
            this[word].LogDF = Math.Log10(newDf);    
        }
        public void calculateIDF(int D)
        {
            this.D = D;
            logD = Math.Log10(D);

            foreach (IDFData data in this.Values)
            {
                data.calculateIDF(logD);
            }
        }
        public double calcuateIDF(string word)
        {
            return this[word].calculateIDF(logD);
        }
    }
    private class IDFData
    {
        private double idf;
        private double logdf;
        public IDFData(double idf, double logdf)
        {
            this.idf = idf;
            this.logdf = logdf;
        }
        public IDFData(double df)
        {
            idf = 0;
            logdf = Math.Log10(df);
        }
        public double IDF
        {
            get
            {
                return idf;
            }
            set
            {
                idf = value;
            }
        }
        public double LogDF
        {
            get
            {
                return logdf;
            }
            set
            {
                logdf = value;
            }
        }
        public double calculateIDF(double logD)
        {
            return idf = logD - logdf;
        }
    }
} 