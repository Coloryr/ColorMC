using System;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.User;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 账户显示
/// </summary>
public partial class UserDisplayModel : SelectItemModel
{
    public const string NameReload = "Reload";

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
    /// 上次登录
    /// </summary>
    public string Time => _obj.LastLogin.ToString("D");

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

    [ObservableProperty]
    private bool _emptyTex;

    /// <summary>
    /// 账户类型
    /// </summary>
    public AuthType AuthType => _obj.AuthType;

    /// <summary>
    /// 账户类型颜色
    /// </summary>
    public IBrush AuthColor => ColorManager.GetColor(_obj.AuthType);

    /// <summary>
    /// 能否刷新
    /// </summary>
    public bool CanRefresh => _obj.AuthType is not AuthType.Offline;
    /// <summary>
    /// 能否重新登录
    /// </summary>
    public bool CanRelogin => _obj.AuthType is not AuthType.Offline or AuthType.OAuth;
    /// <summary>
    /// 能否编辑
    /// </summary>
    public bool CanEdit => _obj.AuthType == AuthType.Offline;

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

                    EmptyTex = _img == null && _img1 == null && _img2 == null;
                });
            });
        }
        catch (Exception e)
        {
            Logs.Error(LangUtils.Get("AddModPackWindow.Text26"), e);
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
    [RelayCommand]
    public void Refresh()
    {
        _top.Refresh(this);
    }

    /// <summary>
    /// 重新登录
    /// </summary>
    [RelayCommand]
    public void Relogin()
    {
        _top.Relogin(this);
    }

    /// <summary>
    /// 删除
    /// </summary>
    [RelayCommand]
    public void Remove()
    {
        _top.Remove(this);
    }

    /// <summary>
    /// 编辑
    /// </summary>
    [RelayCommand]
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