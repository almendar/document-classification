using System;
using System.Collections.Generic;
using System.Text;

namespace document_classification
{
    class DBRepresentation : Dictionary<string, int>
    {
        private static DBRepresentation instance = null;
        public static DBRepresentation Instance
        {
            get
            {
                if(instance == null)
                    instance = new DBRepresentation();
                return instance;
            }
        }
        public string lastRecordDate
        {
            get
            {
                return lastRecordDate;
            }
            set;
        }
        private DBRepresentation() : base()
        {
        }
    }
}
