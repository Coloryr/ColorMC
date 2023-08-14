using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Model;

public interface IMainTop
{
    void Launch(GameItemModel obj);
    void Select(GameItemModel? model);
    void EditGroup(GameItemModel model);
}
