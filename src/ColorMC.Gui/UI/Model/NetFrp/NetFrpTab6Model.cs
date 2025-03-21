﻿using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel
{
    public const string NameCon1 = "ShareCon1";

    /// <summary>
    /// 是否没有自定义映射
    /// </summary>
    [ObservableProperty]
    private bool _isSelfFrpEmpty;

    /// <summary>
    /// 自定义映射列表
    /// </summary>
    public ObservableCollection<NetFrpSelfItemModel> RemoteSelfFrp { get; set; } = [];

    /// <summary>
    /// 选中的自定义映射项目
    /// </summary>
    private NetFrpSelfItemModel? _frpSelfItem;

    /// <summary>
    /// 加载自定义映射列表
    /// </summary>
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

    /// <summary>
    /// 添加自定义映射
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddSelfFrp()
    {
        var model = new NetFrpAddModel();
        var res = await DialogHost.Show(model, NameCon1);
        if (res is not true || string.IsNullOrWhiteSpace(model.Name))
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
    /// <param name="model">自定义映射</param>
    public async void Edit(NetFrpSelfItemModel model)
    {
        var model1 = new NetFrpAddModel(model);
        var res = await DialogHost.Show(model1, NameCon1);
        if (res is not true || string.IsNullOrWhiteSpace(model1.Name))
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

    /// <summary>
    /// 删除自定义映射
    /// </summary>
    /// <param name="model">自定义映射</param>
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

    /// <summary>
    /// 选中自定义映射
    /// </summary>
    /// <param name="model">自定义映射</param>
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
