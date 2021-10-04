using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeStreamWatcher_Blazor.Data
{
    public class Log
    {
        public string Id { get; set; }
        public string OperationType { get; set; }

        public string FullDocument { get; set; }


    }
}
