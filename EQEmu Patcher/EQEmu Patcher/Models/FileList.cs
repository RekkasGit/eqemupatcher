using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQEmu_Patcher.Models
{
    public class FileList
    {
        public string version { get; set; }

        public List<FileEntry> deletes { get; set; }
        public string downloadprefix { get; set; }
        public List<FileEntry> downloads { get; set; }
        public List<FileEntry> unpacks { get; set; }

    }
}
