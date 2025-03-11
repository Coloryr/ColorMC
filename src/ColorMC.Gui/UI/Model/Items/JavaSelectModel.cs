using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

public partial class JavaSelectModel : JavaDisplayModel
{
    /// <summary>
    /// 是否选中
    /// </summary>
    public bool IsSelect { get; set; }

    public JavaSelectModel(JavaDisplayModel model)
    {
        Name = model.Name;
        Path = model.Path;
        MajorVersion = model.MajorVersion;
        Version = model.Version;
        Type = model.Type;
        Arch = model.Arch;
    }
}
