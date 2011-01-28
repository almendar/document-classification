namespace DocumentClassification.BagOfWords
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DocumentClassification.BagOfWordsClassifier.Decisions;
    using DocumentClassification.BagOfWordsClassifier.Matrices;
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
        private const int NrOfBestDecisionsReturned = 4;

        static readonly BagOfWordsTextClassifier instance = new BagOfWordsTextClassifier();

        private AllCases AllCases = null;
        private AllProcedures AllProcedures = null;
        private AllDecisions AllDecisionsNextPerson = null;
        private AllDecisions AllDecisionsNextPhase = null;
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

        public void reloadData()
        {
           // ResetAllValues();
            ReadDataBase();
            FetchMeaningfulWords();
            ComputeStatisticParams();
            CreateDataMatrices();
        }



        public ClassificationResult[] NextPersonPrediction(int procedurId, int phaseId, string text)
        {
            return GenericPredictor(NextPersonMatrix, MapRowToNextPersonId,
                MapProcIdPhasIdToNextPersonRowsSet, procedurId, phaseId, text);
        }

        public ClassificationResult[] NextStagePrediciton(int procedurId, int phaseId, string text)
        {
            return GenericPredictor(NextStageMatrix, MapRowToNextStageId,
                MapProcIdPhasIdToNextStageRowsSet, procedurId, phaseId, text);


            //Orginal metod below!!!
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
        public ClassificationResult[] ProcedureRecognition(string text)
        {
            BestDecisionResult BDR = new BestDecisionResult(NrOfBestDecisionsReturned);
            String[] textTokens = TextExtraction.GetTextTokens(text);
            double[] textVector = CreateVectorFromText(textTokens);
            for (int i = 0; i < numberOfProcedures; i++)
            {
                double[] checkedVector = ProcedureMatrix[i];
                //Cosine is 1 when 0 degree angel is between vectors
                //so similarity will be 0 when vectors will have the same sense
                double similarity = (1 - VectorOperations.VectorsConsine(checkedVector, textVector));
                int bestProcedureId = MapRowToProcedureId[i];
                ClassificationResult result = new ClassificationResult(bestProcedureId, similarity);
                BDR.addResult(result);
            }
            return BDR.BestResults();
        }

        private void ComputeStatisticParams()
        {
            this.numberOfProcedures = AllProcedures.Keys.Count;
            this.numberOfMeaningfulWords = MapWordToColumn.Keys.Count; //vector length
        }



        private void CreateDataMatrices()
        {
            //ProcedureMatrixBuild();
            //NextStageMatrixBuild();
            //NextPersonMatrixBuild();
        }

        /// <summary>
        /// Takes table of strings and count frequency of words in vector
        /// that are listed in <see cref="MapWordToColumn"/>
        /// </summary>
        /// <param name="textTokens">Tokens from text</param>
        /// <returns>Vector with document TF of words</returns>
        private double[] CreateVectorFromText(string[] textTokens)
        {
            double[] vectorRep = new double[numberOfMeaningfulWords];
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
            this.wordThreshold = (int)Math.Floor((numberOfCases * MaximumFrequency));
            int vectorIndice = 0;
            MapWordToColumn = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> kvp in DBRepresentation)
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

        private ClassificationResult[] GenericPredictor(double[][] dataMatrix, int[] bestRowToDecisionIdMapping,
            Dictionary<int, Dictionary<int, List<int>>> ProcIdPhaseIdToRowsMapping, int procedurId, int phaseId, string text)
        {
            BestDecisionResult BDR = new BestDecisionResult(NrOfBestDecisionsReturned);
            String[] textTokens = TextExtraction.GetTextTokens(text);
            double[] textVector = CreateVectorFromText(textTokens);
            int nrOfDecisions = dataMatrix.Length;
            if(!ProcIdPhaseIdToRowsMapping.ContainsKey(procedurId))
            {
                throw new KeyNotFoundException("Procedure ID " + procedurId + " not found");
            }

            if (!ProcIdPhaseIdToRowsMapping[procedurId].ContainsKey(phaseId))
            {
                throw new KeyNotFoundException("Phase ID " + phaseId + " in procedure ID " + procedurId + " not found");
            }

            List<int> rowSet = ProcIdPhaseIdToRowsMapping[procedurId][phaseId];
            for (int i = 0; i < rowSet.Count; i++)
            {
                double[] checkedVector = dataMatrix[rowSet[i]];
                double similarity = (1 - VectorOperations.VectorsConsine(checkedVector, textVector));
                int bestNextDecisionId = bestRowToDecisionIdMapping[rowSet[i]];
                ClassificationResult result = new ClassificationResult(bestNextDecisionId, similarity);
                BDR.addResult(result);
            }
            return BDR.BestResults();
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
            this.AllDecisionsNextPerson = Data.Instance.AllDecisionsPeople;
        }

        #endregion Methods
    }


   
}