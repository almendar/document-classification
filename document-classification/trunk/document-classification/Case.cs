﻿using System;
using System.Collections.Generic;
using System.Text;

namespace document_classification
{
    /// <summary>
    /// Represens a case in the flow.
    /// Assign to all words that case contains TF-IDF measure
    /// </summary>
    public class Case : Dictionary<String, double>
    {   
        /// <summary>
        /// Procedure id to which this Case is associated to.
        /// </summary>
        private readonly int procedureId;
        private readonly int caseId;

        public Case(int procedureId, int caseId)
        {
            this.procedureId = procedureId;
            this.caseId = caseId;
        }

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
    }

    /// <summary>
    /// Represents all cases that can be found in the DB
    /// Keys are id of the case in the DB.
    /// </summary>
    public class AllCases : Dictionary<int, Case>
    {
        static readonly AllCases instance = new AllCases();
        static AllCases()
        {
        }
        public static AllCases Instance
        {
            get
            {
                return instance;
            }
        }

        private AllCases()
        {
        }

        public int getNumberOfCasesInDB()
        {
            return this.Keys.Count;
        }
    }
}
