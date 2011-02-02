namespace DocumentClassification.BagOfWords
{
    using System;
    using System.Collections.Generic;
    using System.Text;

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
        public static double[] CreateVectorFromText(string[] textTokens, Dictionary<string, int> MapWordToColumn)
        {
            int numberOfMeaningfulWords = MapWordToColumn.Count;
            double[] vectorRep = new double[numberOfMeaningfulWords];
            foreach (String word in textTokens)
            {
                if (!MapWordToColumn.ContainsKey(word))
                    continue;

                int indice = MapWordToColumn[word];
                vectorRep[indice] += 1.0d;
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