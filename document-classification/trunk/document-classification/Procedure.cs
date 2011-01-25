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
    public class AllProcedures : Dictionary<int, Procedure>
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
            foreach (Case tempCase in allCases.Values)
            {
                if (!this.ContainsKey(tempCase.ProcedureId))
                {
                    Add(tempCase.ProcedureId, new Procedure(tempCase.ProcedureId));
                }
                this[tempCase.ProcedureId].addCase(tempCase);
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// Represent a procedure.
    /// Procedure assigns to words TF-IDF measure sumed over all associated Cases with this procedure.
    /// </summary>
    [Serializable]
    public class Procedure : Dictionary<string, double>
    {
        #region Fields

        /// <summary>
        /// This procedure id from DB
        /// </summary>
        private readonly int procedureId;

        #endregion Fields

        #region Constructors

        public Procedure(int procedureId)
            : base()
        {
            this.procedureId = procedureId;
        }

        public Procedure()
            : base()
        {
        }

        public Procedure(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors

        #region Properties

        public int ProcedureId
        {
            get
            {
                return procedureId;
            }
        }

        #endregion Properties

        #region Methods

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

        public void addCase(AllCases allCases)
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

        #endregion Methods
    }
}