using System;
using System.Collections.Generic;
using System.Text;
using DocumentClassification.Representation;

namespace DocumentClassification.BagOfWordsClassifier.Matrices
{
    public class ProcedureMatrices
    {
        private double[][] dataMatrix;
        private int[] mapRowToId;
        private AllProcedures allProcedures;
        private Dictionary<string, int> mapWordToColumn;

        public ProcedureMatrices(AllProcedures allProcedures,
            Dictionary<String, int> mapWordToColumn)
        {

            this.allProcedures = allProcedures;
            this.mapWordToColumn = mapWordToColumn;

            int nrOfProcedures = allProcedures.Count;
            int nrOfMeaningfulWords = mapWordToColumn.Count;

            int[] MapRowToProcedureId = new int[nrOfProcedures];
            dataMatrix = new double[nrOfProcedures][];
            for (int i = 0; i < nrOfProcedures; i++)
            {
                dataMatrix[i] = new double[nrOfMeaningfulWords];
            }
        }

        public int NrOfProcedures
        {
            get
            {
                return this.allProcedures.Count;
            }
        }

        public int NumberOfMeaningfullWords
        {
            get
            {
                return mapWordToColumn.Count;
            }
        }

        /// <summary>
        /// Builds the <see cref="ProcedureMatrices"/> out of <see cref="AllProcedures"/> for words
        /// that are listed in <see cref="MapWordToColumn"/>
        /// </summary>
        public void build()
        {
            int procedurIndex = 0;
            foreach (KeyValuePair<int, TextRepresentation> kvp in allProcedures)
            {
                int procedurId = kvp.Key;
                TextRepresentation currentProcedure = kvp.Value;
                MapRowToId[procedurIndex] = procedurId;
                foreach (KeyValuePair<string, double> textStatistic in currentProcedure)
                {
                    string word = textStatistic.Key;
                    double TFIDF = textStatistic.Value;

                    //Means word is not meaningful, and is not take into consideration.
                    if (!mapWordToColumn.ContainsKey(word))
                    {
                        continue;
                    }
                    else
                    {
                        int wordIndex = mapWordToColumn[word]; //Find what is this word place in vector
                        DataMatrix[procedurIndex][wordIndex] = TFIDF;
                    }
                }
                ++procedurIndex; //increment procedure indice
            }
        }


        public double[][] DataMatrix
        {
            get { return dataMatrix; }
            set { dataMatrix = value; }
        }
        public int[] MapRowToId
        {
            get { return mapRowToId; }
            set { mapRowToId = value; }
        }

    }
    public class NextDecisionMatrices
    {
        private double[][] dataMatrix = null;
        private int[] mapRowToNextId = null;
        private Dictionary<int, Dictionary<int, List<int>>> mapProcIdPhasIdToRowsSet = null;
        private AllDecisions allDecisions;
        private Dictionary<string, int> mapWordToColumn;


        public NextDecisionMatrices(AllDecisions allDecisions,
           Dictionary<String, int> mapWordToColumn)
        {
            this.allDecisions = allDecisions;
            this.mapWordToColumn = mapWordToColumn;
            int nrOfDecisions = CountPastDecisions();
            int nrOfMeaningfulWords = mapWordToColumn.Count;
            mapProcIdPhasIdToRowsSet = new Dictionary<int, Dictionary<int, List<int>>>();
            dataMatrix = new double[nrOfDecisions][];
            for (int i = 0; i < nrOfDecisions; i++)
            {
                dataMatrix[i] = new double[nrOfMeaningfulWords];
            }
            mapRowToNextId = new int[nrOfDecisions];

        }
        public void build()
        {
            int indexer = 0;
            foreach (int procId in allDecisions.Keys)
            {

                MapProcIdPhasIdToRowsSet[procId] =
                    new Dictionary<int, List<int>>();
                foreach (int phaseId in allDecisions[procId].Keys)
                {
                    MapProcIdPhasIdToRowsSet[procId][phaseId] = new List<int>();
                    foreach (int nextPhasId in allDecisions[procId][phaseId].Keys)
                    {
                        TextRepresentation textRepresentation = allDecisions[procId][phaseId][nextPhasId];
                        foreach (KeyValuePair<string, double> kvp in textRepresentation)
                        {

                            if (!mapWordToColumn.ContainsKey(kvp.Key))
                            {
                                continue;
                            }
                            else
                            {
                                String word = kvp.Key;
                                double TFIDF = kvp.Value;
                                int indice = mapWordToColumn[word];
                                DataMatrix[indexer][indice] = TFIDF;
                            }
                        }
                        MapProcIdPhasIdToRowsSet[procId][phaseId].Add(indexer);
                        MapRowToNextId[indexer] = nextPhasId;
                        indexer += 1;
                    }
                }
            }
        }
        private int CountPastDecisions()
        {
            int nrRet = 0;
            foreach (int i in allDecisions.Keys)
                foreach (int j in allDecisions[i].Keys)
                    foreach (int k in allDecisions[i][j].Keys)
                    {
                        nrRet += 1;
                    }
            return nrRet;
        }


        public Dictionary<int, Dictionary<int, List<int>>> MapProcIdPhasIdToRowsSet
        {
            get { return mapProcIdPhasIdToRowsSet; }
            set { mapProcIdPhasIdToRowsSet = value; }
        }
        public int[] MapRowToNextId
        {
            get { return mapRowToNextId; }
            set { mapRowToNextId = value; }
        }
        public double[][] DataMatrix
        {
            get { return dataMatrix; }
            set { dataMatrix = value; }
        }

        public int NumberOfDecisions
        {
            get
            {
                return dataMatrix.Length;
            }
        }


    }
    public class WordPicker
    {
        private AllCases allCases;
        private double maximumFrequency;
        private DBRepresentation dbRepresentation;


        public WordPicker(AllCases allCases, DBRepresentation dbRepresentation, double maxFrequency)
        {
            this.allCases = allCases;
            this.maximumFrequency = maxFrequency;
            this.dbRepresentation = dbRepresentation;
        }

        public Dictionary<string, int> FetchMeaningfulWords()
        {
            int numberOfCases = allCases.Count;
            int wordThreshold = (int)Math.Floor((numberOfCases * maximumFrequency));
            int vectorIndice = 0;
            Dictionary<string, int> mapWordToColumn = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> kvp in dbRepresentation)
            {
                int documentFrequency = kvp.Value;
                string word = kvp.Key;
                if (documentFrequency > wordThreshold)
                {
                    continue;
                }
                else
                {
                    mapWordToColumn[word] = vectorIndice;
                    vectorIndice += 1;
                }
            }
            return mapWordToColumn;
        }
    }
}