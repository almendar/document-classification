using System;
using System.Collections.Generic;
using System.Text;

namespace document_classification
{
    /// <summary>
    /// Singleton instance of classificator based on bag-of-words paradigm
    /// </summary>
    class BagOfWordsTextClassifier
    {
        static readonly BagOfWordsTextClassifier instance = new BagOfWordsTextClassifier();
        static BagOfWordsTextClassifier()  
        {
        }

        private DBRepresentation             allWords = null;
        private AllCases                     CasesSet = null;
        private DecisionRepresentationPhase  PhaseDecisionData = null;
        private DecisionRepresentationPeople PeopleDecisionData = null;

        //---------------------------------------------------------------//
        /// <summary>
        /// Rows of this matrix represents procedures, columns are words.
        /// Values of this matrix are TFIDF of words in procedures.
        /// </summary>
        private double[][] ProcedureMatrix  = null;
        /// <summary>
        /// Table maps row of the <see cref="ProcedureMatrix"/>procedure matrix</see> to correspondent procedure id 
        /// in the database.
        /// </summary>
        private int[] MapRowToProcedureId = null;
        /// <summary>
        /// Maps word to column number.
        /// Take into consideration that not evey word is present because of the threashold of the TFIDF.
        /// </summary>
        private Dictionary<String,int> MapWordToColumn = null;
        /// <summary>
        /// Threashold below which words are not taken into consideration.
        /// </summary>
        private double TFIDFThreshold = Double.NegativeInfinity;

        //---------------------------------------------------------------//
        private double[][][] PhaseMatrix      = null;
        private int[] MapHeightToNextPhaseId = null;

        //---------------------------------------------------------------//
        private double[][][] PeopleMatrix     = null;
        private int[] MapHeightToNextPersonId = null;

        BagOfWordsTextClassifier()
        {
            ReadDataBase();
            CreateDataMatrices();
        }

        private void CreateDataMatrices()
        {
            throw new NotImplementedException();
        }

        private void ReadDataBase()
        {
            throw new NotImplementedException();
        }


        public static BagOfWordsTextClassifier Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Based on text trys to find righ procedures for text
        /// </summary>
        /// <param name="text">Text of document</param>
        /// <returns>Procedures ids table</returns>
        public int[] ProcedureRecognition(string text)
        {
            throw new NotImplementedException();
        }

        public int[] NextPersonPrediction(string text)
        {
            throw new NotImplementedException();
        }

        public int[] NextStagePrediciton(string text)
        {
            throw new NotImplementedException();
        }

        
    }
}
