using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using Newtonsoft.Json;

namespace ColorMC.Gui.Objs;

public record CollectItemObj
{
    [JsonIgnore]
    public string UUID;
    /// <summary>
    /// 下载源
    /// </summary>
    public SourceType Source { get; set; }
    /// <summary>
    /// 资源类型
    /// </summary>
    public FileType FileType { get; set; }
    /// <summary>
    /// 资源名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 项目ID
    /// </summary>
    public string Pid { get; set; }
    /// <summary>
    /// 文件ID
    /// </summary>
    public string Fid { get; set; }
    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }
}

public record CollectObj
{
    public Dictionary<string, CollectItemObj> Items { get; set; }
    public Dictionary<string, List<string>> Groups { get; set; }

    public bool ModPack { get; set; }
    public bool Mod { get; set; }
    public bool ResourcePack { get; set; }
    public bool Shaderpack { get; set; }
}
