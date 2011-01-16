using System;
using System.Collections.Generic;
using System.Text;

namespace document_classification
{
    class DecisionRepresentationPeople : Dictionary<string, double>
    {
        private int procedureId;
        private int phaseId;
        private int nextPhaseId;
    }

    /// <summary>
    /// ProcedurID,PersonId,nextPersonId
    /// </summary>

    class AllDecisionsPeople : Dictionary<int, Dictionary<int, Dictionary<int, DecisionRepresentationPeople>>>
    {
        public int getNrOfDecisions()
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

    public class DecisionRepresentationPhase :  Dictionary<string, double>
    {
        private int procedureId;
        private int personId;
        private int nextPersonId;
    }
}
