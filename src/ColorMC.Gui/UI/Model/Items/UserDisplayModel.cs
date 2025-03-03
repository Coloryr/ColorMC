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
using System;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// �˻���ʾ
/// </summary>
public partial class UserDisplayModel(UsersControlModel top, LoginObj Obj) : SelectItemModel
{
    public const double DefaultWidth = 350;

    /// <summary>
    /// �û���
    /// </summary>
    public string Name => Obj.UserName;
    /// <summary>
    /// UUID
    /// </summary>
    public string UUID => Obj.UUID;
    /// <summary>
    /// ����
    /// </summary>
    public string Type => Obj.AuthType.GetName();
    /// <summary>
    /// ������Ϣ
    /// </summary>
    public string Text1 => Obj.Text1;
    /// <summary>
    /// ������Ϣ
    /// </summary>
    public string Text2 => Obj.Text2;

    /// <summary>
    /// ���
    /// </summary>
    [ObservableProperty]
    private double _width = DefaultWidth;

    /// <summary>
    /// �˻�����
    /// </summary>
    public AuthType AuthType => Obj.AuthType;

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
                var temp = await PlayerSkinAPI.DownloadSkin(Obj);
                if (temp.Item1)
                {
                    var file = AssetsPath.GetSkinFile(Obj);
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