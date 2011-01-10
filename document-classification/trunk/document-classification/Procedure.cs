using System;
using System.Collections.Generic;
using System.Text;

namespace document_classification
{
    /// <summary>
    /// Represent a procedure.
    /// Procedure assigns to words TF-IDF measure sumed over all associated Cases with this procedure.
    /// </summary>
    class Procedure : Dictionary<string, double>
    {

        /// <summary>
        /// This procedure id from DB
        /// </summary>
        private readonly int procedureId;

        public Procedure(int procedureId)
        {
            this.procedureId = procedureId;
        }


        public int ProcedureId
        {
            get
            {
                return procedureId;
            }

        }

        /// <summary>
        /// Takes case and insert its key-value to the procedure.
        /// Adds additional value if key exists, or creates new
        /// entry for key that is inserted for the first time.
        /// </summary>
        /// <param name="singleCase">Case object that belongs to this procedure</param>
        public void addCase(Case singleCase)
        {
            foreach (string word in singleCase.Keys)
            {
                if (this.ContainsKey(word))
                {
                    this[word] += singleCase[word];
                }
                else
                {
                    this[word] = singleCase[word];
                }
            }
        }

        public void addCases(AllCases allCases)
        {
            foreach (Case singleCase in allCases.Values)
            {
                //Add cases that has the same procedureId
                if (this.ProcedureId == singleCase.ProcedureId)
                {
                    addCase(singleCase);
                }
            }
        }

    }

    /// <summary>
    /// Represent all known procedures in the DB.
    /// Keys are id's of the procedures stored in the DB.
    /// </summary>
    class AllProcedures : Dictionary<int, Procedure>
    {
    }
}
