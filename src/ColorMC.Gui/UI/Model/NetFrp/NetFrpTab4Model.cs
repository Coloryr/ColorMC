using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel
{
    public ObservableCollection<CloudServerModel> CloudServers { get; init; } = [];

    [RelayCommand]
    public void GetCloud()
    {
        LoadCloud();
    }

    public async void LoadCloud()
    {
        Model.Progress(App.Lang("NetFrpWindow.Tab4.Info1"));
        CloudServers.Clear();
        var list = await WebBinding.GetCloudServer();
        Model.ProgressClose();
        if (list == null)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab4.Error3"));
            return;
        }
        list.ForEach(CloudServers.Add);
    }
}
