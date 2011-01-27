﻿using System;
using System.Collections.Generic;
using System.IO;
using DocumentClassification.BagOfWords;
using DocumentClassification.BagOfWordsClassifier.Decisions;
using DocumentClassification.BagOfWordsClassifier.LiveSearch;

public class TestApp
{
    #region Fields

    private string textFile = null;
    public string appName;
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

        ProcedureRecognitionTest(@"d:\test.txt");
        //NextPersonRecognitionTest(@"path", 1, 2); //Tutaj sa testy :)
        //NextStageRecognitionTest(@"path", 2, 3);
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
        ClassificationResult[] result = classifier.NextStagePrediciton(procId, phaseId,app.TextFile);
        WriteResults(result, "Next stage id:");
    }

    private static void ProcedureRecognitionTest(string filePath)
    {
        TestApp app;
        BagOfWordsTextClassifier classifier;
        PrepareForClassification(filePath, out app, out classifier);
        ClassificationResult [] resutl = classifier.NextStagePrediciton(19, 338, app.TextFile);
        WriteResults(resutl, "Procedure id:");
    }

    private static void WriteResults(ClassificationResult[] resutl, string classifiedType)
    {
        foreach (ClassificationResult a in resutl)
        {
            System.Console.WriteLine(classifiedType + "{0} similarity: {1}", a.Id, a.Similarity);
        }
        System.Console.ReadLine();
    }

    private static void PrepareForClassification(string filePath, out TestApp app, out BagOfWordsTextClassifier classifier)
    {
        app = new TestApp();
        app.ReadTextFromFile(filePath);
        classifier = BagOfWordsTextClassifier.Instance;
    }

    void ReadTextFromFile(string fileName)
    {
        TextReader tr = new StreamReader(fileName);
        TextFile = tr.ReadToEnd();



    }


    

    public void TestLiveSearch(object sender, SearchNotificationEventArgs sne)
    {
        while (false)
        {
            Console.WriteLine(this.appName + " "  +sne.ID + " " + sne.Similarity);
            System.Threading.Thread.Sleep(300);
        }
    }

    public static void TestLiveSearch()
    {
        TestApp app = new TestApp();
        app.appName = "dadada";
        LiveSearchNotification lsn = new LiveSearchNotification();
        lsn.newResult += new LiveSearchNotification.NewResultHandler(app.TestLiveSearch);
        lsn.NotifyAboutNewResult(new ClassificationResult(1,0.01));
        while (true)
        {
            Console.WriteLine("AAA");
            System.Threading.Thread.Sleep(300);
        }
    }

    #endregion Methods
}