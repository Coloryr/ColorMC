﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs.Frp;

public record FrpDownloadObj
{
    public string linux_arm64 { get; set; }
    public string linux_amd64 { get; set; }
    public string darwin_amd64 { get; set; }
    public string darwin_arm64 { get; set; }
    public string windows_arm64 { get; set; }
    public string windows_amd64 { get; set; }
}
