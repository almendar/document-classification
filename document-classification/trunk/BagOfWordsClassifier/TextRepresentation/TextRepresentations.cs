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
		/// <summary>
		/// Empty constructor 
		/// </summary>
        public AllCases(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
		/// <summary>
		/// Empty constructor 
		/// </summary>
        public AllCases()
            : base()
        {
        }

        #endregion Constructors

        #region Methods

		/// <summary>
		/// Returns how many cases are in the db 
		/// </summary>
		/// <returns>
		/// Count of cases
		/// </returns>
        public int getNumberOfCasesInDB()
        {
            return this.Keys.Count;
        }

        #endregion Methods
    }

    /// <summary>
    /// ProcedurID,PersonId,nextStageId
    /// </summary>
    [Serializable]
    public class AllDecisions : Dictionary<int, Dictionary<int, Dictionary<int, TextRepresentation>>>
    {
        #region Constructors

		/// <summary>
		/// Empty constructor 
		/// </summary>
        public AllDecisions()
            : base()
        {
        }

		/// <summary>
		/// Serialization constructor 
		/// </summary>		
        public AllDecisions(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
    }

    /// <summary>
    /// Represent all known procedures in the DB.
    /// Keys are IDs of the procedures stored in the DB.
    /// </summary>
    [Serializable]
    public class AllProcedures : Dictionary<int, TextRepresentation>
    {
        #region Constructors
		/// <summary>
		/// Empty constructor 
		/// </summary>
        public AllProcedures()
            : base()
        {
        }
		
		/// <summary>
		/// Serialization constructor 
		/// </summary>
        public AllProcedures(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors

        #region Methods

		/// <summary>
		/// rebuidl representation 
		/// </summary>
		/// <param name="allCases">
		/// A <see cref="AllCases"/>
		/// </param>
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

	/// <summary>
	/// All words that appear in texts and their count
	/// </summary>
    [Serializable]
    public class DBRepresentation : Dictionary<string, int>, ISerializable
    {
        #region Constructors

		/// <summary>
		/// Empty constructor 
		/// </summary>
        public DBRepresentation()
            : base()
        {
        }

		/// <summary>
		/// Serialization constructor 
		/// </summary>
        public DBRepresentation(SerializationInfo info, StreamingContext context)
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
		/// <summary>
		/// Empty constructor 
		/// </summary>
		/// <param name="procedureId">
		/// procedure id
		/// </param>
		/// <param name="caseId">
		/// case id
		/// </param>
        public TextRepresentation(int procedureId, int caseId)
        {
            this.procedureId = procedureId;
            this.caseId = caseId;
        }

		/// <summary>
		/// Serialization constructor 
		/// </summary>
		/// <param name="info">
		/// A <see cref="SerializationInfo"/>
		/// </param>
		/// <param name="context">
		/// A <see cref="StreamingContext"/>
		/// </param>
        public TextRepresentation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

		/// <summary>
		/// Copies reference 
		/// </summary>
		/// <param name="textRepresentation">
		/// A <see cref="TextRepresentation"/>
		/// </param>
        public TextRepresentation(TextRepresentation textRepresentation)
        {
            // TODO: Complete member initialization
            this.textRepresentation = textRepresentation;
        }

        #endregion Constructors

        #region Properties

		/// <summary>
		/// Case id 
		/// </summary>
        public int CaseId
        {
            get
            {
                return caseId;
            }
        }

		/// <summary>
		/// Procedure id 
		/// </summary>
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
		/// Adds to current text representation another text representation as simple set sum
		//  and adds words frequencies
		/// </summary>
		/// <param name="tr">
		/// A <see cref="TextRepresentation"/>
		/// </param>
        public void add(TextRepresentation tr)
        {
            foreach (string key in tr.Keys)
            {
                if (this.ContainsKey(key))
                    this[key] += tr[key];
                else
                    this[key] = tr[key];
            }
        }

        #endregion Methods
    }
}