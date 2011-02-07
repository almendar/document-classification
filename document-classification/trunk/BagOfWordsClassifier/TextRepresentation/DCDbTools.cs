namespace DocumentClassification.Representation
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    using DocumentClassification.BagOfWordsClassifier.Matrices;

    using MySql.Data.MySqlClient;

    public class DCDbTools
    {
        #region Fields

        private const string database = "dc";
        private const string pwd = "1207pegazo";
        private const string server = "localhost";
        private const string uid = "root";

        private static readonly DCDbTools instance = new DCDbTools();

        //private MySqlConnection conn;
        private int CurrentVersion;

        #endregion Fields

        #region Constructors

        static DCDbTools()
        {
        }

        private DCDbTools()
        {
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

        public MySqlConnection GetConnection()
        {
            MySqlConnection conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            return conn;
        }


        public CasesTF getCasesTF()
        {
            CasesTF result = (CasesTF)getObject("casestf");
            return result != null ? result : new CasesTF(); 
        }

        public IDFcalculation getIDFcalculation()
        {
            IDFcalculation result = (IDFcalculation)getObject("idfcalculation");
            return result != null ? result : new IDFcalculation(); 
        }
        public ProcedureMatrices getProcedureMatrices()
        {
            ProcedureMatrices result = (ProcedureMatrices)getObject("procedurematrices");
            return result;
        }
        public NextDecisionMatrices getNextStageMatrices()
        {
            NextDecisionMatrices result = (NextDecisionMatrices)getObject("nextstagematrices");
            return result;
        }
        public NextDecisionMatrices getNextPersonMatrices()
        {
            NextDecisionMatrices result = (NextDecisionMatrices)getObject("nextpersonmatrices");
            return result;
        }

        public void loadData()
        {
            lock (Data.Instance)
            {
                setCurrentVersion();
                loadDBRepresentation();
                loadAllCases();
                loadAllDecisionsStatus();
                loadAllProcedures();
                loadAllDecisionsPeople();
            }
        }

        public void loadMatricesFromDb()
        {
            setCurrentVersion();
            loadNextPersonMatrices();
            loadNextStageMatrices();
            loadProcedureMatrices();
            loadWordPicker();
        }

        public void sendData()
        {
            lock (Data.Instance)
            {
                MySqlConnection conn = GetConnection();
                createNewVersion(conn);
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
        /*
        private void connect()
        {
            if (conn == null)
                conn = new MySqlConnection();
            conn.ConnectionString = GetConnectionString();
            conn.Open();
        }
        */

        private void createNewVersion(MySqlConnection conn)
        {
            string Query = "INSERT INTO dc.versionHistory(imageDate) values" +
             "('" + System.DateTime.Now + "');" +
             "select LAST_INSERT_ID();";
            DbDataReader rdr = executeQuery(Query, conn);
            rdr.Read();
            CurrentVersion = rdr.GetInt32(0);
            rdr.Close();
        }
        /*
        private void disconnect()
        {
            conn.Close();
        }
        */

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
            rdr.Read();
            CurrentVersion = rdr.GetInt32(0);
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