using System;
using System.Collections.Generic;
using System.Text;
using DocumentClassification.Representation;
using DocumentClassification.DCUpdate;

namespace TestApp
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
