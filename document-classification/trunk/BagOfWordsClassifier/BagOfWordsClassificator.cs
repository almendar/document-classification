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
        private int NumberOfCases;
        private DBRepresentation allWords = null;
        private AllCases CasesSet = null;
        private AllProcedures ProceduresSet = null;
        private AllDecisionsPhase PhaseDecisionData = null;
        private AllDecisionsPeople PeopleDecisionData = null;

        private const double MaximumFrequency = 0.9;

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
        private double[][] ProcedureMatrix  = null;
        /// <summary>
        /// Table maps row of the <see cref="ProcedureMatrix"/>procedure matrix</see> to correspondent procedure id 
        /// in the database.
        /// </summary>
        private int[] MapRowToProcedureId = null;
        

        //**********************************************************************//
        private double[,] PhaseMatrix = null;
        private Dictionary<int, Dictionary<int, List<int>>> MapProcIdPhasIdToRowsSet = null;
        private int [] MapRowToNextPhaseId = null;

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

        public static BagOfWordsTextClassifier Instance
        {
            get
            {
                return instance;
            }
        }

        private void CreateDataMatrices()
        {
            ProcedureMatrixBuild();
            
            //Not ready yet
            //NextPhaseMatrixBuild();
            //NextPersonMatrixBuild();
        }

        /// <summary>
        /// Reads serialized objects from database that will be base for building matrices
        /// </summary>
        private void ReadDataBase()
        {
            throw new NotImplementedException();
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
            for (int i = 0; i < ProcedureMatrix.Length; i++)
            {
                double [] checkedVector = ProcedureMatrix[i];
                double similarity = 1 - VectorOperations.VectorsConsine(checkedVector, textVector);
                if (similarity < bestSimilarity)
                {
                    bestSimilarity = similarity;
                    bestProcedureIndice = i;
                }
            }
            int bestProcedureId = MapRowToProcedureId[i];
            int[] ret = new int[] { bestProcedureId };
            return ret;
          }

        private double[] CreateVectorFromText(string[] textTokens)
        {
            double [] vectorRep = new double[MapWordToColumn.Keys.Count];
            foreach (String word in textTokens)
            {
                if (!MapWordToColumn.ContainsKey(word))
                    continue;

                int indice = MapWordToColumn[word];
                vectorRep[indice] += 1.0d;
            }
            return vectorRep;
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
            //int nrOfProcedures = ProceduresSet.Keys.Count;
            ProcedureMatrix = new double[numberOfProcedures][];
            for (int h = 0; h < numberOfProcedures; h++)
            {
                ProcedureMatrix[h] = new double[nrOfMeaningfulWords];
            }
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
                        ProcedureMatrix[procedurIndex][wordIndex] = TFIDF;
                    }
                }
                ++procedurIndex; //step to next procedure
            }

            
        }

        private void NextPhaseMatrixBuild()
        {
            int nrOfDecisions = PhaseDecisionData.GetNrOfDecisions();
            int nrOfWordsInVector = MapWordToColumn.Keys.Count;
            PhaseMatrix = new double[nrOfDecisions, nrOfWordsInVector];
            MapRowToNextPhaseId = new int[nrOfDecisions];
            int indexer = 0;
            foreach (int procId in PhaseDecisionData.Keys)
            {
                MapProcIdPhasIdToRowsSet[procId] = new Dictionary<int, List<int>>();
                foreach (int phaseId in PhaseDecisionData[procId].Keys)
                {
                    MapProcIdPhasIdToRowsSet[procId][phaseId] = new List<int>();
                    foreach (int nextPhasId in PhaseDecisionData[procId][phaseId].Keys)
                    {
                        Dictionary<string, double> textRepresentation = PhaseDecisionData[procId][phaseId][nextPhasId];
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
                                PhaseMatrix[indexer,indice]= TFIDF;

                                //wstaw wektor do tablicy
                                //dodaj do listy, gdzie są takie przejścia
                                //zwiększ indeks bo dalej trzeba szukać
                            }
                        }
                        
                        MapProcIdPhasIdToRowsSet[procId][phaseId].Add(indexer);
                        MapRowToNextPhaseId[indexer] = nextPhasId;
                        indexer += 1;
                    }
                }
            }
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
            this.wordThreashold = (int) Math.Floor((numberOfProcedures * MaximumFrequency));
            int vectorIndice = 0;
            foreach(KeyValuePair<string, int> kvp in allWords)
            {
                int documentFrequency = kvp.Value;
                string word = kvp.Key;
                if (documentFrequency > wordThreashold)
                    continue;
                MapWordToColumn.Add(word, vectorIndice);
                vectorIndice += 1;
            }
        }
        
    }
}
