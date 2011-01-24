using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentClassification.Representation
{
    public class Data
    {
        private static readonly Data instance = new Data();
        public static Data Instance
        {
            get
            {
                return instance;
            }
        }

        static Data() { }

        private Data()
        {
            allCases = new AllCases();
            dbRepresentation = new DBRepresentation();
            allProcedures = new AllProcedures();
            allDecisionsStatus = new AllDecisionsStatus();
            allDecisionsPeople = new AllDecisionsPeople(); 
        }

        private AllCases allCases;
        private DBRepresentation dbRepresentation;
        private AllProcedures allProcedures;
        private AllDecisionsStatus allDecisionsStatus;
        private AllDecisionsPeople allDecisionsPeople;

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
    }
}
