using System.Collections.Generic;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Model;

public interface IMainTop
{
    void Launch(GameItemModel obj);
    void Launch(ICollection<GameItemModel> list);
    void Launch(string[] list);
    void Select(GameItemModel? model);
    void EditGroup(GameItemModel model);
    void DoStar(GameItemModel model);
    GameItemModel? GetGame(string uuid);
}
