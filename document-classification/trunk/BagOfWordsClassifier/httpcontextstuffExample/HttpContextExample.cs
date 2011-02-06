using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Threading;
using System.Globalization;
using System.Data.Common;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Web.Caching;
using DocumentClassification.BagOfWordsClassifier.Matrices;
namespace DocumentClassification.BagOfWordsClassifier.Decisions.httpcontextstuffExample
{
    class HttpContextExample
    {


        // Ta metoda musi gdzieś być bez zmian, jest podawana jako delegat do poniższej metody. 
        // Spokojnie może być statyczna
        public static void RefreshMatricesCallback(string key, Object value, CacheItemRemovedReason reason)
        {
            Object obj = null;
            HttpContext context =  HttpContext.Current;
            switch(key)
            {
                case BagOfWords.BagOfWordsTextClassifier.PROCEDURE_MATRICES:
                    obj = DataMatrices.Instance.ProcedureMatrices;
                    break;
                case BagOfWords.BagOfWordsTextClassifier.NEXT_PERSON_MATRICES:
                    obj = DataMatrices.Instance.NextPersonMatrices;
                    break;
                case BagOfWords.BagOfWordsTextClassifier.NEXT_STAGE_MATRICES:
                    obj = DataMatrices.Instance.NextStageMatrices;
                    break;
            }
        }


        // Ta też może być statycza. Musisz ją uruchomić po tym jak będzie można dostać się do macierzy, przy ładowaniu
        // DataMatrices
        private static void PutMatricesToHttpContext()
        {
            HttpContext context = HttpContext.Current;
            const double EXPIRATION_TIME = 3.0;

            context.Cache.Add(BagOfWords.BagOfWordsTextClassifier.PROCEDURE_MATRICES, DataMatrices.Instance.ProcedureMatrices, 
                null, DateTime.Now.AddHours(EXPIRATION_TIME), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, RefreshMatricesCallback);
            
            context.Cache.Add(BagOfWords.BagOfWordsTextClassifier.NEXT_STAGE_MATRICES, DataMatrices.Instance.NextStageMatrices,
                null, DateTime.Now.AddHours(EXPIRATION_TIME), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, RefreshMatricesCallback);
            
            context.Cache.Add(BagOfWords.BagOfWordsTextClassifier.NEXT_PERSON_MATRICES, DataMatrices.Instance.NextPersonMatrices,
                null, DateTime.Now.AddHours(EXPIRATION_TIME), Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, RefreshMatricesCallback);
          }

    }
}
