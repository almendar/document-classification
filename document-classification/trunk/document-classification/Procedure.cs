using System;
using System.Collections.Generic;
using System.Text;

namespace document_classification
{
    class Procedure : Dictionary<string, double>
    {

        /// <summary>
        /// Need to travers through case and add all of its TF-IDF
        /// </summary>
        /// <param name="?"></param>
        public void addCase(Case cas)
        {
        }

    }

    class AllProcedures : Dictionary<int, Procedure>
    {
    }
}
