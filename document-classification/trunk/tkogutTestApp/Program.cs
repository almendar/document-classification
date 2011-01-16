using System;
using System.Collections.Generic;
using System.Text;




namespace tkogutTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<int, Dictionary<int, List<int>>> a = new Dictionary<int, Dictionary<int, List<int>>>();
            a[1] = new Dictionary<int, List<int>>();
            a[1][2] = new List<int>();

            Console.Out.WriteLine(a[1][2].Count);


            double[] b = new double[50];

            for (int i = 0; i < b.Length; i++)
            {
                Console.Out.WriteLine(b[i]);
            }

            int k = (int)Math.Floor((34 * 0.9d));
            Console.Out.WriteLine(k);
            Console.In.ReadLine();



        }
    }

}
