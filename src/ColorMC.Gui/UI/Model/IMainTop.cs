using System.Collections.Generic;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;

namespace ColorMC.Gui.UI.Model;

public interface IMainTop
{
    void Launch(GameItemModel obj);
    void Launch(ICollection<GameItemModel> list);
    void Launch(string[] list);
    void Select(GameItemModel? model);
    void EditGroup(GameItemModel model);
    void DoStar(GameItemModel model);
    void SearchClose();
    GameItemModel? GetGame(string uuid);
}

public interface IMutTop : IMainTop
{
    bool IsMut { get; }
    void StartMut();
    void StartMut(GameGroupModel model);
    List<GameItemModel> EndMut();
    List<GameItemModel> GetMut();
    void MutLaunch();
    void MutEdit();
    void MutEditGroup();
    void MutDelete();
}