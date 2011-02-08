namespace DocumentClassification.BagOfWords
{
    using System;
    using System.Collections.Generic;
    using System.Text;

	/// <summary>
	/// Exception thrown when vectors used for 
	/// math operations were not equal length 
	/// </summary>
    class VectorLenghtException : Exception
    {
        #region Fields

        private string p;

        #endregion Fields

        #region Constructors

		/// <summary>
		/// Constructor of this exception
		/// </summary>
		/// <param name="p">
		/// Msg of error
		/// </param>
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

	/// <summary>
	/// Class is responsible for certain math operations 
	/// </summary>
    class VectorOperations
    {
        #region Methods

		/// <summary>
		/// Dot product of two vectors
		/// </summary>
		/// <param name="v1">
		/// First of the vectors
		/// </param>
		/// <param name="v2">
		/// Second of the vectors
		/// </param>
		/// <returns>
		/// Value of the dot product of vectors v1 and v2
		/// </returns>
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

		/// <summary>
		/// Returns and euclidean lenght of the vector
		/// </summary>
		/// <param name="v1">
		/// Vector
		/// </param>
		/// <returns>
		/// Length of the vector
		/// </returns>
        public static double VectorLength(double[] v1)
        {
            double vectorLenRet = 0.0;
            foreach (double xi in v1)
            {
                vectorLenRet += (xi * xi);
            }
            return Math.Sqrt(vectorLenRet);
        }

		/// <summary>
		/// Computes cosine between two vectors 
		/// </summary>
		/// <param name="v1">
		/// The first vector
		/// </param>
		/// <param name="v2">
		/// The second vector
		/// </param>
		/// <returns>
		/// Cosine value of the vectors
		/// </returns>
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