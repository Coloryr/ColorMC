using System;
using System.Collections.Generic;
using System.Text;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 结构文件展示
/// </summary>
public record SchematicDisplayModel
{
    public required SchematicObj Obj;

    public string Name => Obj.Name;
    public string Type => Obj.Type.GetName();
    public string Description => Obj.Description;
    public string Author => Obj.Author;
    public string BlockCount => string.Format(LangUtils.Get("GameEditWindow.Tab12.Text13"), Obj.BlockCount);
    public string BlockTypes => string.Format(LangUtils.Get("GameEditWindow.Tab12.Text14"), Obj.BlockTypes);
    public string Width => string.Format(LangUtils.Get("GameEditWindow.Tab12.Text15"), Obj.Width);
    public string Height => string.Format(LangUtils.Get("GameEditWindow.Tab12.Text15"), Obj.Height);
    public string Length => string.Format(LangUtils.Get("GameEditWindow.Tab12.Text15"), Obj.Length);
    public string Local => Obj.Local;
}
