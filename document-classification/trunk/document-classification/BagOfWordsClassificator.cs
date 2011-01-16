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

        #region fields
        private int                          NumberOfCases;
        private DBRepresentation             allWords = null;
        private AllCases                     CasesSet = null;
        private AllProcedures                ProceduresSet = null;
        private DecisionRepresentationPhase  PhaseDecisionData = null;
        private DecisionRepresentationPeople PeopleDecisionData = null;
        

        /// <summary>
        /// Threashold above which words are not taken into consideration.
        /// When wors appears in more documents that trheashold stand for
        /// word is being omited
        /// </summary>
        private int wordThreashold = int.MaxValue;

        /// <summary>
        /// Maps word to column number.
        /// Take into consideration that not evey word is present because of the threashold of the TFIDF.
        /// </summary>
        private Dictionary<String, int> MapWordToColumn = null;

        //**********************************************************************//
        /// <summary>
        /// Rows of this matrix represents procedures, columns are words.
        /// Values of this matrix are TFIDF of words in procedures.
        /// </summary>
        private double[,] ProcedureMatrix  = null;
        /// <summary>
        /// Table maps row of the <see cref="ProcedureMatrix"/>procedure matrix</see> to correspondent procedure id 
        /// in the database.
        /// </summary>
        private int[] MapRowToProcedureId = null;

        //**********************************************************************//
        private double[,,,] PhaseMatrix      = null;
        private int[,,] MapRowColumnToNextPhaseId = null;

        //**********************************************************************//
        private double[,,,] PeopleMatrix     = null;
        private int[,,] MapRowColumnToNextPersonId = null;

        static readonly BagOfWordsTextClassifier instance = new BagOfWordsTextClassifier();
#endregion

        static BagOfWordsTextClassifier()
        {
        }

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



        private void ProcedureMatrixBuild()
        {
            int numberOfProcedures = ProceduresSet.Keys.Count;
            int nrOfMeaningfulWords = MapWordToColumn.Keys.Count;//vector length
            int nrOfProcedures = ProceduresSet.Keys.Count;
            ProcedureMatrix = new double[numberOfProcedures, nrOfMeaningfulWords];
            int procedurIndex = 0;
            foreach (KeyValuePair<int, Procedure> kvp in ProceduresSet)
            {
                int procedurId = kvp.Key;
                Procedure currentProcedure = kvp.Value;
                MapRowToProcedureId[procedurIndex] = procedurId;
                foreach (KeyValuePair<string, double> procedHashMap in currentProcedure)
                {
                    string word = procedHashMap.Key;
                    double TFIDF = procedHashMap.Value;
                    if (!MapWordToColumn.ContainsKey(word))
                    {
                        continue;
                    }
                    else
                    {
                        int wordIndex = MapWordToColumn[word];
                        ProcedureMatrix[procedurIndex,wordIndex] = TFIDF;
                    }
                }
                ++procedurIndex; //step to next procedure
            }

            
        }

        private void NextPhaseMatrixBuild()
        {
        }

        private void NextPersonMatrixBuild()
        {
        }

        /// <summary>
        /// Finds all words that frequency in documents is less than threashold
        /// </summary>
        private void FetchMeaningfulWords()
        {
            int numberOfProcedures = ProceduresSet.Keys.Count;
            this.wordThreashold = (numberOfProcedures * 9) / 10;
            int vectorIndice = 0;
            foreach(KeyValuePair<string, int> kvp in allWords)
            {
                int documentFrequency = kvp.Value;
                string word = kvp.Key;
                if (documentFrequency <= wordThreashold)
                {
                    MapWordToColumn.Add(word, vectorIndice);
                    ++vectorIndice;
                }

            }
        }
        
    }
}
