using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingTrack.AppData
{
    public sealed class JournalGridItem
    {
        public int ID_Record { get; set; }
        public string Date { get; set; }
        public string Player { get; set; }
        public string Group { get; set; }
        public string Training { get; set; }
        public bool IsPresent { get; set; }
    }
}
