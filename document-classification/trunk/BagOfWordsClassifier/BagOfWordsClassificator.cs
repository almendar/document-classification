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
        static readonly BagOfWordsTextClassifier instance = new BagOfWordsTextClassifier();
        private int nrOfBestDecisionsReturned = 4;
        private Dictionary<string, int> mapWordToColumn = null;    
        #endregion Fields

        #region Constructors
        static BagOfWordsTextClassifier()
        {

        }
        private BagOfWordsTextClassifier()
        {
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
        public int NrOfBestDecisionsReturned
        {
            get { return nrOfBestDecisionsReturned; }
            set { nrOfBestDecisionsReturned = value; }
        }
        public Dictionary<string, int> MapWordToColumn
        {
            get { return mapWordToColumn; }
            set { mapWordToColumn = value; }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Based on text tries to find right procedures for given text.
        /// Now returns only the best procedure ID.
        /// </summary>
        /// <param name="text">Text of document</param>
        /// <returns>Procedures IDs table</returns>
        public ClassificationResult[] ProcedureRecognition(List<int> attachmentsIdList)
        {
            //jakims cudem mam to miec z cache przeglądarki
            ProcedureMatrices procedureMatrix = new ProcedureMatrices(null, null);
            //

            //
            //Z attachmentsIdList wyciągną tekst, który mam sprawnie poszatkować.
            String text = null;



            BestDecisionResult BDR = new BestDecisionResult(nrOfBestDecisionsReturned);
            double[] textVector = CreateVectorFromText(text);
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

        
        public ClassificationResult[] NextPersonPrediction(int procId, int phaseId, string p)
        {
            NextDecisionMatrices dataMatrices = null; //Z http contexu trzeba to wyciągnąć
            return NextDecisionPrediciton(dataMatrices, procId, phaseId, p);
        }
        public ClassificationResult[] NextStagePrediciton(int procId, int phaseId, string p)
        {
            NextDecisionMatrices dataMatrices = null; //Z http contexu trzeba to wyciągnąć
            return NextDecisionPrediciton(dataMatrices, procId, phaseId, p);
        }
        private double[] CreateVectorFromText(String text)
        {
            String[] textTokens = TextExtraction.GetTextTokens(text);
            double[] textVector = TextExtraction.CreateVectorFromText(textTokens, mapWordToColumn);
            return textVector;
        }
        private ClassificationResult[] NextDecisionPrediciton(NextDecisionMatrices nextDecisionsMatrices, int procedurId, int phaseId, string text)
        {

            NextDecisionMatrices decisionMatrices = nextDecisionsMatrices;
            BestDecisionResult BDR = new BestDecisionResult(nrOfBestDecisionsReturned);
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