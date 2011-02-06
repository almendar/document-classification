﻿namespace DocumentClassification.BagOfWords
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    class VectorLenghtException : Exception
    {
        #region Fields

        private string p;

        #endregion Fields

        #region Constructors

        public VectorLenghtException(string p)
            : base(p)
        {
            this.p = p;
        }

        VectorLenghtException()
        {
        }

        #endregion Constructors
    }

    class VectorOperations
    {
        #region Methods

        public static double DotProduct(double[] v1, double[] v2)
        {
            if (v1.Length != v2.Length)
                throw new VectorLenghtException("Vectors are not equal length");
            double dotProductRet = 0.0;
            int vectorDimension = v1.Length;

            for (int i = 0; i < vectorDimension; i++)
            {
                dotProductRet += v1[i] * v2[i];
            }
            return dotProductRet;
        }

        public static double VectorLength(double[] v1)
        {
            double vectorLenRet = 0.0;
            foreach (double xi in v1)
            {
                vectorLenRet += (xi * xi);
            }
            return Math.Sqrt(vectorLenRet);
        }

        public static double VectorsConsine(double[] v1, double[] v2)
        {
            if (v1.Length != v2.Length)
                throw new VectorLenghtException("Vectors are not equal length");
            double dotProduct = DotProduct(v1, v2);
            double vec1Len = VectorLength(v1);
            double vec2Len = VectorLength(v2);
            double ret = 0.0d; //no familiar at all
            if (vec1Len * vec2Len != 0)
            {
                ret = (dotProduct) / (vec1Len * vec2Len);
            }
            return ret;
        }

        #endregion Methods
    }
}