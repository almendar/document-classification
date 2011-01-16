using System;
using System.Collections.Generic;
using System.Text;

namespace document_classification
{
    class DCTools
    {
        public void save()
        {
            DBRepresentation dBRepresentation = DBRepresentation.Instance;
        }
        public void update()
        {
            DBRepresentation.Instance.update();
        }
        public double calculateTFIDF(int TF, int d)
        {
            return 0;
        }
    }
}
