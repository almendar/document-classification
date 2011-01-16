﻿using System;
using System.Collections.Generic;
using System.Text;

namespace document_classification
{
    /// <summary>
    /// Singleton instance of classificator based on bag-of-words paradigm
    /// </summary>
    class BagOfWordsTextClassifier
    {
        #region Singleton stuff
        static readonly BagOfWordsTextClassifier instance = new BagOfWordsTextClassifier();
        public static BagOfWordsTextClassifier Instance
        {
            get
            {
                return instance;
            }
        }
        static BagOfWordsTextClassifier()
        {
        }
        #endregion
        
        #region fields
        private int NumberOfCases;
        private DBRepresentation allWords = null;
        private AllCases CasesSet = null;
        private AllProcedures ProceduresSet = null;
        private AllDecisionsPhase PhaseDecisionData = null;
        private AllDecisionsPeople PeopleDecisionData = null;

        /// <summary>
        /// Maximum percentage of cases in which word can appear to be take into consideration
        /// </summary>
        private const double MaximumFrequency = 0.9;

        /// <summary>
        /// How many words are being used in computation
        /// </summary>
        private int NumberOfMeaningfulWords;

        /// <summary>
        /// Number of procedures read from database
        /// </summary>
        private int numberOfProcedures;

        /// <summary>
        /// Threashold above which words are not taken into consideration.
        /// When wors appears in more documents that trheashold stand for
        /// word is being omited
        /// </summary>
        private int wordThreshold = int.MaxValue;

        /// <summary>
        /// Maps word to column number.
        /// Take into consideration that not evey word is present because of the threashold.
        /// Only words that appeare in less than <see cref="MaximumFrequency"/> percentage of documents
        /// will be taken into consideration
        /// </summary>
        private Dictionary<String, int> MapWordToColumn = null;

        //**********************************************************************//
        /// <summary>
        /// Rows of this matrix represents procedures, columns are words <see cref="MapWordToColumn"/>.
        /// Values of this matrix are TFIDF of words in procedures.
        /// </summary>
        private double[][] ProcedureMatrix  = null;
        
        /// <summary>
        /// Table maps row of the <see cref="ProcedureMatrix"/> to correspondent procedure id 
        /// in the database.
        /// </summary>
        private int[] MapRowToProcedureId = null;
        

        //TODO
        //**********************************************************************//
        private double[,] PhaseMatrix = null;
        private Dictionary<int, Dictionary<int, List<int>>> MapProcIdPhasIdToRowsSet = null;
        private int [] MapRowToNextPhaseId = null;

        //**********************************************************************//
        private double[,,,] PeopleMatrix     = null;
        private int[,,] MapRowColumnToNextPersonId = null;


        
#endregion
        

        BagOfWordsTextClassifier()
        {
            ReadDataBase();
            ComputeStatisticParams();
            FetchMeaningfulWords();
            CreateDataMatrices();
        }

        private void ComputeStatisticParams()
        {
            this.numberOfProcedures = ProceduresSet.Keys.Count;
            this.NumberOfMeaningfulWords = MapWordToColumn.Keys.Count; //vector length
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

        public int[] NextStagePrediciton(string text)
        {
            throw new NotImplementedException();
        }


        public int[] NextPersonPrediction(string text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Takes table of strings and count frequency of words in vector
        /// that are listed in <see cref="MapWordToColumn"/>
        /// </summary>
        /// <param name="textTokens">Tokens from text</param>
        /// <returns>Vector with document TF of words</returns>
        private double[] CreateVectorFromText(string[] textTokens)
        {
            double [] vectorRep = new double[NumberOfMeaningfulWords];
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
        /// Builds the <see cref="ProcedureMatrix"/> out of <see cref="ProceduresSet"/> for words
        /// that are listed in <see cref="MapWordToColumn"/>
        /// </summary>
        private void ProcedureMatrixBuild()
        {

            //int nrOfProcedures = ProceduresSet.Keys.Count;
            ProcedureMatrix = new double[numberOfProcedures][];
            for (int h = 0; h < numberOfProcedures; h++)
            {
                ProcedureMatrix[h] = new double[NumberOfMeaningfulWords];
            }
            int procedurIndex = 0;
            foreach (KeyValuePair<int, Procedure> kvp in ProceduresSet)
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

        private void NextPhaseMatrixBuild()
        {
            int nrOfDecisions = PhaseDecisionData.GetNrOfDecisions();
            PhaseMatrix = new double[nrOfDecisions, NumberOfMeaningfulWords];
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all words that frequency in <see cref="CasesSet"/> is less than threashold
        /// </summary>
        private void FetchMeaningfulWords()
        {
            this.wordThreshold = (int) Math.Floor((NumberOfCases * MaximumFrequency));
            int vectorIndice = 0;
            foreach(KeyValuePair<string, int> kvp in allWords)
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
        
    }
}
