using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.User;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// �˻���ʾ
/// </summary>
public partial class UserDisplayModel : SelectItemModel
{
    public const string NameReload = "Reload";

    public const double DefaultWidth = 350;

    public LoginObj Obj => _obj;

    /// <summary>
    /// �û���
    /// </summary>
    public string Name => _obj.UserName;
    /// <summary>
    /// UUID
    /// </summary>
    public string UUID => _obj.UUID;
    /// <summary>
    /// ����
    /// </summary>
    public string Type => _obj.AuthType.GetName();
    /// <summary>
    /// ������Ϣ
    /// </summary>
    public string Text1 => _obj.Text1;
    /// <summary>
    /// ������Ϣ
    /// </summary>
    public string Text2 => _obj.Text2;

    /// <summary>
    /// ���
    /// </summary>
    [ObservableProperty]
    private double _width = DefaultWidth;

    /// <summary>
    /// �Ƿ���Ƥ���ļ�
    /// </summary>
    [ObservableProperty]
    private bool _haveSkin;
    /// <summary>
    /// �Ƿ��������ļ�
    /// </summary>
    [ObservableProperty]
    private bool _haveCape;

    /// <summary>
    /// �˻�����
    /// </summary>
    public AuthType AuthType => _obj.AuthType;

    /// <summary>
    /// ͷ��
    /// </summary>
    [ObservableProperty]
    public Bitmap? image = ImageManager.LoadBitmap;

    /// <summary>
    /// Ƥ��
    /// </summary>
    [ObservableProperty]
    public Bitmap? _skin = ImageManager.LoadBitmap;

    /// <summary>
    /// ����
    /// </summary>
    [ObservableProperty]
    public Bitmap? _cape = ImageManager.LoadBitmap;

    /// <summary>
    /// ͷ�� Ƥ�� ����
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
                if (temp.Message != null)
                {
                    using var skin = SKBitmap.Decode(temp.Message);
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
    /// ����Сģʽ
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
    /// ѡ��
    /// </summary>
    public void Select()
    {
        _top.Select(this);
    }

    /// <summary>
    /// ˢ��
    /// </summary>
    public void Refresh()
    {
        _top.Refresh(this);
    }

    /// <summary>
    /// ���µ�¼
    /// </summary>
    public void Relogin()
    {
        _top.Relogin(this);
    }

    /// <summary>
    /// ɾ��
    /// </summary>
    public void Remove()
    {
        _top.Remove(this);
    }

    /// <summary>
    /// �༭
    /// </summary>
    public void Edit()
    {
        _top.Edit(this);
    }

    /// <summary>
    /// ������Դ
    /// </summary>
    public void Close()
    {
        _img?.Dispose();
        _img1?.Dispose();
    }

    /// <summary>
    /// ����ͷ��
    /// </summary>
    public void ReloadHead()
    {
        OnPropertyChanged(NameReload);

        Image = null;
        _img?.Dispose();
        Task.Run(async () =>
        {
            var temp = await ImageManager.GetUserSkinAsync(_obj);
            if (temp.Message != null)
            {
                using var skin = SKBitmap.Decode(temp.Message);
                _img = ImageManager.GenHeadImage(skin);
            }
            Dispatcher.UIThread.Post(() =>
            {
                Image = _img;
            });
        });
    }
}