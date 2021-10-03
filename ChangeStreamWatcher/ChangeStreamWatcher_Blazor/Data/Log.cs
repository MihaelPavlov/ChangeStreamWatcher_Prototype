using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeStreamWatcher_Blazor.Data
{
    public class Log
    {
        public string Id { get; set; }
        public string Date_Time { get; set; }
        public string Operation_Type { get; set; }
        public string Document_Key { get; set; }
        public string Collection_Namespace { get; set; }

        public string Clusture_Time { get; set; }

        public string Backing_Document { get; set; }

        public string Full_Document { get; set; }
        public string Update_Description { get; set; }


    }
}
