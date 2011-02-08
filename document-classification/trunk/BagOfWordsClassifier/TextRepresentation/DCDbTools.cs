namespace DocumentClassification.Representation
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    using DocumentClassification.BagOfWordsClassifier.Matrices;

    using MySql.Data.MySqlClient;
    using DocumentClassification.DCUpdate;

    /// <summary>
    /// DCDbTools holds all methods responsible for operations on DC database
    /// </summary>
    public class DCDbTools
    {
        #region Fields

        private const string database = "dc";
        private const string pwd = "1207pegazo";
        private const string server = "localhost";
        private const string uid = "root";

        private static readonly DCDbTools instance = new DCDbTools();

        private string lastDbRecordDate;

        //private MySqlConnection conn;
        private int CurrentVersion;

        #endregion Fields

        #region Constructors

        static DCDbTools()
        {
        }

        private DCDbTools()
        {
            lastDbRecordDate = "1900-12-31 23:59:59";
            CurrentVersion = -1;
        }

        #endregion Constructors

        #region Properties

        public static DCDbTools Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion Properties

        #region Methods

        public string LastDbRecordDate
        {
            get
            {
                return lastDbRecordDate;
            }
            set
            {
                lastDbRecordDate = value;
            }
        }

        /// <summary>
        /// Get connection to db
        /// </summary>
        /// <returns>new open MySqlConnection connection</returns>
        public MySqlConnection GetConnection()
        {
            MySqlConnection conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            return conn;
        }


        /// <summary>
        /// Get newest CasesTF from db
        /// </summary>
        /// <returns>CasesTF</returns>
        public CasesTF getCasesTF()
        {
            CasesTF result = (CasesTF)getObject("casestf");
            return result != null ? result : new CasesTF(); 
        }

        /// <summary>
        /// Get newest IDFcalculation from database
        /// </summary>
        /// <returns>IDFcalculation</returns>
        public IDFcalculation getIDFcalculation()
        {
            IDFcalculation result = (IDFcalculation)getObject("idfcalculation");
            return result != null ? result : new IDFcalculation(); 
        }
        /// <summary>
        /// Get ProcedureMatrices from database
        /// </summary>
        /// <returns>ProcedureMatrices</returns>
        public ProcedureMatrices getProcedureMatrices()
        {
            ProcedureMatrices result = (ProcedureMatrices)getObject("procedurematrices");
            return result;
        }
        /// <summary>
        /// Get NextStageMatrices from database 
        /// </summary>
        /// <returns></returns>
        public NextDecisionMatrices getNextStageMatrices()
        {
            NextDecisionMatrices result = (NextDecisionMatrices)getObject("nextstagematrices");
            return result;
        }
        /// <summary>
        /// Get NextDecisionMatrices from database
        /// </summary>
        /// <returns>NextDecisionMatrices</returns>
        public NextDecisionMatrices getNextPersonMatrices()
        {
            NextDecisionMatrices result = (NextDecisionMatrices)getObject("nextpersonmatrices");
            return result;
        }

        /// <summary>
        /// Load newest Data (DBRepresentation, AllCases, AllDecisionStatus, AllProcedures, AllDecisionPeople) from database to Data.Instance;
        /// If database versionHistory is empty do nothing.
        /// </summary>
        public void loadData()
        {
            lock (Data.Instance)
            {
                setCurrentVersion();
                if (CurrentVersion != -1)
                {
                    setLastDbRecordDate();
                    loadDBRepresentation();
                    loadAllCases();
                    loadAllDecisionsStatus();
                    loadAllProcedures();
                    loadAllDecisionsPeople();
                }
            }
        }

        /// <summary>
        /// Load newest Matrices from database to DataMatrices.Instance
        /// If database versionHistory is empty do nothing
        /// </summary>
        public void loadMatricesFromDb()
        {
            setCurrentVersion();
            if (CurrentVersion != -1)
            {
                setLastDbRecordDate();
                loadNextPersonMatrices();
                loadNextStageMatrices();
                loadProcedureMatrices();
                loadWordPicker();
            }
        }

        /// <summary>
        /// Send current data from Data.Instance to database
        /// </summary>
        public void sendData()
        {
            lock (Data.Instance)
            {
                MySqlConnection conn = GetConnection();
                createNewVersion(conn,AmodDBTools.Instance.LastUpdateDate);
                startTransaction(conn);
                sendAllCases(conn);
                sendAllDecisionsPeople(conn);
                sendAllDecisionsStatus(conn);
                sendAllProcedures(conn);
                sendDBRepresentation(conn);
                sendCasesTF(conn);
                sendIDFcalculation(conn);
                commit(conn);
                conn.Close();
            }
        }

        /// <summary>
        /// Send current data from DataMatrices to database
        /// </summary>
        public void sendDataMatricesToDb()
        {
            MySqlConnection conn = GetConnection();
            //connect();
            startTransaction(conn);
            sendNextPersonMatrices(conn);
            sendNextStageMatrices(conn);
            sendProcedureMatrices(conn);
            sendWordPicker(conn);
            commit(conn);
            //disconnect();
            conn.Close();
        }

        private void commit(MySqlConnection conn)
        {
            string Query = "COMMIT;";
            executeNonQuery(Query, conn);
        }

        private void createNewVersion(MySqlConnection conn)
        {
            createNewVersion(conn, System.DateTime.Now.ToString());
        }

        private void createNewVersion(MySqlConnection conn, string lastUpdate)
        {
            string Query = "INSERT INTO dc.versionHistory(imageDate) values" +
             "('" + lastUpdate + "');" +
             "select LAST_INSERT_ID();";
            DbDataReader rdr = executeQuery(Query, conn);
            rdr.Read();
            CurrentVersion = rdr.GetInt32(0);
            rdr.Close();
        }

        private void executeNonQuery(String nonQuery, MySqlConnection conn)
        {
            DbCommand cmd = new MySqlCommand(nonQuery, conn);
            cmd.ExecuteNonQuery();
        }

        private DbDataReader executeQuery(String query, MySqlConnection conn)
        {
            DbCommand cmd = new MySqlCommand(query, conn);
            return (cmd.ExecuteReader());
        }

        private void loadAllCases()
        {
            Data.Instance.AllCases = (AllCases)getObject("allcases");
        }

        private void loadAllDecisionsPeople()
        {
            Data.Instance.AllDecisionsPeople = (AllDecisions)getObject("alldecisionspeople");
        }

        private void loadAllDecisionsStatus()
        {
            Data.Instance.AllDecisionsStatus = (AllDecisions)getObject("alldecisionsstatus");
        }

        private void loadAllProcedures()
        {
            Data.Instance.AllProcedures = (AllProcedures)getObject("allprocedures");
        }

        private string GetConnectionString()
        {
            return "Server=" + server + ";Database=" + database + ";Uid=" + uid + ";Pwd=" + pwd + ";";
        }

        private void loadDBRepresentation()
        {
            Data.Instance.DBRepresentation = (DBRepresentation)getObject("dbrepresentation");
        }

        private void loadNextPersonMatrices()
        {
            DataMatrices.Instance.NextPersonMatrices = (NextDecisionMatrices)getObject("nextpersonmatrices");
        }

        private void loadNextStageMatrices()
        {
            DataMatrices.Instance.NextStageMatrices = (NextDecisionMatrices)getObject("nextstagematrices");
        }

        private object getObject(string tableName)
        {
            MySqlConnection conn = GetConnection();
            string Query = "SELECT * FROM dc." + tableName +
                           " where versionhistory_idversionHistory = " + CurrentVersion + ";";
            DbDataReader rdr = executeQuery(Query,conn);
            object result = null;
            if (rdr.Read())
            {
                BinaryFormatter bf = new BinaryFormatter();
                System.IO.MemoryStream mem =
                new System.IO.MemoryStream(Convert.FromBase64String(rdr.GetString(1)));
                result = bf.Deserialize(mem);
            }
            conn.Close();
            return result;
        }

        private void loadProcedureMatrices()
        {
            DataMatrices.Instance.ProcedureMatrices = (ProcedureMatrices)getObject("procedurematrices");
        }

        private void loadWordPicker()
        {
            DataMatrices.Instance.WordPicker = (WordPicker)getObject("wordpicker");
        }

        private void sendAllCases(MySqlConnection conn)
        {
            sendObject("allcases", Data.Instance.AllCases, conn);
        }

        private void sendAllDecisionsPeople(MySqlConnection conn)
        {
            sendObject("alldecisionspeople", Data.Instance.AllDecisionsPeople, conn);
        }

        private void sendAllDecisionsStatus(MySqlConnection conn)
        {
            sendObject("alldecisionsstatus", Data.Instance.AllDecisionsStatus, conn);
        }

        private void sendAllProcedures(MySqlConnection conn)
        {
            sendObject("allprocedures", Data.Instance.AllProcedures, conn);
        }

        private void sendCasesTF(MySqlConnection conn)
        {
            sendObject("casestf", IDFcalculationHelper.Instance.CasesTF, conn);
        }

        private void sendDBRepresentation(MySqlConnection conn)
        {
            sendObject("dbrepresentation", Data.Instance.DBRepresentation, conn);
        }

        private void sendIDFcalculation(MySqlConnection conn)
        {
            sendObject("idfcalculation", IDFcalculationHelper.Instance.IDFcalculation, conn);
        }

        private void sendNextPersonMatrices(MySqlConnection conn)
        {
            sendObject("nextpersonmatrices", DataMatrices.Instance.NextPersonMatrices, conn);
        }

        private void sendNextStageMatrices(MySqlConnection conn)
        {
            sendObject("nextstagematrices", DataMatrices.Instance.NextStageMatrices, conn);
        }

        private void sendObject(string tableName, object obj, MySqlConnection conn)
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            bf.Serialize(mem, obj);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc." + tableName +
                "(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            executeNonQuery(Query,conn);
        }

        private void sendProcedureMatrices(MySqlConnection conn)
        {
            sendObject("procedurematrices", DataMatrices.Instance.ProcedureMatrices, conn);
        }

        private void sendWordPicker(MySqlConnection conn)
        {
            sendObject("wordpicker", DataMatrices.Instance.WordPicker, conn);
        }

        private void setCurrentVersion()
        {
            MySqlConnection conn = GetConnection();
            string Query = "SELECT MAX(idversionHistory) FROM versionhistory;";
            DbDataReader rdr = executeQuery(Query,conn);
            if (rdr.Read() && !rdr.IsDBNull(0))
            {
                CurrentVersion = rdr.GetInt32(0);
            }
            else
                CurrentVersion = -1;
            conn.Close();
        }

        private void setLastDbRecordDate()
        {
            MySqlConnection conn = GetConnection();
            string Query = @"SELECT imageDate FROM versionhistory
                            WHERE idversionHistory = " + CurrentVersion + ";"; 
            DbDataReader rdr = executeQuery(Query,conn);
            rdr.Read();
            lastDbRecordDate = rdr.GetString(0);
            conn.Close();
        }

        private void startTransaction(MySqlConnection conn)
        {
            string Query = "START TRANSACTION;";
            executeNonQuery(Query, conn);
        }

        #endregion Methods
    }
}