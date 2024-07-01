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
using SkiaSharp;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// �˻���ʾ
/// </summary>
public partial class UserDisplayModel(UsersControlModel top, LoginObj Obj) : SelectItemModel
{
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

    public AuthType AuthType => Obj.AuthType;

    public Task<Bitmap?> Image => GetImage();

    private Bitmap? _img;
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

    public void Select()
    {
        top.Select(this);
    }

    public void Refresh()
    {
        top.Refresh(this);
    }

    public void ReLogin()
    {
        top.ReLogin(this);
    }

    public void Remove()
    {
        top.Remove(this);
    }

    public void Edit()
    {
        top.Edit(this);
    }
}