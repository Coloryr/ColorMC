using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Frp;

public record SakuraFrpDownloadObj
{
    public record DownloadItemObj
    {
        public record Arch
        {
            public record ArchItem
            {
                public string title { get; set; }
                public string url { get; set; }
                public string hash { get; set; }
                public long size { get; set; }
            }

            public ArchItem windows_amd64 { get; set; }
            public ArchItem windows_arm64 { get; set; }
            public ArchItem linux_amd64 { get; set; }
            public ArchItem linux_arm64 { get; set; }
            public ArchItem darwin_amd64 { get; set; }
            public ArchItem darwin_arm64 { get; set; }
        }
        public string ver { get; set; }
        public long time { get; set; }
        public string note { get; set; }
        public Arch archs { get; set; }
    }
    public DownloadItemObj frpc { get; set; }
}
