using ColorMC.Core.Net.Apis;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;
using System.Collections.Generic;
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

    [RelayCommand]
    public async Task TestKey()
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

    public void Load()
    {
        if (FrpConfigUtils.Config.SakuraFrp is { } con)
        {
            Key = con.Key;
        }
    }
}
