using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.ServerPack;

public record ModItem
{
    public string Name { get; set; }
    /// <summary>
    /// 是否可以从下载源下载
    /// </summary>
    public bool IsNet { get; set; }
    public SourceLocal Source { get; set; }
    public string Projcet { get; set; }
    public string File { get; set; }
    public string Sha1 { get; set; }
    public string Url { get; set; }
}
