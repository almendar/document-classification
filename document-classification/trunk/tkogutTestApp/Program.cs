using System;
using System.Collections.Generic;
using System.IO;

using DocumentClassification.BagOfWords;

public class TestApp
{
    #region Fields

    private string textFile = null;

    #endregion Fields

    #region Properties

    public string TextFile
    {
        set
        {
            textFile = value;
        }

        get
        {
            return textFile;
        }
    }

    #endregion Properties

    #region Methods

    private static void FindBestREsult()
    {
        List<ClassificationResult> testData = new List<ClassificationResult>();
        LinkedList<ClassificationResult> bestData = new LinkedList<ClassificationResult>();
        Random rand = new Random(DateTime.Now.Second);
        for (int i = 0; i < 1000; i++)
        {
            ClassificationResult cr = new ClassificationResult(rand.Next(), rand.NextDouble() * 1000);
            testData.Add(cr);
        }
        for (int i = 0; i < 3; i++)
        {
            bestData.AddLast(new ClassificationResult(0, Double.MaxValue));
        }

        foreach (ClassificationResult testNode in testData)
        {

            LinkedListNode<ClassificationResult> bestListNode = bestData.Last;
            //iterate from end to front
            while (bestListNode != null)
            {
                if (bestListNode.Value.Similarity < testNode.Similarity && bestListNode == bestData.Last)
                {
                    //Console.WriteLine("Worst then the worstes");
                    bestListNode = bestListNode.Previous;
                    break;

                }
                else if (bestListNode.Value.Similarity < testNode.Similarity)
                {
                    bestData.AddAfter(bestListNode, testNode);
                    //Console.WriteLine("Insert in middle: {0} -> {1}", bestListNode.Value.Similarity, testNode.Similarity);
                    bestData.RemoveLast();
                    bestListNode = bestListNode.Previous;
                    break;
                }
                else if (bestListNode.Value.Similarity >= testNode.Similarity && bestListNode != bestData.First)
                {
                    bestListNode = bestListNode.Previous;
                    continue;
                }
                else
                {
                    bestData.AddFirst(testNode);
                    //Console.WriteLine("Insert at beginging");
                    bestData.RemoveLast();
                    bestListNode = bestListNode.Previous;
                    break;
                }

            }

        }

        foreach (ClassificationResult cr in bestData)
        {
            Console.WriteLine("{0}", cr.Similarity);

            /* testData.Sort();
             Console.WriteLine("!!!!!!!!!");
             Console.WriteLine("{0}\n{1}\n{2}", testData[0].Similarity, testData[1].Similarity, testData[2].Similarity);
             */
        }
    }

    static void Main()
    {
        ProcedureRecognitionTest();
           // FindBestREsult();
        Console.ReadKey();
    }

    private static void ProcedureRecognitionTest()
    {
        TestApp app = new TestApp();
        app.ReadTextFromFile(@"C:\Users\karol.galazka\Documents\test.txt");

        BagOfWordsTextClassifier classifier = BagOfWordsTextClassifier.Instance;
        int [] resutl = classifier.NextPhasePrediciton(19, 338, app.TextFile);
        foreach (int a in resutl)
        {
            System.Console.WriteLine("Best procedures: {0}", a);
        }
        System.Console.ReadLine();
    }

    void ReadTextFromFile(string fileName)
    {
        TextReader tr = new StreamReader(fileName);
        TextFile = tr.ReadToEnd();
    }

    #endregion Methods
}