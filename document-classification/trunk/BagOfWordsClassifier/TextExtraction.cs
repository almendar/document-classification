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

        public static String[] GetTextTokens(String text)
        {
            String trimmedText = text.Trim();
               String[] tokens = trimmedText.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
               return tokens;
        }

        #endregion Methods
    }
}