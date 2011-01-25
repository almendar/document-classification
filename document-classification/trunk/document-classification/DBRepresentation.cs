namespace DocumentClassification.Representation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    [Serializable]
    public class DBRepresentation : Dictionary<string, int>, ISerializable
    {
        #region Constructors

        public DBRepresentation()
            : base()
        {
        }

        public DBRepresentation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Constructors
    }
}