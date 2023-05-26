using ColorMC.Gui.UI.Model.Main;

namespace ColorMC.Gui.UI.Model;

public interface IMainTop
{
    void Launch(GameModel obj, bool debug);
    void Select(GameModel? model);
    void EditGroup(GameModel model);
}
