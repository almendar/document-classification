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
    public class AllDecisionsNextPerson : Dictionary<int, Dictionary<int, Dictionary<int, DecisionRepresentationNextPerson>>>
    {
        #region Constructors

        public AllDecisionsNextPerson()
            : base()
        {
        }

        public AllDecisionsNextPerson(SerializationInfo info, StreamingContext context)
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
    public class AllDecisionsNextStage : Dictionary<int, Dictionary<int, Dictionary<int, DecisionRepresentationNextStage>>>
    {
        #region Constructors

        public AllDecisionsNextStage()
            : base()
        {
        }

        public AllDecisionsNextStage(SerializationInfo info, StreamingContext context)
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
    public class DecisionRepresentationNextPerson : Dictionary<string, double>
    {
        #region Fields

        private Case p;

        #endregion Fields

        #region Constructors

        public DecisionRepresentationNextPerson()
            : base()
        {
        }

        public DecisionRepresentationNextPerson(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public DecisionRepresentationNextPerson(Case p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }

        #endregion Constructors
    }

    [Serializable]
    public class DecisionRepresentationNextStage : Dictionary<string, double>
    {
        #region Constructors

        public DecisionRepresentationNextStage()
            : base()
        {
        }

        public DecisionRepresentationNextStage(Dictionary<string, double> dict)
            : base(dict)
        {
        }

        public DecisionRepresentationNextStage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
    }
}