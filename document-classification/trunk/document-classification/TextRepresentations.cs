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



    [Serializable]
    public class DBRepresentation : Dictionary<string, int>, ISerializable
    {
        #region Constructors

        public DBRepresentation()
            : base()
        {
        }

        public DBRepresentation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
    }


    /// <summary>
    /// ProcedurID,PersonId,nextStageId
    /// </summary>
    [Serializable]
    public class AllDecisions : Dictionary<int, Dictionary<int, Dictionary<int, TextRepresentation>>>
    {
        #region Constructors

        public AllDecisions()
            : base()
        {
        }

        public AllDecisions(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
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