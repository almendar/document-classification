using System;
using System.Collections.Generic;
using System.Text;
using DocumentClassification.Representation;

namespace DocumentClassification.DCUpdate
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
