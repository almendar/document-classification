namespace DocumentClassification.BagOfWordsClassifier.Decisions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AMODDecisionSupportClasses;
    using AMODClasses.Objects;

    /// <summary>
    /// Class stores best classification result.
    /// Number of result can be set.
    /// </summary>
    public class BestDecisionResult
    {
        #region Fields

        /// <summary>
        /// How many result were added to best result since 
        /// creation of this object
        /// </summary>
        private int actualNrOfResultsAdded;

        /// <summary>
        /// List of best results
        /// </summary>
        private LinkedList<ClassificationResult> resultData;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates this object
        /// </summary>
        /// <param name="nrOfBestDecisions">How many result will be stored</param>
        public BestDecisionResult(int nrOfBestDecisions, ClassificatorType type)
        {
            actualNrOfResultsAdded = 0;
            this.resultData = new LinkedList<ClassificationResult>();
            this.type  = type;
            for (int i = 0; i < nrOfBestDecisions; i++)
            {
                resultData.AddLast(new ClassificationResult(0, Double.MaxValue));
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Returns how many decisions may be taken
        /// </summary>
        int MaximumNumberOfBestDecisions
        {
            get
            {
                return resultData.Count;
            }
        }

        /// <summary>
        /// This returns how many result are being stored now
        /// and will be returned by <see cref="BestResults"/>
        /// </summary>
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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds a new result and classifies it. 
        /// Adds it if it is better than any of the present result
        /// and then removes last one.
        /// </summary>
        /// <param name="resultToAdd">Classification result to store and check for quality</param>
        /// <returns>True if result was better than any of the known results</returns>
        public bool addResult(ClassificationResult resultToAdd)
        {
            bool ret = false;

            if (!(resultToAdd.Similarity < 1.0))
            {
                return ret;
            }

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

       /// <summary>
       /// Returns list of best results
       /// </summary>
       /// <returns>
       /// List of best results
       /// </returns>
        public List<AMODPrediction> BestResults()
        {
            List<AMODPrediction> retList = new List<AMODPrediction>();
            int nrOfResultToReturn = NrOfDecisionsClassified;
            if (nrOfResultToReturn == 0)
                return retList;
            LinkedListNode<ClassificationResult> node = resultData.First;
            for (int i = 0; i < nrOfResultToReturn; i++, node = node.Next)
            {
                Object obj = null;
                switch(type)
                {
                    case ClassificatorType.Procedures:
                        obj = AMODProcedure.CreateAMODProcedure(node.Value.Id);
                        break;
                    case ClassificatorType.Stages:
                        obj = AMODProcedureStatus.CreateAMODProcedureStatus(node.Value.Id);
                        break;
                    case ClassificatorType.Users:
                        obj = AMODWorkflowUser.CreateAMODWorkflowUser(node.Value.Id);
                        break;
                }
                AMODPrediction prediction = new AMODPrediction(obj, 1.0d - node.Value.Similarity, type);
                retList.Add(prediction);
            }
            return retList;
        }

        #endregion Methods

        public ClassificatorType type { get; set; }
    }

    /// <summary>
    /// Represens classification result
    /// </summary>
    public class ClassificationResult : IComparable<ClassificationResult>
    {
        #region Fields

        private readonly int id;
        private readonly double similarity;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructs a classification result
        /// </summary>
        /// <param name="id">ID of the checked object</param>
        /// <param name="similarity">Similarity normalized to zero. That means closer to zero is more similar</param>
        public ClassificationResult(int id, double similarity)
        {
            this.id = id;
            this.similarity = similarity;
        }

        /// <summary>
        /// Constructs a copy of the classification result.
        /// </summary>
        /// <param name="copy"></param>
        public ClassificationResult(ClassificationResult copy)
            : this(copy.Id, copy.Similarity)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// ID of the classified object.
        /// </summary>
        public int Id
        {
            get
               {
               return id;
               }
        }

        /// <summary>
        /// Similarity rating. Closer to zero means more similar.
        /// </summary>
        public double Similarity
        {
            get
            {
               return similarity;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Standard method realization of the <see cref="IComparable"/> interface
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ClassificationResult other)
        {
            return this.Similarity.CompareTo(other.Similarity);
        }

        #endregion Methods
    }
}