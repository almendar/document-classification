﻿using System;
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
        public int getProcedureId(int caseId)
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
                            AllCases.Instance[caseId].Add(word, 0);
                            if (!newWords.Contains(word))
                                newWords.Add(word);

                        }
                    }
                }
                else
                {
                    AllCases.Instance.Add(caseId, new Case(getProcedureId(caseId), caseId));
                    newCases.Add(caseId, data[caseId]);
                    CasesTF.Instance.Add(newCases);
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
                IDFcalculaction.Instance.setNumberOfCases(AllCases.Instance.getNumberOfCasesInDB() + newCases.Count);
                IDFcalculaction.Instance.calculateIDF();
                foreach (Case tempCase in AllCases.Instance.Values)
                {
                    foreach (string word in tempCase.Keys)
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
                        AllCases.Instance[caseId][word] += IDFcalculaction.Instance[word].IDF;    
                    }
                }

                // for new words - recalculate all words that appeared in all cases
                foreach (string word in newWords)
                {
                    IDFcalculaction.Instance.calculateIDF(word);
                    foreach (int caseId in AllCases.Instance.Keys)
                    {
                        if (AllCases.Instance[caseId].ContainsKey(word))
                        {
                            AllCases.Instance[caseId][word] = calculateTFIDF(caseId, word);
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
            this.D = D;
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