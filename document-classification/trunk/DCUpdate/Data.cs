using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace document_classification
{
    class Data
    {
        public static readonly Data instance = new Data();
        public static Data Instance
        {
            get
            {
                return instance;
            }
        }
        private Data()
        {
            allCases = new AllCases();
            dbRepresentation = new DBRepresentation();
        }
        private AllCases allCases;
        private DBRepresentation dbRepresentation;

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
    }
}
