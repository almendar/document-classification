using System;
using System.Collections.Generic;
using System.Text;

using document_classification;


namespace tkogutTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
          
            String sampleText = "   Babka jaga, poszła do domu. Zrobła \"kotelty\" i w sumie to bezsensu. !!!";
            String[] tokens = TextExtraction.GetTextTokens(sampleText);

            foreach (String s in tokens)
            {
                Console.Out.WriteLine("#"+s+"#");

            }

            Console.ReadKey
                ();
        }


    }
}
