﻿namespace DocumentClassification.Representation
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Data
    {
        #region Fields

        private static readonly Data instance = new Data();

        private AllCases allCases;
        private AllDecisionsPeople allDecisionsPeople;
        private AllDecisionsStatus allDecisionsStatus;
        private AllProcedures allProcedures;
        private DBRepresentation dbRepresentation;

        #endregion Fields

        #region Constructors

        static Data()
        {
        }

        private Data()
        {
            allCases = new AllCases();
            dbRepresentation = new DBRepresentation();
            allProcedures = new AllProcedures();
            allDecisionsStatus = new AllDecisionsStatus();
            allDecisionsPeople = new AllDecisionsPeople();
        }

        #endregion Constructors

        #region Properties

        public static Data Instance
        {
            get
            {
                return instance;
            }
        }

        public AllCases AllCases
        {
            get
            {
                return allCases;
            }
            set
            {
                allCases = value;
            }
        }

        public AllDecisionsPeople AllDecisionsPeople
        {
            get
            {
                return allDecisionsPeople;
            }
            set
            {
                allDecisionsPeople = value;
            }
        }

        public AllDecisionsStatus AllDecisionsStatus
        {
            get
            {
                return allDecisionsStatus;
            }
            set
            {
                allDecisionsStatus = value;
            }
        }

        public AllProcedures AllProcedures
        {
            get
            {
                return allProcedures;
            }
            set
            {
                allProcedures = value;
            }
        }

        public DBRepresentation DBRepresentation
        {
            get
            {
                return dbRepresentation;
            }
            set
            {
                dbRepresentation = value;
            }
        }

        #endregion Properties
    }
}