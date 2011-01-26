using System;
using System.Collections.Generic;
using System.Text;

namespace BagOfWordsClassifier
{
    public class BestDecisionResult
    {
        private LinkedList<ClassificationResult> resultData;
        private int actualNrOfResultsAdded;
        public BestDecisionResult(int nrOfBestDecisions)
        {
            actualNrOfResultsAdded = 0;
            this.resultData = new LinkedList<ClassificationResult>();
            for (int i = 0; i < nrOfBestDecisions; i++)
            {
                resultData.AddLast(new ClassificationResult(0, Double.MaxValue));
            }
        }

        int MaximumNumberOfBestDecisions
        {
            get
            {
                return resultData.Count;
            }
        }

        int NrOfDecisionsClassified
        {
            get
            {
                if (actualNrOfResultsAdded == 0)
                {
                    return 0;
                }
                else if(MaximumNumberOfBestDecisions > actualNrOfResultsAdded)
                {
                    return actualNrOfResultsAdded;
                }
                else
                {
                    return MaximumNumberOfBestDecisions;
                }
            }
        }

        public ClassificationResult [] GetBestResults()
        {

            int nrOfResultToReturn = NrOfDecisionsClassified;
            if (nrOfResultToReturn == 0)
                return null;
            ClassificationResult[] ret = new ClassificationResult[nrOfResultToReturn];
            LinkedListNode<ClassificationResult> node = resultData.First;
            for (int i = 0; i < nrOfResultToReturn; i++, node = node.Next)
            {
                ret[i] = new ClassificationResult(node.Value);
            }
            return ret;
        }

        public bool addResult(ClassificationResult resultToAdd)
        {
            bool ret = false;

            LinkedListNode<ClassificationResult> bestListNode 
                = resultData.Last;

            while (bestListNode != null)
            {
                if (bestListNode.Value.Similarity < resultToAdd.Similarity && bestListNode == resultData.Last)
                {
                    //bestListNode = bestListNode.Previous;
                    //Console.WriteLine("Worst than worst");
                    ret = false;
                    break;
                }
                else if (bestListNode.Value.Similarity < resultToAdd.Similarity)
                {
                    //Console.WriteLine("Added to middle");
                    resultData.AddAfter(bestListNode, resultToAdd);
                    ++actualNrOfResultsAdded;
                    resultData.RemoveLast();
                    //bestListNode = bestListNode.Previous;
                    ret = true;
                    break;
                }
                else if (bestListNode.Value.Similarity >= resultToAdd.Similarity && 
                    bestListNode != resultData.First)
                {
                    //Console.WriteLine("Moving forward");
                    bestListNode = bestListNode.Previous;
                    continue;
                }

                else
                {
                    resultData.AddFirst(resultToAdd);
                    ++actualNrOfResultsAdded;
                    ret = true;
                    //Console.WriteLine("Insert at beginging");
                    resultData.RemoveLast();
                    //bestListNode = bestListNode.Previous;
                    break;
                }

            }
            return ret;
        }
    }

    public class ClassificationResult : IComparable<ClassificationResult>
    {
        #region Fields

        private readonly int id;
        private readonly double similarity;

        #endregion Fields

        #region Constructors

        public ClassificationResult(int id, double similarity)
        {
            this.id = id;
            this.similarity = similarity;
        }

        public ClassificationResult(ClassificationResult copy) : this(copy.Id, copy.Similarity)
        {
        }

        #endregion Constructors

        #region Properties

        public int Id
        {
            get
               {
               return id;
               }
        }

        public double Similarity
        {
            get
            {
               return similarity;
            }
        }

        #endregion Properties

        #region Methods

        public int CompareTo(ClassificationResult other)
        {
            return this.Similarity.CompareTo(other.Similarity);
        }

        #endregion Methods
    }
}

