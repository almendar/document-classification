namespace DocumentClassification.BagOfWords
{
    using System;
    using System.Collections.Generic;
    using System.Text;

	/// <summary>
	/// Class responsible for text extraction.
	/// Is depreacted and not used anymore 
	/// </summary>
    public class TextExtraction
    {
        #region Fields

        private static char[] splitChars = new char[] {
                ' ',
                ',',
                '.',
                '!',
                '?',
                '\'',
                '"',
                ')',
                '(',
                '\n'};

        #endregion Fields

        #region Methods

        /// <summary>
        /// Takes table of strings and count frequency of words in vector
        /// that are listed in <see cref="MapWordToColumn"/>
        /// </summary>
        /// <param name="textTokens">Tokens from text</param>
        /// <returns>Vector with document TF of words</returns>
        public static double[] CreateVectorFromText(Dictionary<string,int> textTokens, Dictionary<string, int> MapWordToColumn)
        {
            int numberOfMeaningfulWords = MapWordToColumn.Count;
            double[] vectorRep = new double[numberOfMeaningfulWords];
            foreach (String word in textTokens.Keys)
            {
                if (!MapWordToColumn.ContainsKey(word))
                    continue;
                int indice = MapWordToColumn[word];
                vectorRep[indice] = textTokens[word];
            }
            return vectorRep;
        }

        public static String[] GetTextTokens(String text)
        {
            String trimmedText = text.Trim();
               String[] tokens = trimmedText.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
               return tokens;
        }

        #endregion Methods
    }
}