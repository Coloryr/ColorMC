using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.ServerPack;

public record ModItem
{
    public string File { get; set; }
    /// <summary>
    /// 是否可以从下载源下载
    /// </summary>
    public bool IsNet { get; set; }
    public SourceType Source { get; set; }
    public string Projcet { get; set; }
    public string FileId { get; set; }
    public string Sha1 { get; set; }
    public string Url { get; set; }
}
