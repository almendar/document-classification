using System;
using System.Collections.Generic;
using System.IO;

using DocumentClassification.BagOfWords;
using DocumentClassification.BagOfWordsClassifier.Decisions;

public class TestApp
{
    #region Fields

    public string appName;

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

    static void Main()
    {
        ProcedureRecognitionTest(@"C:\Users\karol.galazka\Documents\test.txt");
        NextStageRecognitionTest(@"C:\Users\karol.galazka\Documents\test.txt", 4, 304);
        NextPersonRecognitionTest(@"C:\Users\karol.galazka\Documents\test.txt", 4, 2); //Tutaj sa testy :)
        Console.ReadKey();
    }

    private static void NextPersonRecognitionTest(string filePath, int procId, int phaseId)
    {
        TestApp app;
        BagOfWordsTextClassifier classifier;
        PrepareForClassification(filePath, out app, out classifier);
        ClassificationResult[] result = classifier.NextPersonPrediction(procId, phaseId, app.TextFile);
        WriteResults(result, "Next person id");
    }

    private static void NextStageRecognitionTest(string filePath, int procId, int phaseId)
    {
        TestApp app;
        BagOfWordsTextClassifier classifier;
        PrepareForClassification(filePath, out app, out classifier);
        ClassificationResult [] result = classifier.NextStagePrediciton(procId, phaseId, filePath);
        WriteResults(result, "Next stage id:");
    }

    private static void PrepareForClassification(string filePath, out TestApp app, out BagOfWordsTextClassifier classifier)
    {
        app = new TestApp();
        app.ReadTextFromFile(filePath);
        classifier = BagOfWordsTextClassifier.Instance;
    }

    private static void ProcedureRecognitionTest(string filePath)
    {
        /*
        TestApp app;
        BagOfWordsTextClassifier classifier;
        PrepareForClassification(filePath, out app, out classifier);
        ClassificationResult[] result = classifier.ProcedureRecognition(app.TextFile);
        WriteResults(result, "Procedure id:");
        */
    }

    private static void WriteResults(ClassificationResult[] resutl, string classifiedType)
    {
        foreach (ClassificationResult a in resutl)
        {
            System.Console.WriteLine(classifiedType + "{0} similarity: {1}", a.Id, a.Similarity);
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