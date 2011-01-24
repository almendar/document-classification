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
            //AmodDBTools.Instance.update();
            //DCDbTools.Instance.sendData();
            DCDbTools.Instance.loadData();
            int i = Data.Instance.AllDecisionsStatus.Count;
            //DCDbTools.Instance.sendData();
            /*
            DCDbTools.Instance.setCurrentVersion();
            DCDbTools.Instance.getAllDecisionsStatus();
            int i = Data.Instance.AllDecisionsStatus.Count;
            DCDbTools.Instance.loadData();
            int i = Data.Instance.AllCases.Count;
            */
        }
    }
}
