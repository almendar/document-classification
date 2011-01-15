using System;
using System.Collections.Generic;
using System.Text;

namespace document_classification
{
    /// <summary>
    /// Singleton instance of classificator based on bag-of-words paradigm
    /// </summary>
    class BagOfWordsClassificator
    {
        static readonly BagOfWordsClassificator instance = new BagOfWordsClassificator();
        static BagOfWordsClassificator()  
        {
        }

        BagOfWordsClassificator()
        {
            //Should try to read serialised objects from database 
        }


        public static BagOfWordsClassificator Instance
        {
            get
            {
                return instance;
            }
        } 
        
    }
}
