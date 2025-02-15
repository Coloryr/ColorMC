using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Gui.UI.Controls.NetFrp;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel
{
    [ObservableProperty]
    private bool _isSelfFrpEmpty;

    public ObservableCollection<NetFrpSelfItemModel> RemoteSelfFrp { get; set; } = [];

    private NetFrpSelfItemModel? _frpSelfItem;

    [RelayCommand]
    public void LoadSelfFrp()
    {
        RemoteSelfFrp.Clear();
        foreach (var item in FrpConfigUtils.Config.SelfFrp)
        {
            RemoteSelfFrp.Add(new(this, item));
        }

        IsSelfFrpEmpty = RemoteSelfFrp.Count == 0;
    }

    [RelayCommand]
    public async Task AddSelfFrp()
    {
        var model = new NetFrpAddModel();
        await DialogHost.Show(model, NetFrpTab6Control.NameCon);
        if (model.IsCancel || string.IsNullOrWhiteSpace(model.Name))
        {
            return;
        }

        if (FrpConfigUtils.Config.SelfFrp.Any(item => item.Name == model.Name))
        {
            Model.Show(App.Lang("NetFrpWindow.Tab6.Error1"));
            return;
        }

        var obj = model.Build();
        FrpConfigUtils.AddSelfFrp(obj);

        RemoteSelfFrp.Add(new(this, obj));
        IsSelfFrpEmpty = false;
    }

    /// <summary>
    /// 编辑自定义映射
    /// </summary>
    /// <param name="model"></param>
    public async void Edit(NetFrpSelfItemModel model)
    {
        var model1 = new NetFrpAddModel(model);
        await DialogHost.Show(model1, NetFrpTab6Control.NameCon);
        if (model1.IsCancel || string.IsNullOrWhiteSpace(model1.Name))
        {
            return;
        }

        var obj = model1.Build();
        model.Obj.IP = obj.IP;
        model.Obj.Key = obj.Key;
        model.Obj.Port = obj.Port;
        model.Obj.User = obj.User;
        model.Obj.NetPort = obj.NetPort;
        model.Obj.RName = obj.RName;
        model.Reload();
        FrpConfigUtils.Save();
    }

    public async void Delete(NetFrpSelfItemModel model)
    {
        var res = await Model.ShowAsync(App.Lang("NetFrpWindow.Tab6.Info1"));
        if (!res)
        {
            return;
        }

        FrpConfigUtils.RemoveSelfFrp(model.Obj);
        RemoteSelfFrp.Remove(model);

        IsSelfFrpEmpty = RemoteSelfFrp.Count == 0;
    }

    public void Select(NetFrpSelfItemModel model)
    {
        if (_frpSelfItem != null)
        {
            _frpSelfItem.IsSelect = false;
        }

        model.IsSelect = true;
        _frpSelfItem = model;
    }
}
