using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Skin;
using ColorMC.Gui.UI.Model.User;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 账户显示
/// </summary>
public partial class UserDisplayModel(UsersModel top, LoginObj obj) : SelectItemModel
{
    public const double DefaultWidth = 350;

    public LoginObj Obj => obj;

    /// <summary>
    /// 用户名
    /// </summary>
    public string Name => obj.UserName;
    /// <summary>
    /// UUID
    /// </summary>
    public string UUID => obj.UUID;
    /// <summary>
    /// 类型
    /// </summary>
    public string Type => obj.AuthType.GetName();
    /// <summary>
    /// 附加信息
    /// </summary>
    public string Text1 => obj.Text1;
    /// <summary>
    /// 附加信息
    /// </summary>
    public string Text2 => obj.Text2;

    /// <summary>
    /// 宽度
    /// </summary>
    [ObservableProperty]
    private double _width = DefaultWidth;

    /// <summary>
    /// 账户类型
    /// </summary>
    public AuthType AuthType => obj.AuthType;

    /// <summary>
    /// 头像
    /// </summary>
    public Task<Bitmap?> Image => GetImage();
    /// <summary>
    /// 头像
    /// </summary>
    private Bitmap? _img;
    /// <summary>
    /// 获取头像
    /// </summary>
    /// <returns></returns>
    private async Task<Bitmap?> GetImage()
    {
        if (_img != null)
        {
            return _img;
        }

        try
        {
            await Task.Run(async () =>
            {
                var temp = await PlayerSkinAPI.DownloadSkin(obj);
                if (temp.Item1)
                {
                    var file = AssetsPath.GetSkinFile(obj);
                    var skin = SKBitmap.Decode(file);
                    using var data = Skin2DHead.MakeHeadImage(skin);
                    _img = new Bitmap(data);
                }
            });
            return _img;
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("AddModPackWindow.Error5"), e);
        }

        return null;
    }

    /// <summary>
    /// 设置小模式
    /// </summary>
    /// <param name="mode"></param>
    public void SetMin(bool mode)
    {
        if (mode)
        {
            Width = double.NaN;
        }
        else
        {
            Width = DefaultWidth;
        }
    }

    /// <summary>
    /// 选中
    /// </summary>
    public void Select()
    {
        top.Select(this);
    }

    /// <summary>
    /// 刷新
    /// </summary>
    public void Refresh()
    {
        top.Refresh(this);
    }

    /// <summary>
    /// 重新登录
    /// </summary>
    public void Relogin()
    {
        top.Relogin(this);
    }

    /// <summary>
    /// 删除
    /// </summary>
    public void Remove()
    {
        top.Remove(this);
    }

    /// <summary>
    /// 编辑
    /// </summary>
    public void Edit()
    {
        top.Edit(this);
    }
}