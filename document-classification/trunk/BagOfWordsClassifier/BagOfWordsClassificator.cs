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
        private double maxFrequency = 0.9;
        private Dictionary<string, int> MapWordToColumn;
        #endregion Fields

        #region Constructors

        static BagOfWordsTextClassifier()
        {
            
        }

        private BagOfWordsTextClassifier()
        {
        }

        private void ReadDatabaseObjects()
        {
            throw new NotImplementedException();
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


        public ClassificationResult[] NextStagePrediciton(int procedurId, int phaseId, string text)
        {

            NextDecisionMatrices nextDecisionMatrices = null;
            
            BestDecisionResult BDR = new BestDecisionResult(nrOfBestDecisionsReturned);
            String[] textTokens = TextExtraction.GetTextTokens(text);
            double[] textVector = TextExtraction.CreateVectorFromText(textTokens, MapWordToColumn);
            int nrOfDecisions = nextDecisionMatrices.NumberOfDecisions;
            if (!nextDecisionMatrices.MapProcIdPhasIdToRowsSet.ContainsKey(procedurId))
            {
                throw new KeyNotFoundException("Procedure ID " + procedurId + " not found");
            }

            if (!nextDecisionMatrices.MapProcIdPhasIdToRowsSet[procedurId].ContainsKey(phaseId))
            {
                throw new KeyNotFoundException("Phase ID " + phaseId + " in procedure ID " + procedurId + " not found");
            }

            List<int> rowSet = nextDecisionMatrices.MapProcIdPhasIdToRowsSet[procedurId][phaseId];
            for (int i = 0; i < rowSet.Count; i++)
            {
                double[] checkedVector = nextDecisionMatrices.DataMatrix[rowSet[i]];
                double similarity = (1 - VectorOperations.VectorsConsine(checkedVector, textVector));
                int bestNextDecisionId = nextDecisionMatrices.MapRowToNextId[rowSet[i]];
                ClassificationResult result = new ClassificationResult(bestNextDecisionId, similarity);
                BDR.addResult(result);
            }
            return BDR.BestResults();

           
        }

        /// <summary>
        /// Based on text tries to find right procedures for given text.
        /// Now returns only the best procedure ID.
        /// </summary>
        /// <param name="text">Text of document</param>
        /// <returns>Procedures IDs table</returns>
        public ClassificationResult[] ProcedureRecognition(string text)
        {
            //jakims cudem mam to miec z cache przeglądarki
            ProcedureMatrices procedureMatrix = new ProcedureMatrices(null,null);
            //

            BestDecisionResult BDR = new BestDecisionResult(nrOfBestDecisionsReturned);
            String[] textTokens = TextExtraction.GetTextTokens(text);
            double[] textVector = TextExtraction.CreateVectorFromText(textTokens, MapWordToColumn);
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
        #endregion Methods
    }
}