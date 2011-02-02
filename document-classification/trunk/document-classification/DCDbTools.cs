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

        private MySqlConnection conn;
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

        public CasesTF getCasesTF()
        {
            return (CasesTF)getObject("casestf");
        }

        public IDFcalculation getIDFcalculation()
        {
            return (IDFcalculation)getObject("idfcalculation");
        }

        public void loadData()
        {
            lock (Data.Instance)
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
        }

        public void loadMatricesFromDb()
        {
            connect();
            setCurrentVersion();
            getNextPersonMatrices();
            getNextStageMatrices();
            getProcedureMatrices();
            getWordPicker();
            disconnect();
        }

        public void sendData()
        {
            lock (Data.Instance)
            {
                connect();
                createNewVersion();
                startTransaction();
                sendAllCases();
                sendAllDecisionsPeople();
                sendAllDecisionsStatus();
                sendAllProcedures();
                sendDBRepresentation();
                sendCasesTF();
                sendIDFcalculation();
                commit();
                disconnect();
            }
        }

        public void sendDataMatricesToDb()
        {
            connect();
            startTransaction();
            sendNextPersonMatrices();
            sendNextStageMatrices();
            sendProceduresMatrices();
            sendWordPicker();
            commit();
            disconnect();
        }

        private void commit()
        {
            string Query = "COMMIT;";
            executeNonQuery(Query);
        }

        private void connect()
        {
            if (conn == null)
                conn = new MySqlConnection();
            conn.ConnectionString = getConnectionString();
            conn.Open();
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

        private void disconnect()
        {
            conn.Close();
        }

        private void executeNonQuery(String nonQuery)
        {
            DbCommand cmd = new MySqlCommand(nonQuery, conn);
            cmd.ExecuteNonQuery();
        }

        private DbDataReader executeQuery(String query)
        {
            DbCommand cmd = new MySqlCommand(query, conn);
            return (cmd.ExecuteReader());
        }

        private void getAllCases()
        {
            Data.Instance.AllCases = (AllCases)getObject("allcases");
        }

        private void getAllDecisionsPeople()
        {
            Data.Instance.AllDecisionsPeople = (AllDecisions)getObject("alldecisionspeople");
        }

        private void getAllDecisionsStatus()
        {
            Data.Instance.AllDecisionsStatus = (AllDecisions)getObject("alldecisionsstatus");
        }

        private void getAllProcedures()
        {
            Data.Instance.AllProcedures = (AllProcedures)getObject("allprocedures");
        }

        private string getConnectionString()
        {
            return "Server=" + server + ";Database=" + database + ";Uid=" + uid + ";Pwd=" + pwd + ";";
        }

        private void getDBRepresentation()
        {
            Data.Instance.DBRepresentation = (DBRepresentation)getObject("dbrepresentation");
        }

        private void getNextPersonMatrices()
        {
            DataMatrices.Instance.NextPersonMatrices = (NextDecisionMatrices)getObject("nextpersonmatrices");
        }

        private void getNextStageMatrices()
        {
            DataMatrices.Instance.NextStageMatrices = (NextDecisionMatrices)getObject("nextstagematrices");
        }

        private object getObject(string tableName)
        {
            string Query = "SELECT * FROM dc." + tableName +
                           "where versionhistory_idversionHistory = " + CurrentVersion + ";";
            DbDataReader rdr = executeQuery(Query);
            object result = null;
            if (rdr.Read())
            {
                BinaryFormatter bf = new BinaryFormatter();
                System.IO.MemoryStream mem =
                new System.IO.MemoryStream(Convert.FromBase64String(rdr.GetString(1)));
                result = bf.Deserialize(mem);
            }
            rdr.Close();
            return result;
        }

        private void getProcedureMatrices()
        {
            DataMatrices.Instance.ProcedureMatrices = (ProcedureMatrices)getObject("procedurematrices");
        }

        private void getWordPicker()
        {
            DataMatrices.Instance.WordPicker = (WordPicker)getObject("wordpicker");
        }

        private void sendAllCases()
        {
            sendObject("allcases",  Data.Instance.AllCases);
        }

        private void sendAllDecisionsPeople()
        {
            sendObject("alldecisionspeople", Data.Instance.AllDecisionsPeople);
        }

        private void sendAllDecisionsStatus()
        {
            sendObject("alldecisionsstatus",Data.Instance.AllDecisionsStatus);
        }

        private void sendAllProcedures()
        {
            sendObject("allprocedures", Data.Instance.AllProcedures);
        }

        private void sendCasesTF()
        {
            sendObject("casestf", IDFcalculationHelper.Instance.CasesTF);
        }

        private void sendDBRepresentation()
        {
            sendObject("dbrepresentation", Data.Instance.DBRepresentation);
        }

        private void sendIDFcalculation()
        {
            sendObject("idfcalculation", IDFcalculationHelper.Instance.IDFcalculation);
        }

        private void sendNextPersonMatrices()
        {
            sendObject("nextpersonmatrices", DataMatrices.Instance.NextPersonMatrices);
        }

        private void sendNextStageMatrices()
        {
            sendObject("nextstagematrices", DataMatrices.Instance.NextStageMatrices);
        }

        private void sendObject(string tableName, object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            bf.Serialize(mem, obj);
            String str = Convert.ToBase64String(mem.ToArray());

            string Query = "INSERT INTO dc." + tableName +
                "(base64BinaryData,versionhistory_idversionHistory) values" +
             "('" + str + "'," + CurrentVersion + ");";

            executeNonQuery(Query);
        }

        private void sendProceduresMatrices()
        {
            sendObject("proceduresmatrices", DataMatrices.Instance.ProcedureMatrices);
        }

        private void sendWordPicker()
        {
            sendObject("wordpicker", DataMatrices.Instance.WordPicker);
        }

        private void setCurrentVersion()
        {
            string Query = "SELECT MAX(idversionHistory) FROM versionhistory;";
            DbDataReader rdr = executeQuery(Query);
            rdr.Read();
            CurrentVersion = rdr.GetInt32(0);
            rdr.Close();
        }

        private void startTransaction()
        {
            string Query = "START TRANSACTION;";
            executeNonQuery(Query);
        }

        #endregion Methods
    }
}