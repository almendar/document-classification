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
            DCDbTools.Instance.loadData();
            DCDbTools.Instance.loadMatricesFromDb();
            int i =  Data.Instance.AllCases.Count;
        }

        #endregion Methods
    }
}