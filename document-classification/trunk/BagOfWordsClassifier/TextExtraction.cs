using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentClassification.BagOfWords
{
   public  class TextExtraction
    {

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

      public static String[] GetTextTokens(String text)
       {
           String trimmedText = text.Trim();
           String[] tokens = trimmedText.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
           return tokens;
       }
    }
}
