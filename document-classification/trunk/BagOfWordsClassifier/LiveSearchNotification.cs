using System;
using System.Collections.Generic;
using System.Text;
using DocumentClassification.BagOfWordsClassifier.Decisions;
namespace DocumentClassification.BagOfWordsClassifier.LiveSearch
{
    public class LiveSearchNotification
    {
        public delegate void NewResultHandler(object sender, SearchNotificationEventArgs sne);
        public event NewResultHandler newResult;

        public void NotifyAboutNewResult(ClassificationResult cr)
        {
            newResult(this, new SearchNotificationEventArgs(cr));
        }
    }


    public class SearchNotificationEventArgs : EventArgs
    {

        private ClassificationResult classificationResult;

        public SearchNotificationEventArgs(ClassificationResult cr)
        {
            classificationResult = cr;
        }

       public int ID 
        {
            get 
            {
                return classificationResult.Id;
            }
        }

       public double Similarity
        {
            get 
            {
                return classificationResult.Similarity;
            }
        }

        ClassificationResult Result
        {
            get
            {
                return classificationResult;
            }
        }
    }
}
