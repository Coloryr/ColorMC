using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Objs;

public record CloudDataObj
{
    public DateTime ConfigTime { get; set; }
    public List<string> Config { get; set; }
}

public record CloundListObj
{ 
    public string UUID { get; set; }
    public string Name { get; set; }
}
public record CloudWorldObj
{
    public string Name { get; set; }
    public string Time { get; set; }
    public string Icon { get; set; }
    public Dictionary<string, string> Files { get; set; }
}
