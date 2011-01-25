namespace DocumentClassification.Representation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// ProcedurID,PersonId,nextPersonId
    /// </summary>
    [Serializable]
    public class AllDecisionsPeople : Dictionary<int, Dictionary<int, Dictionary<int, DecisionRepresentationPeople>>>
    {
        #region Constructors

        public AllDecisionsPeople()
            : base()
        {
        }

        public AllDecisionsPeople(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors

        #region Methods

        public int GetNrOfDecisions()
        {
            int nrRet = 0;
            foreach (int i in this.Keys)
                foreach (int j in this[i].Keys)
                    foreach (int k in this[i][j].Keys)
                    {
                        nrRet += 1;
                    }
            return nrRet;
        }

        #endregion Methods
    }

    /// <summary>
    /// ProcedurID,PersonId,nextStageId
    /// </summary>
    [Serializable]
    public class AllDecisionsStatus : Dictionary<int, Dictionary<int, Dictionary<int, DecisionRepresentationStatus>>>
    {
        #region Constructors

        public AllDecisionsStatus()
            : base()
        {
        }

        public AllDecisionsStatus(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors

        #region Methods

        public int GetNrOfDecisions()
        {
            int nrRet = 0;
            foreach(int i in this.Keys)
                foreach(int j in this[i].Keys)
                    foreach (int k in this[i][j].Keys)
                    {
                        nrRet += 1;
                    }
            return nrRet;
        }

        #endregion Methods
    }

    [Serializable]
    public class DecisionRepresentationPeople : Dictionary<string, double>
    {
        #region Fields

        private Case p;

        #endregion Fields

        #region Constructors

        public DecisionRepresentationPeople()
            : base()
        {
        }

        public DecisionRepresentationPeople(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public DecisionRepresentationPeople(Case p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }

        #endregion Constructors
    }

    [Serializable]
    public class DecisionRepresentationStatus : Dictionary<string, double>
    {
        #region Constructors

        public DecisionRepresentationStatus()
            : base()
        {
        }

        public DecisionRepresentationStatus(Dictionary<string, double> dict)
            : base(dict)
        {
        }

        public DecisionRepresentationStatus(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
    }
}