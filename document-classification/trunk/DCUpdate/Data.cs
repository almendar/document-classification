using System;
using System.Collections.Generic;
using System.Text;

namespace document_classification
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
        }

        private AllCases allCases;
        private DBRepresentation dbRepresentation;
        private AllProcedures allProcedures;

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
