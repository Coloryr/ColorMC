using System.Collections.ObjectModel;
using System.Linq;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel
{
    /// <summary>
    /// 文件下载列表
    /// </summary>
    public ObservableCollection<ServerPackConfigModel> FileList { get; init; } = [];
    /// <summary>
    /// 文件列表
    /// </summary>
    public ObservableCollection<string> NameList { get; init; } = [];
    /// <summary>
    /// 打包方式
    /// </summary>
    public string[] FuntionList { get; init; } = LanguageBinding.GetFuntionList();

    /// <summary>
    /// 选中的文件
    /// </summary>
    [ObservableProperty]
    private ServerPackConfigModel _fileItem;
    /// <summary>
    /// 选中的打包方式
    /// </summary>
    [ObservableProperty]
    private int _funtion;
    /// <summary>
    /// 文件夹
    /// </summary>
    [ObservableProperty]
    private string? _group;

    /// <summary>
    /// 添加文件
    /// </summary>
    [RelayCommand]
    public void AddFile()
    {
        if (string.IsNullOrEmpty(Group))
        {
            return;
        }
        string local = Obj.Game.GetGamePath() + "/" + Group;
        local = local.Replace('\\', '/');
        Obj.Config ??= [];
        if (local.EndsWith('/'))
        {
            if (Funtion == 0)
            {
                var item = new ConfigPackObj()
                {
                    Group = Group,
                    IsZip = true,
                    IsDir = true
                };

                Obj.Config.Add(item);
            }
            else
            {
                var item = new ConfigPackObj()
                {
                    Group = Group,
                    IsZip = false,
                    IsDir = true
                };

                Obj.Config.Add(item);
            }
        }
        else
        {
            var item = new ConfigPackObj()
            {
                Group = Group,
                IsZip = false,
                IsDir = false
            };

            Obj.Config.Add(item);
        }
        LoadFile();
    }

    /// <summary>
    /// 加载文件列表
    /// </summary>
    public void LoadFile()
    {
        FileList.Clear();
        NameList.Clear();
        var mods = GameBinding.GetAllTopConfig(Obj.Game);

        Obj.Config?.RemoveAll(a => mods.Find(b => a.Group == b) == null);

        mods.ForEach(item =>
        {
            var item1 = Obj.Config?.FirstOrDefault(a => a.Group == item);
            if (item1 != null)
            {
                FileList.Add(new(item1));
            }
            else
            {
                NameList.Add(item);
            }
        });

        GameBinding.SaveServerPack(Obj);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="obj">服务器包文件</param>
    public void DeleteFile(ServerPackConfigModel obj)
    {
        Obj.Config?.Remove(obj.Obj);
        LoadFile();
    }
}
