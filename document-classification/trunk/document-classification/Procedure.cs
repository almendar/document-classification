namespace DocumentClassification.Representation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Represent all known procedures in the DB.
    /// Keys are IDs of the procedures stored in the DB.
    /// </summary>
    [Serializable]
    public class AllProcedures : Dictionary<int, TextRepresentation>
    {
        #region Constructors

        public AllProcedures()
            : base()
        {
        }

        public AllProcedures(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors

        #region Methods

        public void rebuild(AllCases allCases)
        {
            this.Clear();
            foreach (TextRepresentation tempCase in allCases.Values)
            {
                if (!this.ContainsKey(tempCase.ProcedureId))
                {
                    Add(tempCase.ProcedureId, new TextRepresentation(tempCase.ProcedureId, tempCase.CaseId));
                }
                this[tempCase.ProcedureId].add(tempCase);
            }
        }

        #endregion Methods
    }

}