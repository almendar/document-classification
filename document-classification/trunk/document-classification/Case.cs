namespace DocumentClassification.Representation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Represents all cases that can be found in the DB
    /// Keys are id of the case in the DB.
    /// </summary>
    [Serializable]
    public class AllCases : Dictionary<int, TextRepresentation>
    {
        #region Constructors

        public AllCases(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public AllCases()
            : base()
        {
        }

        #endregion Constructors

        #region Methods

        public int getNumberOfCasesInDB()
        {
            return this.Keys.Count;
        }

        #endregion Methods
    }

    /// <summary>
    /// Represens a case in the flow.
    /// Assign to all words that case contains TF-IDF measure
    /// </summary>
    [Serializable]
    public class TextRepresentation : Dictionary<String, double>
    {
        #region Fields

        private readonly int caseId;

        /// <summary>
        /// Procedure id to which this Case is associated to.
        /// </summary>
        private readonly int procedureId;
        private TextRepresentation textRepresentation;

        #endregion Fields

        #region Constructors

        public TextRepresentation(int procedureId, int caseId)
        {
            this.procedureId = procedureId;
            this.caseId = caseId;
        }

        public void add(TextRepresentation tr)
        {
            foreach (string key in tr.Keys)
            {
                this[key] += tr[key];
            }
        }

        public TextRepresentation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public TextRepresentation(TextRepresentation textRepresentation)
        {
            // TODO: Complete member initialization
            this.textRepresentation = textRepresentation;
        }

        #endregion Constructors

        #region Properties

        public int CaseId
        {
            get
            {
                return caseId;
            }
        }

        public int ProcedureId
        {
            get
            {
                return procedureId;
            }
        }

        #endregion Properties
    }
}