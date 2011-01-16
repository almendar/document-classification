using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using document_classification;

namespace document_classification
{
    class Program
    {
        static void Main(string[] args)
        {
            AmodDBTools.Instance.update();
        }
    }
}
