using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace document_classification
{
    [Serializable]
    public class DBRepresentation : Dictionary<string, int>, ISerializable
    {
        public DBRepresentation() : base()
        {
        }
        public DBRepresentation(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}