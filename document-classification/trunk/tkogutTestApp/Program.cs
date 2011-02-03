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
        int CONST_CASE_ID = 66;
        ProcedureRecognitionTest(CONST_CASE_ID);
        NextStageRecognitionTest(CONST_CASE_ID, 4, 304);
        NextPersonRecognitionTest(CONST_CASE_ID, 4, 2); //Tutaj sa testy :)
        Console.ReadKey();
    }

    private static void NextPersonRecognitionTest(int caseIdToClassifie, int procId, int phaseId)
    {
        ClassificationResult[] result = BagOfWordsTextClassifier.Instance.NextPersonPrediction(caseIdToClassifie,procId, phaseId);
        WriteResults(result, "Next person id");
    }

    private static void NextStageRecognitionTest(int caseIdToClassifie, int procId, int phaseId)
    {
        
        ClassificationResult[] result = BagOfWordsTextClassifier.Instance.NextStagePrediciton(caseIdToClassifie,procId, phaseId);
        WriteResults(result, "Next stage id:");
    }

    private static void ProcedureRecognitionTest(int procedureID)
    {
     
        ClassificationResult[] result = BagOfWordsTextClassifier.Instance.ProcedureRecognition(procedureID);
        WriteResults(result, "Procedure id:");  
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