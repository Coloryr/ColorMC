using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.CurseForge;

public record CurseForgeGetFilesObj
{
    [JsonPropertyName("fileIds")]
    public List<long> FileIds { get; set; }
}

public record CurseForgeGetFilesResObj
{
    [JsonPropertyName("data")]
    public List<CurseForgeModObj.DataObj> Data { get; set; }
}
