namespace DocumentClassification.BagOfWords
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;
    using AMODDecisionSupportClasses;
    using DocumentClassification.BagOfWordsClassifier.Decisions;
    using DocumentClassification.BagOfWordsClassifier.Matrices;
    using DocumentClassification.Representation;
    using DocumentClassification.DCUpdate;

    /// <summary>
    /// Singleton instance of classificator based on bag-of-words paradigm
    /// </summary>
    public class BagOfWordsTextClassifier
    {
        #region Fields

        static readonly BagOfWordsTextClassifier instance = new BagOfWordsTextClassifier();
        private Dictionary<string, int> mapWordToColumn = null;
        private int nrOfBestDecisionsReturned = 4;

       #endregion Fields

        #region Constructors

        static BagOfWordsTextClassifier()
        {
        }

        private BagOfWordsTextClassifier()
        {
            DataMatrices.Instance.loadDataMatricesFromDb();
            MapWordToColumn = DataMatrices.Instance.WordPicker.FetchMeaningfulWords();
        }

        #endregion Constructors

        #region Properties

		/// <summary>
		/// Singleton getter to this classifier 
		/// </summary>
        public static BagOfWordsTextClassifier Instance
        {
            get
            {
                return instance;
            }
        }

		/// <summary>
		/// Maps words do positioin in vector that is used when calulation cosine
		/// Because vector is of doubles this is needed. 
		/// </summary>
        public Dictionary<string, int> MapWordToColumn
        {
            get { return mapWordToColumn; }
            set { mapWordToColumn = value; }
        }

		/// <summary>
		/// How many decisions classifier will try to return
		/// </summary>
        public int NrOfBestDecisionsReturned
        {
            get { return nrOfBestDecisionsReturned; }
            set { nrOfBestDecisionsReturned = value; }
        }

        #endregion Properties

        #region Methods

		/// <summary>
		/// Recognise next stage for a case.
		/// For more info see generic method it is based on:
		/// <see cref="NextDecisionPrediciton"/>
		/// </summary>
        public List<AMODPrediction> NextPersonPrediction(int caseId, int procId, int personId)
        {
            NextDecisionMatrices dataMatrices = (NextDecisionMatrices) HttpContext.Current.Cache[DataMatrices.NEXT_PERSON_MATRICES];//DataMatrices.Instance.NextPersonMatrices;
            return NextDecisionPrediciton(dataMatrices,caseId, procId, personId, ClassificatorType.Users);
        }

		/// <summary>
		/// Recognise next stage for a case.
		/// For more info see generic method it is based on:
		/// <see cref="NextDecisionPrediciton"/>
		/// </summary>
        public List<AMODPrediction> NextStagePrediciton(int caseId, int procId, int phaseId)
        {
            NextDecisionMatrices dataMatrices = (NextDecisionMatrices) HttpContext.Current.Cache[DataMatrices.NEXT_STAGE_MATRICES];//DataMatrices.Instance.NextStageMatrices;
            return NextDecisionPrediciton(dataMatrices,caseId, procId, phaseId, ClassificatorType.Stages);
        }

        /// <summary>
        /// Recognises the procedure based on text of case.
        /// Case id taken from the database by its id.
        /// </summary>
        /// Case id to classify
        /// </param>
        /// <returns>
        /// List of best result of classification
        /// </returns>
        public List<AMODPrediction> ProcedureRecognition(int caseId)
        {
            ProcedureMatrices procedureMatrix = (ProcedureMatrices) HttpContext.Current.Cache[DataMatrices.PROCEDURE_MATRICES];//DataMatrices.Instance.ProcedureMatrices;
            double[] textVector = CreateVectorFromText(AmodDBTools.Instance.getData(caseId));
            BestDecisionResult BDR = new BestDecisionResult(nrOfBestDecisionsReturned, ClassificatorType.Procedures);
            
            for (int i = 0; i < procedureMatrix.NrOfProcedures; i++)
            {
                double[] checkedVector = procedureMatrix.DataMatrix[i];
                //Cosine is 1 when 0 degree angel is between vectors
                //so similarity will be 0 when vectors will have the same sense
                double similarity = (1 - VectorOperations.VectorsConsine(checkedVector, textVector));
                int bestProcedureId = procedureMatrix.MapRowToId[i];
                ClassificationResult result = new ClassificationResult(bestProcedureId, similarity);

                BDR.addResult(result);
            }
            return BDR.BestResults();
        }

		/// <summary>
		/// Takes a map of strings with its TF and creates a vector of it.
		/// </summary>
		/// <param name="text">
		/// Map of words with TFs
		/// </param>
		/// <returns>
		/// Table of doubles for quick cosine calculation with other text vector.
		/// </returns>
		private double[] CreateVectorFromText(Dictionary<string, int> text)
        {
            double[] textVector = TextExtraction.CreateVectorFromText(text, mapWordToColumn);
            return textVector;
        }

		/// <summary>
		/// Generic prediction class for next person or stage 
		/// </summary>
		/// <param name="nextDecisionsMatrices">
		/// A <see cref="NextDecisionMatrices"/>
		/// </param>
		/// <param name="caseId">
		/// Id of the case to recognize
		/// </param>
		/// <param name="procedurId">
		/// Id of the procedure id.
		/// </param>
		/// <param name="phaseId">
		/// Phase id
		/// </param>
		/// <param name="type">
		/// A <see cref="ClassificatorType"/>
		/// </param>
		/// <returns>
		/// List of best predicted results
		/// </returns>
        private List<AMODPrediction> NextDecisionPrediciton(NextDecisionMatrices nextDecisionsMatrices,int caseId, int procedurId, int phaseId, ClassificatorType type)
        {
            NextDecisionMatrices decisionMatrices = nextDecisionsMatrices;
            BestDecisionResult BDR = new BestDecisionResult(nrOfBestDecisionsReturned, type);
            Dictionary<string, int> text = AmodDBTools.Instance.getData(caseId);
            double[] textVector = CreateVectorFromText(text);
            int nrOfDecisions = decisionMatrices.NumberOfDecisions;
            if (!decisionMatrices.MapProcIdPhasIdToRowsSet.ContainsKey(procedurId))
            {
                throw new KeyNotFoundException("Procedure ID " + procedurId + " not found");
            }

            if (!decisionMatrices.MapProcIdPhasIdToRowsSet[procedurId].ContainsKey(phaseId))
            {
                throw new KeyNotFoundException("Phase ID " + phaseId + " in procedure ID " + procedurId + " not found");
            }

            List<int> rowSet = decisionMatrices.MapProcIdPhasIdToRowsSet[procedurId][phaseId];
            for (int i = 0; i < rowSet.Count; i++)
            {
                double[] checkedVector = decisionMatrices.DataMatrix[rowSet[i]];
                double similarity = (1 - VectorOperations.VectorsConsine(checkedVector, textVector));
                int bestNextDecisionId = decisionMatrices.MapRowToNextId[rowSet[i]];
                ClassificationResult result = new ClassificationResult(bestNextDecisionId, similarity);
                BDR.addResult(result);
            }
            return BDR.BestResults();
        }

        #endregion Methods
    }
}