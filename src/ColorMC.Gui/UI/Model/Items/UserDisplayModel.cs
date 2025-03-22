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
/// �˻���ʾ
/// </summary>
public partial class UserDisplayModel(UsersModel top, LoginObj obj) : SelectItemModel
{
    public const double DefaultWidth = 350;

    public LoginObj Obj => obj;

    /// <summary>
    /// �û���
    /// </summary>
    public string Name => obj.UserName;
    /// <summary>
    /// UUID
    /// </summary>
    public string UUID => obj.UUID;
    /// <summary>
    /// ����
    /// </summary>
    public string Type => obj.AuthType.GetName();
    /// <summary>
    /// ������Ϣ
    /// </summary>
    public string Text1 => obj.Text1;
    /// <summary>
    /// ������Ϣ
    /// </summary>
    public string Text2 => obj.Text2;

    /// <summary>
    /// ���
    /// </summary>
    [ObservableProperty]
    private double _width = DefaultWidth;

    /// <summary>
    /// �˻�����
    /// </summary>
    public AuthType AuthType => obj.AuthType;

    /// <summary>
    /// ͷ��
    /// </summary>
    public Task<Bitmap?> Image => GetImage();
    /// <summary>
    /// ͷ��
    /// </summary>
    private Bitmap? _img;
    /// <summary>
    /// ��ȡͷ��
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
        top.Select(this);
    }

    /// <summary>
    /// ˢ��
    /// </summary>
    public void Refresh()
    {
        top.Refresh(this);
    }

    /// <summary>
    /// ���µ�¼
    /// </summary>
    public void Relogin()
    {
        top.Relogin(this);
    }

    /// <summary>
    /// ɾ��
    /// </summary>
    public void Remove()
    {
        top.Remove(this);
    }

    /// <summary>
    /// �༭
    /// </summary>
    public void Edit()
    {
        top.Edit(this);
    }
}