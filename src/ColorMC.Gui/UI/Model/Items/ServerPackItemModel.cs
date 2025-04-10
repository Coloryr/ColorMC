using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// ����������Ŀ
/// </summary>
public partial class ServerPackItemModel : ObservableObject
{
    /// <summary>
    /// ���ص�ַ
    /// </summary>
    [ObservableProperty]
    private string _url;
    /// <summary>
    /// ��ĿID
    /// </summary>
    [ObservableProperty]
    private string? _pID;
    /// <summary>
    /// �ļ�ID
    /// </summary>
    [ObservableProperty]
    private string? _fID;
    /// <summary>
    /// У��
    /// </summary>
    [ObservableProperty]
    public string _sha256;
    /// <summary>
    /// �Ƿ�ѡ��
    /// </summary>
    [ObservableProperty]
    public bool _check;
    /// <summary>
    /// �ļ���
    /// </summary>
    [ObservableProperty]
    public string _fileName;

    /// <summary>
    /// ����Դ����
    /// </summary>
    public string Source
    {
        get
        {
            if (SourceType is { } type)
            {
                return type.GetName();
            }
            else
            {
                return "";
            }
        }
    }

    /// <summary>
    /// ����Դ
    /// </summary>
    public SourceType? SourceType;

    /// <summary>
    /// ģ����Ϣ
    /// </summary>
    public ModDisplayModel Mod;
    /// <summary>
    /// ��Դ����Ϣ
    /// </summary>
    public ResourcepackObj Resourcepack;

    partial void OnPIDChanged(string? value)
    {
        Update();
    }

    partial void OnFIDChanged(string? value)
    {
        Update();
    }

    /// <summary>
    /// ������Ϣ
    /// </summary>
    public void Update()
    {
        if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
        {
            SourceType = null;
        }
        else
        {
            SourceType = GameDownloadHelper.TestSourceType(PID, FID);
        }
    }
}
