using ColorMC.Gui.UI.Model.GameEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model;

public interface IScreenshotFuntion
{
    void SetSelect(ScreenshotModel item);
    void Load();
}
