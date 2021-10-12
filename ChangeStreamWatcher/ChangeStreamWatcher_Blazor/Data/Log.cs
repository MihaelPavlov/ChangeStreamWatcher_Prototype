using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace ChangeStreamWatcher_Blazor.Data
{
    public class Log
    {
        public bool ShowDetails { get; set; }

        public string Id { get; set; }

        public string OperationType { get; set; }

        public string FullDocument { get; set; }

        public FullDocument FullDocumentDeserialize { get; set; }




    }
}
