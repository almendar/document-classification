namespace DocumentClassification.Representation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
     /// <summary>
    /// ProcedurID,PersonId,nextStageId
    /// </summary>
    [Serializable]
    public class AllDecisions : Dictionary<int, Dictionary<int, Dictionary<int, TextRepresentation>>>
    {
        #region Constructors

        public AllDecisions()
            : base()
        {
        }

        public AllDecisions(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
    }
}