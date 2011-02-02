namespace DocumentClassification.Representation
{
    using System;
    using System.Collections.Generic;

    public class CasesTF : Dictionary<int, Dictionary<string, int>>
    {
        #region Constructors

        public CasesTF()
        {
        }

        #endregion Constructors

        #region Methods

        public void Add(Dictionary<int, Dictionary<string, int>> data)
        {
            foreach (int caseId in data.Keys)
            {
                Add(caseId, data[caseId]);
            }
        }

        #endregion Methods
    }

    public class IDFcalculation : Dictionary<string, IDFData>
    {
        #region Fields

        private int D;
        private double logD;

        #endregion Fields

        #region Constructors

        private IDFcalculation()
        {
        }

        #endregion Constructors

        #region Methods

        public void calculateIDF()
        {
            logD = Math.Log10(D);

            foreach (IDFData data in this.Values)
            {
                data.calculateIDF(logD);
            }
        }

        public double calculateIDF(string word)
        {
            return this[word].calculateIDF(logD);
        }

        public void dfChanged(string word, int newDf)
        {
            this[word].LogDF = Math.Log10(newDf);
        }

        public void setNumberOfCases(int D)
        {
            this.D = D;
            logD = Math.Log10(D);
        }

        #endregion Methods
    }

    public class IDFcalculationHelper
    {
        #region Fields

        private static IDFcalculationHelper instance = new IDFcalculationHelper();

        private CasesTF casesTF;
        private IDFcalculation idfCalculation;

        #endregion Fields

        #region Constructors

        private IDFcalculationHelper()
        {
            casesTF = DCDbTools.Instance.getCasesTF();
            idfCalculation = DCDbTools.Instance.getIDFcalculation();
        }

        #endregion Constructors

        #region Properties

        public static IDFcalculationHelper Instance
        {
            get
            {
                return instance;
            }
        }

        public CasesTF CasesTF
        {
            get
            {
                return casesTF;
            }
            set
            {
                casesTF = value;
            }
        }

        public IDFcalculation IDFcalculation
        {
            get
            {
                return idfCalculation;
            }
            set
            {
            idfCalculation = value;
            }
        }

        #endregion Properties
    }

    public class IDFData
    {
        #region Fields

        private double idf;
        private double logdf;

        #endregion Fields

        #region Constructors

        public IDFData(double idf, double logdf)
        {
            this.idf = idf;
            this.logdf = logdf;
        }

        public IDFData(double df)
        {
            idf = 0;
            logdf = Math.Log10(df);
        }

        #endregion Constructors

        #region Properties

        public double IDF
        {
            get
            {
                return idf;
            }
            set
            {
                idf = value;
            }
        }

        public double LogDF
        {
            get
            {
                return logdf;
            }
            set
            {
                logdf = value;
            }
        }

        #endregion Properties

        #region Methods

        public double calculateIDF(double logD)
        {
            return idf = logD - logdf;
        }

        #endregion Methods
    }
}