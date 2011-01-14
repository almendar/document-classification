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
            char[] splitChars = new char[] {
                ' ',
                ',',
                '.',
                '\n'};


            String text = ".aklsadkla fclaksc a;vcas;c.a';.cvac as c as c as ckasckla slkc aslkc aslkc     asc.";
            text = text.Trim();

            String [] literals = text.Split(splitChars,StringSplitOptions.RemoveEmptyEntries);

            foreach (String s in literals)r
            {
                Console.Out.WriteLine("#"+s+"#");

            }

            Console.ReadKey
                ();
        }


    }
}
