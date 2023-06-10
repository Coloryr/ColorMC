using ColorMC.Gui.UI.Model.Main;

namespace ColorMC.Gui.UI.Model;

public interface IMainTop
{
    void Launch(GameItemModel obj);
    void Select(GameItemModel? model);
    void EditGroup(GameItemModel model);
}
