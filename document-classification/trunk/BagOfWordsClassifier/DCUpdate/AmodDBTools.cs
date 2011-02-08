namespace DocumentClassification.DCUpdate
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Text;
    using System.Web;
    using DocumentClassification.BagOfWordsClassifier.Matrices;
    using DB_AMODClasses.Tools;

    using DocumentClassification.Representation;

    using MySql.Data.MySqlClient;

    public class AmodDBTools
    {
        #region Fields

        static readonly AmodDBTools instance = new AmodDBTools();

        private string currentUpdateDate;
        private string lastUpdateDate;

        private bool hasNewData;

        #endregion Fields

        #region Constructors

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AmodDBTools()
        {
        }

        private AmodDBTools()
        {
            hasNewData = false;
            DCDbTools.Instance.loadData();
            lastUpdateDate = DCDbTools.Instance.LastDbRecordDate;
        }

        #endregion Constructors

        #region Properties

        public static AmodDBTools Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion Properties

        #region Methods

        public string LastUpdateDate
        {
            get
            {
                return lastUpdateDate;
            }
            set
            {
                lastUpdateDate = value;
            }
        }
        /// <summary>
        /// checks if new data occured
        /// </summary>
        public bool HasNewData
        {
            get
            {
                return hasNewData;
            }
            set
            {
                hasNewData = value;
            }
        }

        /// <summary>
        /// Rebuild all training data
        /// </summary>
        public void rebuild()
        {
            // change lastUpdateDate to some date in past
            lastUpdateDate = "1900-12-31 23:59:59";
            update();
        }

        /// <summary>
        /// Update TrainingData.
        /// </summary>
        public void update()
        {
            DbConnection conn =  DatabaseTool.GetConnection();

            currentUpdateDate = System.DateTime.Now.ToString();
            DbDataReader rdr = getFtsearchdata(lastUpdateDate, currentUpdateDate, conn);


            if (rdr.HasRows)
            {
                hasNewData = true;
                lock (Data.Instance)
                {
                    Dictionary<int, Dictionary<string, int>> data = transformFtsearchDataToDictionary(rdr);
                    rdr.Close();

                    //#region updating_allcases
                    // splitting data and updating AllCases structure
                    List<string> newWords = new List<string>();
                    Dictionary<int, Dictionary<string, int>> newCases = new Dictionary<int, Dictionary<string, int>>();
                    Dictionary<int, Dictionary<string, int>> oldCasesNewWords = new Dictionary<int, Dictionary<string, int>>();
                    Dictionary<int, Dictionary<string, int>> oldCasesOldWords = new Dictionary<int, Dictionary<string, int>>();
                    updateAllCases(data, newWords, newCases, oldCasesNewWords, oldCasesOldWords);
                    //#endregion updating_allcases

                    // updating DBRepresentation and IDFCaclulaction structure
                    updateDBRepresentation(newCases);
                    updateDBRepresentation(oldCasesNewWords);

                    // recalculate TF-IDF
                    // if new cases appeared recalculate all
                    #region TF_IDF
                    if (newCases.Count != 0)
                    {
                        // setting number of cases
                        IDFcalculationHelper.Instance.IDFcalculation.setNumberOfCases(Data.Instance.AllCases.getNumberOfCasesInDB() + newCases.Count);
                        recalculateAllTFIDF();
                    }
                    else
                        recalculateTFIDFFofSelectedRecords(newWords, oldCasesOldWords);
                    #endregion TF_IDF
                    rdr.Close();

                    updateAllDecisionsStatus(conn);
                    updateAllDecisionsPeople(conn);
                    Data.Instance.AllProcedures.rebuild(Data.Instance.AllCases);

                }
            }
            lastUpdateDate = currentUpdateDate;
            conn.Close();
            DataMatrices.Instance.rebuildDataMatrices();
        }

        private static void addStatusConnection(int caseId, int procedureId, Int32 currentStatus, Int32 nextStatus)
        {
            if (!Data.Instance.AllDecisionsStatus[procedureId].ContainsKey(currentStatus))
            {
                Data.Instance.AllDecisionsStatus[procedureId].Add(currentStatus, new Dictionary<int, TextRepresentation>());
                Data.Instance.AllDecisionsStatus[procedureId][currentStatus].Add(nextStatus, new TextRepresentation(Data.Instance.AllCases[caseId]));
            }
            else
            {
                if (!Data.Instance.AllDecisionsStatus[procedureId][currentStatus].ContainsKey(nextStatus))
                {
                    Data.Instance.AllDecisionsStatus[procedureId][currentStatus].Add(nextStatus, new TextRepresentation(Data.Instance.AllCases[caseId]));
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

        private void addPeopleConnection(int caseId, int procedureId, int currentOwner, int nextOwner)
        {
            if (!Data.Instance.AllDecisionsPeople[procedureId].ContainsKey(currentOwner))
            {
                Data.Instance.AllDecisionsPeople[procedureId].Add(currentOwner, new Dictionary<int, TextRepresentation>());
                Data.Instance.AllDecisionsPeople[procedureId][currentOwner].Add(nextOwner, new TextRepresentation(Data.Instance.AllCases[caseId]));
            }
            else
            {
                if (!Data.Instance.AllDecisionsPeople[procedureId][currentOwner].ContainsKey(nextOwner))
                {
                    Data.Instance.AllDecisionsPeople[procedureId][currentOwner].Add(nextOwner, new TextRepresentation(Data.Instance.AllCases[caseId]));
                }
                else
                {
                    foreach (String word in Data.Instance.AllCases[caseId].Keys)
                    {
                        if (!Data.Instance.AllDecisionsPeople[procedureId][currentOwner][nextOwner].ContainsKey(word))
                        {
                            Data.Instance.AllDecisionsPeople[procedureId][currentOwner][nextOwner].Add(word, Data.Instance.AllCases[caseId][word]);
                        }
                        else
                        {
                            Data.Instance.AllDecisionsPeople[procedureId][currentOwner][nextOwner][word] += Data.Instance.AllCases[caseId][word];
                        }
                    }
                }
            }
        }

        private double calculateTFIDF(int caseId, string word)
        {
            return (double)IDFcalculationHelper.Instance.CasesTF[caseId][word] * IDFcalculationHelper.Instance.IDFcalculation[word].IDF;
        }

        private DbDataReader executeQuery(string checkProcedureQuery, DbConnection connection)
        {
            DbCommand cmd = new MySqlCommand(checkProcedureQuery, (MySqlConnection)connection);
            return (cmd.ExecuteReader());
        }

        private Dictionary<string, int> extractDocument(String doc)
        {
            return extractDocument(doc, new Dictionary<string, int>());
        }

        private Dictionary<string, int> extractDocument(String doc, Dictionary<string, int> extractedDoc)
        {
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            string[] words = doc.Split(delimiterChars);

            foreach (string s in words)
            {
                if (extractedDoc.ContainsKey(s))
                    extractedDoc[s]++;
                else
                    extractedDoc.Add(s, 1);
            }
            return extractedDoc;
        }

        private DbDataReader getFtsearchdata(String beginDate, String endDate, DbConnection conn)
        {
            string ftsQueryData = @"select *
                                from amod.ftsearchdata
                                where ftsModified > '" + beginDate +
                                "' AND ftsModified < '" + endDate +
                                "' order by ftsModified;";
            return executeQuery(ftsQueryData, conn);
        }

        private DbDataReader getFtsearchdata(String endDate, DbConnection conn)
        {
            string ftsQueryData = @"select *
                                from amod.ftsearchdata
                                where ftsModified < '" + endDate +
                                "' order by ftsModified;";
            return executeQuery(ftsQueryData, conn);
        }

        /** return procedure Id for particular case */
        private int getProcedureId(int caseId)
        {
            DbConnection conn = DatabaseTool.GetConnection();

            string checkProcedureQuery = @"select caseProcedureId
                               from amod.casedefinition
                               where caseId =" + caseId.ToString() +
                                  ";";
            DbDataReader rdr = executeQuery(checkProcedureQuery, conn);
            int result = 0;
            if (rdr.Read())
            {
                result = (int)(rdr[0]);
            }
            else
            {
                //@TODO exception
            }
            conn.Close();
            return result;
        }

        private void recalculateAllTFIDF()
        {
            IDFcalculationHelper.Instance.IDFcalculation.calculateIDF();
            foreach (TextRepresentation tempCase in Data.Instance.AllCases.Values)
            {
                List<string> tempList = new List<string>(tempCase.Keys);
                foreach (string word in tempList)
                {
                    tempCase[word] = calculateTFIDF(tempCase.CaseId, word);
                }
            }
        }

        private void recalculateTFIDFFofSelectedRecords(List<string> newWords, Dictionary<int, Dictionary<string, int>> oldCasesOldWords)
        {
            // old words - recalculate only TF-IDF in this document
            foreach (int caseId in oldCasesOldWords.Keys)
            {
                foreach (string word in oldCasesOldWords[caseId].Keys)
                {
                    Data.Instance.AllCases[caseId][word] += IDFcalculationHelper.Instance.IDFcalculation[word].IDF;
                }
            }

            // for new words - recalculate all words that appeared in all cases
            foreach (string word in newWords)
            {
                IDFcalculationHelper.Instance.IDFcalculation.calculateIDF(word);
                foreach (int caseId in Data.Instance.AllCases.Keys)
                {
                    if (Data.Instance.AllCases[caseId].ContainsKey(word))
                    {
                        Data.Instance.AllCases[caseId][word] = calculateTFIDF(caseId, word);
                    }
                }
            }
        }

        private Dictionary<int, Dictionary<string, int>> transformFtsearchDataToDictionary(DbDataReader rdr)
        {
            Dictionary<int, Dictionary<string, int>> data = new Dictionary<int, Dictionary<string, int>>();
            while (rdr.Read())
            {
                if (!data.ContainsKey((int)rdr["ftsCaseId"]))
                    data.Add((int)rdr["ftsCaseId"], extractDocument((string)rdr["ftsText"]));
                else
                    extractDocument((string)rdr["ftsText"], data[(int)rdr["ftsCaseId"]]);
                currentUpdateDate = rdr["ftsModified"].ToString();
            }
            return data;
        }

        private void updateAllCases(Dictionary<int, Dictionary<string, int>> data, List<string> newWords, Dictionary<int, Dictionary<string, int>> newCases, Dictionary<int, Dictionary<string, int>> oldCasesNewWords, Dictionary<int, Dictionary<string, int>> oldCasesOldWords)
        {
            foreach (int caseId in data.Keys)
            {
                if (IDFcalculationHelper.Instance.CasesTF.ContainsKey(caseId))
                {
                    oldCasesOldWords.Add(caseId, new Dictionary<string, int>());
                    oldCasesNewWords.Add(caseId, new Dictionary<string, int>());
                    foreach (string word in data[caseId].Keys)
                    {
                        if (IDFcalculationHelper.Instance.CasesTF[caseId].ContainsKey(word))
                        {
                            oldCasesOldWords[caseId].Add(word, data[caseId][word]);
                            //IDFcalculationHelper.Instance.CasesTF[caseId][word].
                        }
                        else
                        {
                            oldCasesNewWords[caseId].Add(word, data[caseId][word]);
                            IDFcalculationHelper.Instance.CasesTF[caseId].Add(word, oldCasesNewWords[caseId][word]);
                            Data.Instance.AllCases[caseId].Add(word, 0);
                            if (!newWords.Contains(word))
                                newWords.Add(word);
                        }
                    }
                }
                else
                {
                    Data.Instance.AllCases.Add(caseId, new TextRepresentation(getProcedureId(caseId), caseId));
                    foreach (string word in data[caseId].Keys)
                    {
                        Data.Instance.AllCases[caseId].Add(word, 0);
                    }
                    newCases.Add(caseId, data[caseId]);
                    IDFcalculationHelper.Instance.CasesTF.Add(caseId, data[caseId]);
                }
            }
        }

        private void updateAllDecisionsPeople(DbConnection conn)
        {
            string Query = @"SELECT `caseId`, `caseVersion`, `caseOwnerId`, `caseModified`, `casePrevOwnerId`, `caseNextOwnerId`
                            FROM `amod`.`casehistory`
                            WHERE `caseNextOwnerId` IS NOT NULL AND `caseNextOwnerId` <> `caseOwnerId`
                            AND caseModified > '" + lastUpdateDate +
                           "' AND caseModified < '" + currentUpdateDate +
                           "' ORDER BY `caseOwnerId`, `caseVersion`;";
            DbDataReader rdr = executeQuery(Query, conn);

            int caseId = -1;
            int procedureId = -1;
            while (rdr.Read())
            {
                if (caseId != rdr.GetInt32(0))
                {
                    caseId = rdr.GetInt32(0);
                    procedureId = getProcedureId(caseId);
                    Int32 currentOwner = rdr.GetInt32(2);
                    Int32? prevOwner = null;
                    if (!rdr.IsDBNull(4))
                        prevOwner = rdr.GetInt32(4);

                    Int32 nextStatus = rdr.GetInt32(5);
                    if (!Data.Instance.AllDecisionsPeople.ContainsKey(procedureId))
                    {
                        Data.Instance.AllDecisionsPeople.Add(procedureId, new Dictionary<int, Dictionary<int, TextRepresentation>>());
                        if (prevOwner == null)
                        {
                            Data.Instance.AllDecisionsPeople[procedureId].Add(currentOwner, new Dictionary<int, TextRepresentation>());
                            Data.Instance.AllDecisionsPeople[procedureId][currentOwner].Add(nextStatus, new TextRepresentation(Data.Instance.AllCases[caseId]));
                        }
                    }
                    else
                    {
                        if (prevOwner == null)
                        {
                            addPeopleConnection(caseId, procedureId, currentOwner, nextStatus);
                        }
                        else
                        {
                            addPeopleConnection(caseId, procedureId, (int)prevOwner, currentOwner);
                            addPeopleConnection(caseId, procedureId, currentOwner, nextStatus);
                        }
                    }
                }
                else
                {
                    addPeopleConnection(caseId, procedureId, rdr.GetInt32(2), rdr.GetInt32(5));
                }
            }
            rdr.Close();
        }

        private void updateAllDecisionsStatus(DbConnection conn)
        {
            string Query = @"SELECT `caseId`, `caseVersion`, `caseStatusId`, `caseModified`, `casePrevStatusId`, `caseNextStatusId`
                            FROM `amod`.`casehistory`
                            WHERE `caseNextStatusId` IS NOT NULL AND `caseNextStatusId` <> `caseStatusId`
                            AND caseModified > '" + lastUpdateDate +
                           "' AND caseModified < '" + currentUpdateDate +
                           "' ORDER BY `caseId`, `caseVersion`;";
            DbDataReader rdr = executeQuery(Query, conn);

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
                        Data.Instance.AllDecisionsStatus.Add(procedureId, new Dictionary<int, Dictionary<int, TextRepresentation>>());
                        if (prevStatus == null)
                        {
                            Data.Instance.AllDecisionsStatus[procedureId].Add(currentStatus, new Dictionary<int, TextRepresentation>());
                            Data.Instance.AllDecisionsStatus[procedureId][currentStatus].Add(nextStatus, new TextRepresentation(Data.Instance.AllCases[caseId]));
                        }
                    }
                    else
                    {
                        if (prevStatus == null)
                        {
                            addStatusConnection(caseId, procedureId, currentStatus, nextStatus);
                        }
                        else
                        {
                            addStatusConnection(caseId, procedureId, (int)prevStatus, currentStatus);
                            addStatusConnection(caseId, procedureId, currentStatus, nextStatus);
                        }
                    }
                }
                else
                {
                    addStatusConnection(caseId, procedureId, rdr.GetInt32(2), rdr.GetInt32(5));
                }
            }
            rdr.Close();
        }

        private void updateDBRepresentation(Dictionary<int, Dictionary<string, int>> data)
        {
            foreach (Dictionary<string, int> tempCase in data.Values)
            {
                foreach (string word in tempCase.Keys)
                {
                    if (Data.Instance.DBRepresentation.ContainsKey(word))
                    {
                        Data.Instance.DBRepresentation[word]++;
                        IDFcalculationHelper.Instance.IDFcalculation.dfChanged(word, Data.Instance.DBRepresentation[word]);
                    }
                    else
                    {
                        Data.Instance.DBRepresentation.Add(word, 1);
                        IDFcalculationHelper.Instance.IDFcalculation.Add(word, new IDFData(1));
                    }
                }
            }
        }
        /// <summary>
        /// Get data (specified by caseId) from ftserachdata 
        /// </summary>
        /// <param name="caseId">caseId</param>
        /// <returns>Dicitonary with words and corresponding quantity value in specified case</returns>
        public Dictionary<string, int> getData(int caseId)
        {
            DbConnection conn = DatabaseTool.GetConnection();
            string ftsQueryData = @"select *
                                from amod.ftsearchdata
                                where ftsCaseId = " + caseId + ";";
            DbDataReader rdr = executeQuery(ftsQueryData, conn);
            Dictionary<string, int> result = new Dictionary<string,int>();

            while (rdr.Read())
            {
                extractDocument((string)rdr["ftsText"], result);
            }
            conn.Close();
            return result;
        }

        #endregion Methods
    }
}