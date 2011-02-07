namespace DocumentClassification.BagOfWordsClassifier.Matrices
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using DocumentClassification.Representation;
    using System.Web;
    using System.Web.Caching;

    public class DataMatrices
    {
        #region Fields

        public const string PROCEDURE_MATRICES = "PROCEDURE_MATRICES";
        public const string NEXT_STAGE_MATRICES= "NEXT_STAGE_MATRICES";
        public const string NEXT_PERSON_MATRICES= "NEXT_PERSON_MATRICES";

        private static DataMatrices instance = new DataMatrices();

        private NextDecisionMatrices nextPersonMatrices;
        private NextDecisionMatrices nextStageMatrices;
        private ProcedureMatrices procedureMatrices;
        private WordPicker wordPicker;

        #endregion Fields

        #region Constructors

        private DataMatrices()
        {
        }

        #endregion Constructors

        #region Properties

        public static DataMatrices Instance
        {
            get
            {
                return instance;
            }
        }

        public NextDecisionMatrices NextPersonMatrices
        {
            get
            {
                return nextPersonMatrices;
            }
            set
            {
                nextPersonMatrices = value;
            }
        }

        public NextDecisionMatrices NextStageMatrices
        {
            get
            {
                return nextStageMatrices;
            }
            set
            {
                nextStageMatrices = value;
            }
        }

        public ProcedureMatrices ProcedureMatrices
        {
            get
            {
                return procedureMatrices;
            }
            set
            {
                procedureMatrices = value;
            }
        }

        public WordPicker WordPicker
        {
            get
            {
                return wordPicker;
            }
            set
            {
                wordPicker = value;
            }
        }

        #endregion Properties

        #region Methods

        public void loadDataMatricesFromDb()
        {
            DCDbTools.Instance.loadMatricesFromDb();
            PutMatricesToHttpContext();
        }

        private void PutMatricesToHttpContext()
        {
            HttpContext context = HttpContext.Current;
            const double EXPIRATION_TIME = 1.0;

            //context.Cache.Add(PROCEDURE_MATRICES, procedureMatrices,
                //null, DateTime.Now.AddHours(EXPIRATION_TIME), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, RefreshMatricesCallback);

            context.Cache.Add(PROCEDURE_MATRICES, procedureMatrices,
                null,Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(0.5), System.Web.Caching.CacheItemPriority.High, new CacheItemRemovedCallback(RefreshMatricesCallback));

                //null, DateTime.Now.AddMinutes(EXPIRATION_TIME), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, RefreshMatricesCallback);

            //context.Cache.Add(NEXT_STAGE_MATRICES, nextStageMatrices,
                //null, DateTime.Now.AddHours(EXPIRATION_TIME), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, RefreshMatricesCallback);
            context.Cache.Add(NEXT_STAGE_MATRICES, nextStageMatrices,
                null,Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(0.5), System.Web.Caching.CacheItemPriority.High, new CacheItemRemovedCallback(RefreshMatricesCallback));

                //null, DateTime.Now.AddMinutes(EXPIRATION_TIME), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, RefreshMatricesCallback);

            //context.Cache.Add(NEXT_PERSON_MATRICES, nextPersonMatrices,
                //null, DateTime.Now.AddHours(EXPIRATION_TIME), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, RefreshMatricesCallback);
            context.Cache.Add(NEXT_PERSON_MATRICES, nextPersonMatrices,
                null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(0.5), System.Web.Caching.CacheItemPriority.High, new CacheItemRemovedCallback(RefreshMatricesCallback));

                //null, DateTime.Now.AddMinutes(EXPIRATION_TIME), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, RefreshMatricesCallback);
        }

        public static void RefreshMatricesCallback(string key, Object value, CacheItemRemovedReason reason)
        {
            if (reason != CacheItemRemovedReason.Expired)
            {
                return;
            }

            Object obj = null;
            const double EXPIRATION_TIME = 1.0;
            HttpContext context = HttpContext.Current;
            switch (key)
            {
                case PROCEDURE_MATRICES:
                    obj = DCDbTools.Instance.getProcedureMatrices();
                    break;
                case NEXT_PERSON_MATRICES:
                    obj = DCDbTools.Instance.getNextPersonMatrices();
                    break;
                case NEXT_STAGE_MATRICES:
                    obj = DCDbTools.Instance.getNextStageMatrices();
                    break;
            }

            //context.Cache.Add(key, obj,
                //null, DateTime.Now.AddHours(EXPIRATION_TIME), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, RefreshMatricesCallback);
            context.Cache.Add(key, obj,
                null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(0.5), System.Web.Caching.CacheItemPriority.High, new CacheItemRemovedCallback(RefreshMatricesCallback));
                //null, DateTime.Now.AddMinutes(EXPIRATION_TIME), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, new CacheItemRemovedCallback(RefreshMatricesCallback));
        }


        public void rebuildDataMatrices()
        {
            wordPicker = new WordPicker(Data.Instance.AllCases, Data.Instance.DBRepresentation, 0.9);
            Dictionary<string, int> mapWordToColumn = wordPicker.FetchMeaningfulWords();
            procedureMatrices = new ProcedureMatrices(Data.Instance.AllProcedures, mapWordToColumn);
            nextPersonMatrices = new NextDecisionMatrices(Data.Instance.AllDecisionsPeople, mapWordToColumn);
            nextStageMatrices = new NextDecisionMatrices(Data.Instance.AllDecisionsStatus, mapWordToColumn);
            procedureMatrices.build();
            nextPersonMatrices.build();
            nextStageMatrices.build();
        }

        public void sendDataMatricesToDb()
        {
            DCDbTools.Instance.sendDataMatricesToDb();
        }

        #endregion Methods
    }

    [Serializable]
    public class NextDecisionMatrices
    {
        #region Fields

        private AllDecisions allDecisions;
        private double[][] dataMatrix = null;
        private Dictionary<int, Dictionary<int, List<int>>> mapProcIdPhasIdToRowsSet = null;
        private int[] mapRowToNextId = null;
        private Dictionary<string, int> mapWordToColumn;

        #endregion Fields

        #region Constructors

        public NextDecisionMatrices(AllDecisions allDecisions,
            Dictionary<String, int> mapWordToColumn)
        {
            this.allDecisions = allDecisions;
            this.mapWordToColumn = mapWordToColumn;
            int nrOfDecisions = CountPastDecisions();
            int nrOfMeaningfulWords = mapWordToColumn.Count;
            mapProcIdPhasIdToRowsSet = new Dictionary<int, Dictionary<int, List<int>>>();
            dataMatrix = new double[nrOfDecisions][];
            for (int i = 0; i < nrOfDecisions; i++)
            {
                dataMatrix[i] = new double[nrOfMeaningfulWords];
            }
            mapRowToNextId = new int[nrOfDecisions];
        }

        #endregion Constructors

        #region Properties

        public double[][] DataMatrix
        {
            get { return dataMatrix; }
            set { dataMatrix = value; }
        }

        public Dictionary<int, Dictionary<int, List<int>>> MapProcIdPhasIdToRowsSet
        {
            get { return mapProcIdPhasIdToRowsSet; }
            set { mapProcIdPhasIdToRowsSet = value; }
        }

        public int[] MapRowToNextId
        {
            get { return mapRowToNextId; }
            set { mapRowToNextId = value; }
        }

        public int NumberOfDecisions
        {
            get
            {
                return dataMatrix.Length;
            }
        }

        #endregion Properties

        #region Methods

        public void build()
        {
            int indexer = 0;
            foreach (int procId in allDecisions.Keys)
            {

                MapProcIdPhasIdToRowsSet[procId] =
                    new Dictionary<int, List<int>>();
                foreach (int phaseId in allDecisions[procId].Keys)
                {
                    MapProcIdPhasIdToRowsSet[procId][phaseId] = new List<int>();
                    foreach (int nextPhasId in allDecisions[procId][phaseId].Keys)
                    {
                        TextRepresentation textRepresentation = allDecisions[procId][phaseId][nextPhasId];
                        foreach (KeyValuePair<string, double> kvp in textRepresentation)
                        {

                            if (!mapWordToColumn.ContainsKey(kvp.Key))
                            {
                                continue;
                            }
                            else
                            {
                                String word = kvp.Key;
                                double TFIDF = kvp.Value;
                                int indice = mapWordToColumn[word];
                                DataMatrix[indexer][indice] = TFIDF;
                            }
                        }
                        MapProcIdPhasIdToRowsSet[procId][phaseId].Add(indexer);
                        MapRowToNextId[indexer] = nextPhasId;
                        indexer += 1;
                    }
                }
            }
        }

        private int CountPastDecisions()
        {
            int nrRet = 0;
            foreach (int i in allDecisions.Keys)
                foreach (int j in allDecisions[i].Keys)
                    foreach (int k in allDecisions[i][j].Keys)
                    {
                        nrRet += 1;
                    }
            return nrRet;
        }

        #endregion Methods
    }

    [Serializable]
    public class ProcedureMatrices
    {
        #region Fields

        private AllProcedures allProcedures;
        private double[][] dataMatrix;
        private int[] mapRowToId;
        private Dictionary<string, int> mapWordToColumn;

        #endregion Fields

        #region Constructors

        public ProcedureMatrices(AllProcedures allProcedures,
            Dictionary<String, int> mapWordToColumn)
        {
            this.allProcedures = allProcedures;
            this.mapWordToColumn = mapWordToColumn;

            int nrOfProcedures = allProcedures.Count;
            int nrOfMeaningfulWords = mapWordToColumn.Count;

            mapRowToId = new int[nrOfProcedures];
            dataMatrix = new double[nrOfProcedures][];
            for (int i = 0; i < nrOfProcedures; i++)
            {
                dataMatrix[i] = new double[nrOfMeaningfulWords];
            }
        }

        #endregion Constructors

        #region Properties

        public double[][] DataMatrix
        {
            get { return dataMatrix; }
            set { dataMatrix = value; }
        }

        public int[] MapRowToId
        {
            get { return mapRowToId; }
            set { mapRowToId = value; }
        }

        public int NrOfProcedures
        {
            get
            {
                return this.allProcedures.Count;
            }
        }

        public int NumberOfMeaningfullWords
        {
            get
            {
                return mapWordToColumn.Count;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Builds the <see cref="ProcedureMatrices"/> out of <see cref="AllProcedures"/> for words
        /// that are listed in <see cref="MapWordToColumn"/>
        /// </summary>
        public void build()
        {
            int procedurIndex = 0;
            foreach (KeyValuePair<int, TextRepresentation> kvp in allProcedures)
            {
                int procedurId = kvp.Key;
                TextRepresentation currentProcedure = kvp.Value;
                MapRowToId[procedurIndex] = procedurId;
                foreach (KeyValuePair<string, double> textStatistic in currentProcedure)
                {
                    string word = textStatistic.Key;
                    double TFIDF = textStatistic.Value;

                    //Means word is not meaningful, and is not take into consideration.
                    if (!mapWordToColumn.ContainsKey(word))
                    {
                        continue;
                    }
                    else
                    {
                        int wordIndex = mapWordToColumn[word]; //Find what is this word place in vector
                        DataMatrix[procedurIndex][wordIndex] = TFIDF;
                    }
                }
                ++procedurIndex; //increment procedure indice
            }
        }

        #endregion Methods
    }

    [Serializable]
    public class WordPicker
    {
        #region Fields

        private AllCases allCases;
        private DBRepresentation dbRepresentation;
        private double maximumFrequency;

        #endregion Fields

        #region Constructors

        public WordPicker(AllCases allCases, DBRepresentation dbRepresentation, double maxFrequency)
        {
            this.allCases = allCases;
            this.maximumFrequency = maxFrequency;
            this.dbRepresentation = dbRepresentation;
        }

        #endregion Constructors

        #region Methods

        public Dictionary<string, int> FetchMeaningfulWords()
        {
            int numberOfCases = allCases.Count;
            int wordThreshold = (int)Math.Floor((numberOfCases * maximumFrequency));
            int vectorIndice = 0;
            Dictionary<string, int> mapWordToColumn = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> kvp in dbRepresentation)
            {
                int documentFrequency = kvp.Value;
                string word = kvp.Key;
                if (documentFrequency > wordThreshold)
                {
                    continue;
                }
                else
                {
                    mapWordToColumn[word] = vectorIndice;
                    vectorIndice += 1;
                }
            }
            return mapWordToColumn;
        }

        #endregion Methods
    }
}