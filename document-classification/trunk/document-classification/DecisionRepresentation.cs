using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentClassification.Representation
{
    [Serializable]
   public class DecisionRepresentationPeople : Dictionary<string, double>
   {
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
    public class AllDecisionsPhase : Dictionary<int, Dictionary<int, Dictionary<int, DecisionRepresentationPhase>>> 
    {
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
    public class DecisionRepresentationPhase :  Dictionary<string, double>
    {
        private int procedureId;
        private int personId;
        private int nextPersonId;
    }
}
