using System;
using System.Collections.Generic;
using System.Text;

namespace document_classification
{
    public class DBRepresentation : Dictionary<string, int>
    {
        static readonly DBRepresentation instance = new DBRepresentation();
        public static DBRepresentation Instance
        {
            get
            {
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
