using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQEmu_Patcher.Models
{
    public class FileEntry
    {
        public string name { get; set; }
        public string md5 { get; set; }
        public string date { get; set; }
        public string zip { get; set; }
        public int size { get; set; }
    }
}
