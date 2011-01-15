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


    class DecisionRepresentationPhase :  Dictionary<string, double>
    {
        private int procedureId;
        private int personId;
        private int nextPersonId;
    }
}
