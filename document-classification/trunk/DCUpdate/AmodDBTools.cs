using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Web;
using System.Data.Common;
using System.Data.SqlClient;
using DocumentClassification.Representation;

namespace DocumentClassification.DCUpdate
{
    public class AmodDBTools
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
            return executeQuery(query, conn);
        }
        private DbDataReader getNewData(String lastRecordDate)
        {
            string ftsQueryNewData = @"select *
                                from amod.ftsearchdata
                                where ftsModified > '" + lastRecordDate +
                                "' order by ftsModified;";
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
                    extractDocument((string)rdr["ftsText"], data[(int)rdr["ftsCaseId"]]);
                lastRecord = rdr["ftsModified"].ToString();
            }
            return data;
        }
        /** return procedure Id for particular case */
        public int getProcedureId(int caseId)
        {
            MySqlConnection connection = new MySqlConnection();
            connection.ConnectionString = connectionString;
            connection.Open();
            string checkProcedureQuery = @"select caseProcedureId
                               from amod.casedefinition
                               where caseId =" + caseId.ToString() +
                                  ";";
            DbDataReader rdr = executeQuery(checkProcedureQuery, connection);
            int result = 0;
            if (rdr.Read())
            {
                result = (int)(rdr[0]);
            }
            else
            {
                //@TODO exception
            }
            connection.Close();
            return result;
        }

        private DbDataReader executeQuery(string checkProcedureQuery, MySqlConnection connection)
        {
            DbCommand cmd = new MySqlCommand(checkProcedureQuery, connection);
            return(cmd.ExecuteReader());
        }
        public void update()
        {
            connect();
            lastUpdate = "2007-12-13 11:26:25";
            DbDataReader rdr = getNewData(lastUpdate);
            Dictionary<int, Dictionary<string, int> > data = createDictionaryFromReader(rdr);
            rdr.Close();
            
            // splitting data and updating AllCases structure
            List<string> newWords = new List<string>();
            Dictionary<int, Dictionary<string, int>> newCases = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> oldCasesNewWords = new Dictionary<int, Dictionary<string, int>>();
            Dictionary<int, Dictionary<string, int>> oldCasesOldWords = new Dictionary<int, Dictionary<string, int>>();
            foreach (int caseId in data.Keys)
            {
                if (CasesTF.Instance.ContainsKey(caseId))
                {
                    oldCasesOldWords.Add(caseId, new Dictionary<string, int> ());
                    oldCasesNewWords.Add(caseId, new Dictionary<string, int> ());
                    foreach (string word in data[caseId].Keys)
                    {
                        if (CasesTF.Instance[caseId].ContainsKey(word))
                        {
                            oldCasesOldWords[caseId].Add(word, data[caseId][word]);
                            CasesTF.Instance[caseId].Add(word, oldCasesOldWords[caseId][word]);
                        }
                        else
                        {
                            oldCasesNewWords[caseId].Add(word, data[caseId][word]);
                            CasesTF.Instance[caseId].Add(word, oldCasesNewWords[caseId][word]);
                            Data.Instance.AllCases[caseId].Add(word, 0);
                            if (!newWords.Contains(word))
                                newWords.Add(word);

                        }
                    }
                }
                else
                {
                    Data.Instance.AllCases.Add(caseId, new Case(getProcedureId(caseId), caseId));
                    foreach (string word in data[caseId].Keys)
                    {
                        Data.Instance.AllCases[caseId].Add(word, 0);
                    }
                    newCases.Add(caseId, data[caseId]);
                    CasesTF.Instance.Add(caseId, data[caseId]);
                }
            }

            // updating DBRepresentation and IDFCaclulaction structure
            updateDBRepresentation(newCases);
            updateDBRepresentation(oldCasesNewWords);


            // recalculate TF-IDF
            // if new cases appeared recalculate all 
            if (newCases.Count != 0)
            {
                // recalculating IDF
                IDFcalculaction.Instance.setNumberOfCases(Data.Instance.AllCases.getNumberOfCasesInDB() + newCases.Count);
                IDFcalculaction.Instance.calculateIDF();
                foreach (Case tempCase in Data.Instance.AllCases.Values)
                {
                    List<string> tempList = new List<string>(tempCase.Keys);
                    foreach(string word in tempList)
                    {
                        tempCase[word] = calculateTFIDF(tempCase.CaseId, word);
                    }
                }
            }
            else
            {
                // old words - recalculate only TF-IDF in this document
                foreach (int caseId in oldCasesOldWords.Keys)
                {
                    foreach (string word in oldCasesOldWords[caseId].Keys)
                    {
                        Data.Instance.AllCases[caseId][word] += IDFcalculaction.Instance[word].IDF;    
                    }
                }

                // for new words - recalculate all words that appeared in all cases
                foreach (string word in newWords)
                {
                    IDFcalculaction.Instance.calculateIDF(word);
                    foreach (int caseId in Data.Instance.AllCases.Keys)
                    {
                        if (Data.Instance.AllCases[caseId].ContainsKey(word))
                        {
                            Data.Instance.AllCases[caseId][word] = calculateTFIDF(caseId, word);
                        }
                    }
                }
            }
            rdr.Close();
            updateAllDecisionsStatus();
            disconnect();
        }
        private void updateAllDecisionsStatus()
        {
            string Query = @"SELECT `caseId`, `caseVersion`, `caseStatusId`, `caseModified`, `casePrevStatusId`, `caseNextStatusId` 
                            FROM `amod`.`casehistory` 
                            WHERE `caseNextStatusId` IS NOT NULL AND `caseNextStatusId` <> `caseStatusId` 
                            ORDER BY `caseId`, `caseVersion`;";
            DbDataReader rdr = executeQuery(Query);

            int caseId = -1;
            int procedureId = -1;
            while (rdr.Read())
            {
                if (caseId != rdr.GetInt32(0))
                {
                    caseId = rdr.GetInt32(0);
                    procedureId = getProcedureId(caseId);
                    Int32 currentStatus = rdr.GetInt32(2);
                    Int32? prevStatus = null;
                    if (!rdr.IsDBNull(4))
                        prevStatus = rdr.GetInt32(4);

                    Int32 nextStatus = rdr.GetInt32(5);
                    if (!Data.Instance.AllDecisionsStatus.ContainsKey(procedureId))
                    {
                        Data.Instance.AllDecisionsStatus.Add(procedureId, new Dictionary<int, Dictionary<int, DecisionRepresentationStatus>>());
                        if (prevStatus == null)
                        {
                            Data.Instance.AllDecisionsStatus[procedureId].Add(currentStatus, new Dictionary<int, DecisionRepresentationStatus>());
                            Data.Instance.AllDecisionsStatus[procedureId][currentStatus].Add(nextStatus, new DecisionRepresentationStatus(Data.Instance.AllCases[caseId]));
                        }
                    }
                    else
                    {
                        if (prevStatus == null)
                        {
                            addConnection(caseId, procedureId, currentStatus, nextStatus);
                        }
                        else
                        {
                            addConnection(caseId, procedureId, (int)prevStatus, currentStatus);
                            addConnection(caseId, procedureId, currentStatus, nextStatus);
                        }
                    }
                }
                else
                {
                    addConnection(caseId, procedureId, rdr.GetInt32(2), rdr.GetInt32(5));
                }
            }
            rdr.Close();
        }

        private static void addConnection(int caseId, int procedureId, Int32 currentStatus, Int32 nextStatus)
        {
            if (!Data.Instance.AllDecisionsStatus[procedureId].ContainsKey(currentStatus))
            {
                Data.Instance.AllDecisionsStatus[procedureId].Add(currentStatus, new Dictionary<int, DecisionRepresentationStatus>());
                Data.Instance.AllDecisionsStatus[procedureId][currentStatus].Add(nextStatus, new DecisionRepresentationStatus(Data.Instance.AllCases[caseId]));
            }
            else
            {
                if (!Data.Instance.AllDecisionsStatus[procedureId][currentStatus].ContainsKey(nextStatus))
                {
                    Data.Instance.AllDecisionsStatus[procedureId][currentStatus].Add(nextStatus, new DecisionRepresentationStatus(Data.Instance.AllCases[caseId]));
                }
                else
                {
                    foreach (String word in Data.Instance.AllCases[caseId].Keys)
                    {
                        if (!Data.Instance.AllDecisionsStatus[procedureId][currentStatus][nextStatus].ContainsKey(word))
                        {
                            Data.Instance.AllDecisionsStatus[procedureId][currentStatus][nextStatus].Add(word, Data.Instance.AllCases[caseId][word]);
                        }
                        else
                        {
                            Data.Instance.AllDecisionsStatus[procedureId][currentStatus][nextStatus][word] += Data.Instance.AllCases[caseId][word];
                        }
                    }
                }
            }
        }
        private void updateDBRepresentation(Dictionary<int, Dictionary<string, int> > data)
        {
            foreach(Dictionary<string, int> tempCase in data.Values)
            {
                foreach(string word in tempCase.Keys)
                {
                    if(Data.Instance.DBRepresentation.ContainsKey(word))
                    {
                        Data.Instance.DBRepresentation[word]++;
                        IDFcalculaction.Instance.dfChanged(word, Data.Instance.DBRepresentation[word]);
                    }
                    else
                    {
                        Data.Instance.DBRepresentation.Add(word, 1);
                        IDFcalculaction.Instance.Add(word, new IDFData(1));
                    }
                }
            }
        }
        private double calculateTFIDF(int caseId, string word)
        {
            return (double)CasesTF.Instance[caseId][word] * IDFcalculaction.Instance[word].IDF;
        }
    }
    public class CasesTF : Dictionary<int, Dictionary<string, int> >
    {
        static readonly CasesTF instance = new CasesTF();
        private CasesTF()
        {
        }
        public static CasesTF Instance
        {
            get
            {
                return instance;
            }
        }
        public void Add(Dictionary<int, Dictionary<string, int> > data)
        {
            foreach(int caseId in data.Keys)
            {
                Add(caseId, data[caseId]);
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
        public void calculateIDF()
        {
            logD = Math.Log10(D);

            foreach (IDFData data in this.Values)
            {
                data.calculateIDF(logD);
            }
        }
        public double calculateIDF(string word)
        {
            return this[word].calculateIDF(logD);
        }
    
        public void setNumberOfCases(int D)
        {
            this.D = D;
            logD = Math.Log10(D);
        }
    }
    public class IDFData
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