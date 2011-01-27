namespace TestApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using DocumentClassification.DCUpdate;
    using DocumentClassification.Representation;

    class Program
    {
        #region Methods

        static void Main(string[] args)
        {
            AmodDBTools.Instance.rebuild();
            AmodDBTools.Instance.update();
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

        #endregion Methods
    }
}