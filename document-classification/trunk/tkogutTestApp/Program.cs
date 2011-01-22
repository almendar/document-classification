using System.IO;
using DocumentClassification.BagOfWords;

public class TestApp
{

    private string textFile = null;

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

    void ReadTextFromFile(string fileName)
    {
        TextReader tr = new StreamReader(fileName);
        TextFile = tr.ReadToEnd();
    }

    static void Main()
    {

        TestApp app = new TestApp();
        app.ReadTextFromFile(@"C:\Users\karol.galazka\Documents\test.txt");

        BagOfWordsTextClassifier classifier = BagOfWordsTextClassifier.Instance;
        int [] resutl = classifier.ProcedureRecognition(app.TextFile);
        foreach (int a in resutl)
        {
            System.Console.WriteLine("Best procedures: {0}", a);
        }
        System.Console.ReadLine();
    }
}