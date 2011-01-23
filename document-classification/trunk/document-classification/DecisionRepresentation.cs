using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace DocumentClassification.Representation
{
    [Serializable]
   public class DecisionRepresentationPeople : Dictionary<string, double>
   {
        public DecisionRepresentationPeople() : base()
        {
        }
        public DecisionRepresentationPeople(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        private int procedureId;
        private int phaseId;
        private int nextPhaseId;
    }

    /// <summary>
    /// ProcedurID,PersonId,nextPersonId
    /// </summary>

    [Serializable]
    public class AllDecisionsPeople : Dictionary<int, Dictionary<int, Dictionary<int, DecisionRepresentationPeople>>>
    {
        public AllDecisionsPeople() : base()
        {
        }
        public AllDecisionsPeople(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
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
    }

    /// <summary>
    /// ProcedurID,PersonId,nextStageId
    /// </summary>
    [Serializable]
    public class AllDecisionsStatus : Dictionary<int, Dictionary<int, Dictionary<int, DecisionRepresentationStatus>>> 
    {
        public AllDecisionsStatus()
            : base()
        {
        }
        public AllDecisionsStatus(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
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
    }

    [Serializable]
    public class DecisionRepresentationStatus :  Dictionary<string, double>
    {
        public DecisionRepresentationStatus() : base()
        {
        }
        public DecisionRepresentationStatus(Dictionary<string, double> dict) : base(dict)
        {
        }
        public DecisionRepresentationStatus(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}
