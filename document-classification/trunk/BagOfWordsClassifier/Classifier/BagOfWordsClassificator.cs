namespace DocumentClassification.BagOfWords
{
    using System;
    using System.Collections.Generic;
    using System.Text;

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

        public static BagOfWordsTextClassifier Instance
        {
            get
            {
                return instance;
            }
        }

        public Dictionary<string, int> MapWordToColumn
        {
            get { return mapWordToColumn; }
            set { mapWordToColumn = value; }
        }

        public int NrOfBestDecisionsReturned
        {
            get { return nrOfBestDecisionsReturned; }
            set { nrOfBestDecisionsReturned = value; }
        }

        #endregion Properties

        #region Methods

        public List<AMODPrediction> NextPersonPrediction(int caseId, int procId, int personId)
        {
            //Z http contexu trzeba to wyciągnąć
            NextDecisionMatrices dataMatrices = DataMatrices.Instance.NextPersonMatrices;
            return NextDecisionPrediciton(dataMatrices,caseId, procId, personId, ClassificatorType.Users);
        }

        public List<AMODPrediction> NextStagePrediciton(int caseId, int procId, int phaseId)
        {
            //Z http contexu trzeba to wyciągnąć
            NextDecisionMatrices dataMatrices = DataMatrices.Instance.NextStageMatrices;
            return NextDecisionPrediciton(dataMatrices,caseId, procId, phaseId, ClassificatorType.Stages);
        }

        /// <summary>
        /// Based on text tries to find right procedures for given text.
        /// Now returns only the best procedure ID.
        /// </summary>
        /// <param name="text">Text of document</param>
        /// <returns>Procedures IDs table</returns>
        public List<AMODPrediction> ProcedureRecognition(int caseId)
        {
            //jakims cudem mam to miec z cache przeglądarki
            ProcedureMatrices procedureMatrix = DataMatrices.Instance.ProcedureMatrices;
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

        private double[] CreateVectorFromText(Dictionary<string, int> text)
        {
            double[] textVector = TextExtraction.CreateVectorFromText(text, mapWordToColumn);
            return textVector;
        }

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