using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.User;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 账户显示
/// </summary>
public partial class UserDisplayModel : SelectItemModel
{
    public const string NameReload = "Reload";

    public const double DefaultWidth = 350;

    public LoginObj Obj => _obj;

    /// <summary>
    /// 用户名
    /// </summary>
    public string Name => _obj.UserName;
    /// <summary>
    /// UUID
    /// </summary>
    public string UUID => _obj.UUID;
    /// <summary>
    /// 类型
    /// </summary>
    public string Type => _obj.AuthType.GetName();
    /// <summary>
    /// 附加信息
    /// </summary>
    public string Text1 => _obj.Text1;
    /// <summary>
    /// 附加信息
    /// </summary>
    public string Text2 => _obj.Text2;

    /// <summary>
    /// 宽度
    /// </summary>
    [ObservableProperty]
    private double _width = DefaultWidth;

    /// <summary>
    /// 是否有皮肤文件
    /// </summary>
    [ObservableProperty]
    private bool _haveSkin;
    /// <summary>
    /// 是否有披风文件
    /// </summary>
    [ObservableProperty]
    private bool _haveCape;

    /// <summary>
    /// 账户类型
    /// </summary>
    public AuthType AuthType => _obj.AuthType;

    /// <summary>
    /// 头像
    /// </summary>
    [ObservableProperty]
    public Bitmap? image = ImageManager.LoadBitmap;

    /// <summary>
    /// 皮肤
    /// </summary>
    [ObservableProperty]
    public Bitmap? _skin = ImageManager.LoadBitmap;

    /// <summary>
    /// 披风
    /// </summary>
    [ObservableProperty]
    public Bitmap? _cape = ImageManager.LoadBitmap;

    /// <summary>
    /// 头像 皮肤 披风
    /// </summary>
    private Bitmap? _img, _img1, _img2;

    private readonly UsersModel _top;
    private readonly LoginObj _obj;

    public UserDisplayModel(UsersModel top, LoginObj obj)
    {
        _top = top;
        _obj = obj;

        try
        {
            Task.Run(async () =>
            {
                var temp = await ImageManager.GetUserSkinAsync(obj);
                if (temp.Data != null)
                {
                    using var skin = SKBitmap.Decode(temp.Data);
                    _img = ImageManager.GenHeadImage(skin);
                    _img1 = ImageManager.GenSkinImage(skin, temp.State);
                }
                var temp1 = await ImageManager.GetUserCapeAsync(obj);
                if (temp1 != null)
                {
                    using var cape = SKBitmap.Decode(temp1);
                    _img2 = ImageManager.GenCapeImage(cape);
                }
                Dispatcher.UIThread.Post(() =>
                {
                    HaveSkin = _img != null;
                    HaveCape = _img2 != null;
                    Image = _img;
                    Skin = _img1;
                    Cape = _img2;
                });
            });
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("AddModPackWindow.Error5"), e);
        }
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
        _top.Select(this);
    }

    /// <summary>
    /// 刷新
    /// </summary>
    public void Refresh()
    {
        _top.Refresh(this);
    }

    /// <summary>
    /// 重新登录
    /// </summary>
    public void Relogin()
    {
        _top.Relogin(this);
    }

    /// <summary>
    /// 删除
    /// </summary>
    public void Remove()
    {
        _top.Remove(this);
    }

    /// <summary>
    /// 编辑
    /// </summary>
    public void Edit()
    {
        _top.Edit(this);
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Close()
    {
        _img?.Dispose();
        _img1?.Dispose();
    }

    /// <summary>
    /// 重载头像
    /// </summary>
    public void ReloadHead()
    {
        OnPropertyChanged(NameReload);

        Image = null;
        _img?.Dispose();
        Task.Run(async () =>
        {
            var temp = await ImageManager.GetUserSkinAsync(_obj);
            if (temp.Data != null)
            {
                using var skin = SKBitmap.Decode(temp.Data);
                _img = ImageManager.GenHeadImage(skin);
            }
            Dispatcher.UIThread.Post(() =>
            {
                Image = _img;
            });
        });
    }
}