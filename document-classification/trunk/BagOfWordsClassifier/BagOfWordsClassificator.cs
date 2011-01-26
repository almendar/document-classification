namespace DocumentClassification.BagOfWords
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using DocumentClassification.Representation;

    /// <summary>
    /// Singleton instance of classificator based on bag-of-words paradigm
    /// </summary>
    public class BagOfWordsTextClassifier
    {
        #region Fields

        /// <summary>
        /// Maximum percentage of cases in which word can appear to be take into consideration
        /// </summary>
        private const double MaximumFrequency = 0.9;

        static readonly BagOfWordsTextClassifier instance = new BagOfWordsTextClassifier();

        private AllCases AllCases = null;
        private AllDecisionsNextPerson AllDecisionsNextPerson = null;
        private AllDecisionsNextStage AllDecisionsNextPhase = null;
        private AllProcedures AllProcedures = null;
        private DBRepresentation DBRepresentation = null;
        private Dictionary<int, Dictionary<int, List<int>>> MapProcIdPhasIdToNextPersonRowsSet = null;
        private Dictionary<int, Dictionary<int, List<int>>> MapProcIdPhasIdToNextStageRowsSet = null;
        private int[] MapRowToNextPersonId = null;
        private int[] MapRowToNextStageId = null;

        /// <summary>
        /// Table maps row of the <see cref="ProcedureMatrix"/> to correspondent procedure id 
        /// in the database.
        /// </summary>
        private int[] MapRowToProcedureId = null;

        /// <summary>
        /// Maps word to column number.
        /// Take into consideration that not evey word is present because of the threashold.
        /// Only words that appeare in less than <see cref="MaximumFrequency"/> percentage of documents
        /// will be taken into consideration
        /// </summary>
        private Dictionary<String, int> MapWordToColumn = null;

        //**********************************************************************//
        private double[][] NextPersonMatrix = null;

        //TODO
        //**********************************************************************//
        private double[][] NextStageMatrix = null;
        private int numberOfCases;

        /// <summary>
        /// How many words are being used in computation
        /// </summary>
        private int numberOfMeaningfulWords;

        /// <summary>
        /// Number of procedures read from database
        /// </summary>
        private int numberOfProcedures;

        //**********************************************************************//
        /// <summary>
        /// Rows of this matrix represents procedures, columns are words <see cref="MapWordToColumn"/>.
        /// Values of this matrix are TFIDF of words in procedures.
        /// </summary>
        private double[][] ProcedureMatrix = null;

        /// <summary>
        /// Threashold above which words are not taken into consideration.
        /// When wors appears in more documents that trheashold stand for
        /// word is being omited
        /// </summary>
        private int wordThreshold = int.MaxValue;

        #endregion Fields

        #region Constructors

        static BagOfWordsTextClassifier()
        {
        }

        private BagOfWordsTextClassifier()
        {
            ReadDataBase();
            FetchMeaningfulWords();
            ComputeStatisticParams();
            CreateDataMatrices();
        }

        #endregion Constructors

        #region Properties

        public static BagOfWordsTextClassifier Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion Properties

        #region Methods

        public int[] NextPersonPrediction(int procedurId, int phaseId, string text)
        {
            return GenericPredictor(NextPersonMatrix, MapRowToNextPersonId,
                MapProcIdPhasIdToNextPersonRowsSet, procedurId, phaseId, text);
        }

        public int[] NextPhasePrediciton(int procedurId, int phaseId, string text)
        {
            return GenericPredictor(NextStageMatrix, MapRowToNextStageId,
                MapProcIdPhasIdToNextStageRowsSet, procedurId, phaseId, text);

            //String[] textTokens = TextExtraction.GetTextTokens(text);
            //double[] textVector = CreateVectorFromText(textTokens);
            //int nrOfDecisions = PhaseMatrix.Length;
            //int bestDecisionIndice = int.MinValue;
            //double bestSimilarity = double.PositiveInfinity;
            //List<int> rowSet = MapProcIdPhasIdToRowsSet[procedurId][phaseId];
            //for (int i = 0; i < rowSet.Count; i++)
            //{
            //    double[] checkedVector = PhaseMatrix[rowSet[i]];
            //    double similarity = (1-VectorOperations.VectorsConsine(checkedVector,textVector));
            //    if (similarity < bestSimilarity)
            //    {
            //        bestSimilarity = similarity;
            //        bestDecisionIndice = rowSet[i];
            //    }
            //}
            //int bestNextPhaseId = MapRowToNextPhaseId[bestDecisionIndice];
            //int[] ret = new int[] { bestNextPhaseId };
            //return ret;
        }

        /// <summary>
        /// Based on text tries to find right procedures for given text.
        /// Now returns only the best procedure ID.
        /// </summary>
        /// <param name="text">Text of document</param>
        /// <returns>Procedures IDs table</returns>
        public int[] ProcedureRecognition(string text)
        {
            String [] textTokens = TextExtraction.GetTextTokens(text);
            double[] textVector = CreateVectorFromText(textTokens);
            int bestProcedureIndice = int.MinValue;
            double bestSimilarity = double.PositiveInfinity;
            for (int i = 0; i < numberOfProcedures; i++)
            {
                double [] checkedVector = ProcedureMatrix[i];
                //Cosine is 1 when 0 degree angel is between vectors
                //so similarity will be 0 when vectors will have the same sense
                double similarity = (1 - VectorOperations.VectorsConsine(checkedVector, textVector));
                if (similarity < bestSimilarity)
                {
                    bestSimilarity = similarity;
                    bestProcedureIndice = i;
                }
            }
            int bestProcedureId = MapRowToProcedureId[bestProcedureIndice];
            int[] ret = new int[] { bestProcedureId };
            return ret;
        }

        private void ComputeStatisticParams()
        {
            this.numberOfProcedures = AllProcedures.Keys.Count;
            this.numberOfMeaningfulWords = MapWordToColumn.Keys.Count; //vector length
        }

        private int CountPastDecisions<T>(ref Dictionary<int, Dictionary<int, Dictionary<int, T>>> pastData)
        {
            int nrRet = 0;
            foreach (int i in pastData.Keys)
                foreach (int j in pastData[i].Keys)
                    foreach (int k in pastData[i][j].Keys)
                    {
                        nrRet += 1;
                    }
            return nrRet;
        }

        private int CountPastDecisionsNextPerson(AllDecisionsNextPerson AllDecisionsNextPerson)
        {
            Dictionary<int, Dictionary<int, Dictionary<int, DecisionRepresentationNextPerson>>> data = AllDecisionsNextPerson;
            return this.CountPastDecisions(ref data);
        }

        private int CountPastDecisionsNextStage(AllDecisionsNextStage AllDecisionsPhase)
        {
            Dictionary<int, Dictionary<int, Dictionary<int, DecisionRepresentationNextStage>>> data = AllDecisionsPhase;
            return this.CountPastDecisions(ref data);
        }

        private void CreateDataMatrices()
        {
            ProcedureMatrixBuild();
            NextStageMatrixBuild();
            NextPersonMatrixBuild();
        }

        /// <summary>
        /// Takes table of strings and count frequency of words in vector
        /// that are listed in <see cref="MapWordToColumn"/>
        /// </summary>
        /// <param name="textTokens">Tokens from text</param>
        /// <returns>Vector with document TF of words</returns>
        private double[] CreateVectorFromText(string[] textTokens)
        {
            double [] vectorRep = new double[numberOfMeaningfulWords];
            foreach (String word in textTokens)
            {
                if (!MapWordToColumn.ContainsKey(word))
                    continue;

                int indice = MapWordToColumn[word];
                vectorRep[indice] += 1.0d;
            }
            return vectorRep;
        }

        /// <summary>
        /// Finds all words that frequency in <see cref="AllCases"/> is less than threashold
        /// </summary>
        private void FetchMeaningfulWords()
        {
            this.numberOfCases = AllCases.Count;
            this.wordThreshold = (int) Math.Floor((numberOfCases * MaximumFrequency));
            int vectorIndice = 0;
            MapWordToColumn = new Dictionary<string, int>();
            foreach(KeyValuePair<string, int> kvp in DBRepresentation)
            {
                int documentFrequency = kvp.Value;
                string word = kvp.Key;
                if (documentFrequency > wordThreshold)
                {
                    continue;
                }
                else
                {
                    MapWordToColumn[word] = vectorIndice;
                    vectorIndice += 1;
                }
            }
        }

        private int[] GenericPredictor(double[][] dataMatrix, int[] bestRowToDecisionIdMapping,
            Dictionary<int, Dictionary<int, List<int>>> ProcIdPhaseIdToRowsMapping, int procedurId, int phaseId, string text)
        {
            String[] textTokens = TextExtraction.GetTextTokens(text);
            double[] textVector = CreateVectorFromText(textTokens);
            int nrOfDecisions = dataMatrix.Length;
            int bestDecisionIndice = int.MinValue;
            double bestSimilarity = double.PositiveInfinity;
            List<int> rowSet = ProcIdPhaseIdToRowsMapping[procedurId][phaseId];
            for (int i = 0; i < rowSet.Count; i++)
            {
                double[] checkedVector = dataMatrix[rowSet[i]];
                double similarity = (1 - VectorOperations.VectorsConsine(checkedVector, textVector));
                if (similarity < bestSimilarity)
                {
                    bestSimilarity = similarity;
                    bestDecisionIndice = rowSet[i];
                }
            }
            int bestNextDecisionId = bestRowToDecisionIdMapping[bestDecisionIndice];
            int[] ret = new int[] { bestNextDecisionId };
            return ret;
        }

        private void NextPersonMatrixBuild()
        {
            MapProcIdPhasIdToNextPersonRowsSet = new Dictionary<int, Dictionary<int, List<int>>>();
            int nrOfDecisions = CountPastDecisionsNextPerson(AllDecisionsNextPerson);
            NextPersonMatrix = new double[nrOfDecisions][];
            for (int i = 0; i < nrOfDecisions; i++)
            {
                NextPersonMatrix[i] = new double[numberOfMeaningfulWords];
            }

            MapRowToNextPersonId = new int[nrOfDecisions];
            int indexer = 0;
            foreach (int procId in AllDecisionsNextPerson.Keys)
            {
                MapProcIdPhasIdToNextStageRowsSet[procId] = new Dictionary<int, List<int>>();
                foreach (int phaseId in AllDecisionsNextPerson[procId].Keys)
                {
                    MapProcIdPhasIdToNextStageRowsSet[procId][phaseId] = new List<int>();
                    foreach (int nextPhasId in AllDecisionsNextPerson[procId][phaseId].Keys)
                    {
                        DecisionRepresentationNextPerson textRepresentation = AllDecisionsNextPerson[procId][phaseId][nextPhasId];
                        foreach (KeyValuePair<string, double> kvp in textRepresentation)
                        {

                            if (!MapWordToColumn.ContainsKey(kvp.Key))
                            {
                                continue;
                            }
                            else
                            {
                                String word = kvp.Key;
                                double TFIDF = kvp.Value;
                                int indice = MapWordToColumn[word];
                                NextPersonMatrix[indexer][indice] = TFIDF;
                            }
                        }

                        MapProcIdPhasIdToNextPersonRowsSet[procId][phaseId].Add(indexer);
                        MapRowToNextPersonId[indexer] = nextPhasId;
                        indexer += 1;
                    }
                }
            }
        }

        private void NextStageMatrixBuild()
        {
            MapProcIdPhasIdToNextStageRowsSet = new Dictionary<int, Dictionary<int, List<int>>>();
            int nrOfDecisions = CountPastDecisionsNextStage(AllDecisionsNextPhase);
            NextStageMatrix = new double[nrOfDecisions][];
            for(int i=0; i < nrOfDecisions; i++)
            {
                NextStageMatrix[i] = new double[numberOfMeaningfulWords];
            }

            MapRowToNextStageId = new int[nrOfDecisions];
            int indexer = 0;
            foreach (int procId in AllDecisionsNextPhase.Keys)
            {
                MapProcIdPhasIdToNextStageRowsSet[procId] = new Dictionary<int, List<int>>();
                foreach (int phaseId in AllDecisionsNextPhase[procId].Keys)
                {
                    MapProcIdPhasIdToNextStageRowsSet[procId][phaseId] = new List<int>();
                    foreach (int nextPhasId in AllDecisionsNextPhase[procId][phaseId].Keys)
                    {
                        DecisionRepresentationNextStage textRepresentation = AllDecisionsNextPhase[procId][phaseId][nextPhasId];
                        foreach (KeyValuePair<string, double> kvp in textRepresentation)
                        {

                            if (!MapWordToColumn.ContainsKey(kvp.Key))
                            {
                                continue;
                            }
                            else
                            {
                                String word = kvp.Key;
                                double TFIDF = kvp.Value;
                                int indice = MapWordToColumn[word];
                                NextStageMatrix[indexer][indice]= TFIDF;
                            }
                        }

                        MapProcIdPhasIdToNextStageRowsSet[procId][phaseId].Add(indexer);
                        MapRowToNextStageId[indexer] = nextPhasId;
                        indexer += 1;
                    }
                }
            }
        }

        /// <summary>
        /// Builds the <see cref="ProcedureMatrix"/> out of <see cref="AllProcedures"/> for words
        /// that are listed in <see cref="MapWordToColumn"/>
        /// </summary>
        private void ProcedureMatrixBuild()
        {
            //int nrOfProcedures = ProceduresSet.Keys.Count;
            ProcedureMatrix = new double[numberOfProcedures][];
            MapRowToProcedureId = new int[numberOfProcedures];
            for (int h = 0; h < numberOfProcedures; h++)
            {
                ProcedureMatrix[h] = new double[numberOfMeaningfulWords];
            }
            int procedurIndex = 0;
            foreach (KeyValuePair<int, Procedure> kvp in AllProcedures)
            {
                int procedurId = kvp.Key;
                Procedure currentProcedure = kvp.Value;
                MapRowToProcedureId[procedurIndex] = procedurId;
                foreach (KeyValuePair<string, double> textStatistic in currentProcedure)
                {
                    string word = textStatistic.Key;
                    double TFIDF = textStatistic.Value;

                    //Means word is not meaningful, and is not take into consideration.
                    if (!MapWordToColumn.ContainsKey(word))
                    {
                        continue;
                    }
                    else
                    {
                        int wordIndex = MapWordToColumn[word]; //Find what is this word place in vector
                        ProcedureMatrix[procedurIndex][wordIndex] = TFIDF;
                    }
                }
                ++procedurIndex; //increment procedure indice
            }
        }

        /// <summary>
        /// Reads serialized objects from database that will be base for building matrices
        /// </summary>
        private void ReadDataBase()
        {
            DCDbTools.Instance.loadData();
            this.DBRepresentation = Data.Instance.DBRepresentation;
            this.AllCases = Data.Instance.AllCases;
            this.AllProcedures = Data.Instance.AllProcedures;
            this.AllDecisionsNextPhase = Data.Instance.AllDecisionsStatus;
        }

        #endregion Methods
    }

    public class ClassificationResult : IComparable<ClassificationResult>
    {
        #region Fields

        private readonly int id;
        private readonly double similarity;

        #endregion Fields

        #region Constructors

        public ClassificationResult(int id, double similarity)
        {
            this.id = id;
               this.similarity = similarity;
        }

        #endregion Constructors

        #region Properties

        public int Id
        {
            get
               {
               return id;
               }
        }

        public double Similarity
        {
            get
               {
               return similarity;
               }
        }

        #endregion Properties

        #region Methods

        public int CompareTo(ClassificationResult other)
        {
            return this.Similarity.CompareTo(other.Similarity);
        }

        #endregion Methods
    }
}