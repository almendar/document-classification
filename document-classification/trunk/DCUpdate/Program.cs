using System;
using System.Collections.Generic;
using System.Text;
using document_classification;

namespace document_classification
{
    class Program
    {
        static void Main(string[] args)
        {
            AmodDBTools.Instance.update();
            DCDbTools.Instance.sendDBRepresentation();
            DCDbTools.Instance.sendAllCases();
            Data.Instance.AllProcedures.rebuild(Data.Instance.AllCases);
            DCDbTools.Instance.sendAllProcedures();
        }
    }
}
