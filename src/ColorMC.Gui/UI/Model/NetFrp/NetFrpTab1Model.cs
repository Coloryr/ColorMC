using ColorMC.Core.Net.Apis;
using ColorMC.Gui.UI.Controls.NetFrp.Items;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel : MenuModel
{
    [ObservableProperty]
    private string _key;
    [ObservableProperty]
    private string _user1ID;
    [ObservableProperty]
    private string _user1Name;

    private bool _isLoad1;

    public ObservableCollection<NetFrpRemoteModel> Remotes { get; set; } = new();

    partial void OnKeyChanged(string value)
    {
        if (_isLoad1)
            return;

        ConfigBinding.SetFrpKey1(Key);
    }

    [RelayCommand]
    public async Task TestKey1()
    {
        if (string.IsNullOrWhiteSpace(Key))
        {
            Model.Show("请输入密钥");
            return;
        }
        Model.Progress("正在获取用户信息");
        var res = await SakuraFrpAPI.GetUserInfo(Key);
        Model.ProgressClose();
        if (res == null)
        {
            Model.Show("用户信息获取失败");
            return;
        }

        User1ID = res.id.ToString();
        User1Name = res.name;
    }

    [RelayCommand]
    public async Task GetChannel1()
    {
        if (string.IsNullOrWhiteSpace(Key))
        {
            Model.Show("请输入密钥");
            return;
        }
        Model.Progress("正在获取隧道信息");
        var res = await SakuraFrpAPI.GetChannel(Key);
        Model.ProgressClose();
        if (res == null)
        {
            Model.Show("隧道信息获取失败");
            return;
        }

        Remotes.Clear();
        foreach (var item in res)
        {
            Remotes.Add(new(item));
        }
    }

    public async void Load()
    {
        _isLoad1 = true;

        if (FrpConfigUtils.Config.SakuraFrp is { } con)
        {
            Key = con.Key;
        }

        if (string.IsNullOrWhiteSpace(Key))
        {
            return;
        }

        Model.Progress("正在获取用户信息");
        var res = await SakuraFrpAPI.GetUserInfo(Key);
        Model.ProgressClose();
        if (res == null)
        {
            return;
        }

        User1ID = res.id.ToString();
        User1Name = res.name;
        Model.Progress("正在获取隧道信息");
        var res1 = await SakuraFrpAPI.GetChannel(Key);
        Model.ProgressClose();
        if (res1 == null)
        {
            return;
        }

        Remotes.Clear();
        foreach (var item in res1)
        {
            Remotes.Add(new(item));
        }

        _isLoad1 = false;
    }
}
