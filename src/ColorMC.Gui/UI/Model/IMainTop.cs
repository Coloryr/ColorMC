using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model;

public interface IMainTop
{
    void Launch(GameModel obj, bool debug);
    void Select(GameModel? model);
    void EditGroup(GameModel model);
}
