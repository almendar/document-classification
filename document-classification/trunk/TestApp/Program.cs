namespace TestApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using DocumentClassification.DCUpdate;
    using DocumentClassification.Representation;
    using DocumentClassification.BagOfWordsClassifier.Matrices;

    class Program
    {
        #region Methods

        static void Main(string[] args)
        {
            AmodDBTools.Instance.rebuild();
            DCDbTools.Instance.sendData();
            DCDbTools.Instance.sendDataMatricesToDb();
            /*
            DCDbTools.Instance.loadData();
            DCDbTools.Instance.loadMatricesFromDb();
            int i =  Data.Instance.AllCases.Count;
            */
            //Dictionary<string, int> caseData = AmodDBTools.Instance.getData(64);
        }

        #endregion Methods
    }
}