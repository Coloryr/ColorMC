using System.Collections.Generic;
using System.IO;
using System.Linq;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Collect;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 资源收藏
/// </summary>
public static class CollectUtils
{
    /// <summary>
    /// 收藏储存文件
    /// </summary>
    private static string s_local;

    /// <summary>
    /// 收藏
    /// </summary>
    public static CollectObj Collect { get; set; }

    /// <summary>
    /// 初始化收藏
    /// </summary>
    public static void Init()
    {
        s_local = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameCollectFile);

        Load(s_local);
    }

    /// <summary>
    /// 读取收藏
    /// </summary>
    /// <param name="file">文件</param>
    private static void Load(string file)
    {
        if (File.Exists(file))
        {
            try
            {
                using var stream = PathHelper.OpenRead(file);
                var obj1 = JsonUtils.ToObj(stream, JsonGuiType.CollectObj);
                if (obj1 != null)
                {
                    obj1.Items ??= [];
                    Collect = obj1;
                }
            }
            catch
            {

            }
        }

        if (Collect == null)
        {
            Collect = Make();
        }

        foreach (var item in Collect.Items)
        {
            item.Value.UUID = item.Key;
        }

        foreach (var item in Collect.Groups)
        {
            foreach (var item1 in item.Value.ToArray())
            {
                if (!Collect.Items.ContainsKey(item1))
                {
                    item.Value.Remove(item1);
                    continue;
                }
            }
        }

        Save();
    }

    /// <summary>
    /// 删除一个收藏
    /// </summary>
    /// <param name="obj">收藏项目</param>
    public static void RemoveItem(CollectItemObj obj)
    {
        string? uuid = null;
        foreach (var item in Collect.Items)
        {
            if (item.Value.Source == obj.Source && item.Value.Pid == obj.Pid)
            {
                uuid = item.Key;
                break;
            }
        }
        if (uuid == null)
        {
            return;
        }

        foreach (var item in Collect.Groups)
        {
            item.Value.Remove(uuid);
        }

        Collect.Items.Remove(uuid);

        if (WindowManager.CollectWindow is { } window
            && window.DataContext is CollectModel model1)
        {
            model1.Update();
        }

        Save();
    }

    /// <summary>
    /// 根据UUID删除
    /// </summary>
    /// <param name="uuid">收藏UUID</param>
    public static void RemoveItem(string uuid)
    {
        foreach (var item in Collect.Groups)
        {
            item.Value.Remove(uuid);
        }

        Collect.Items.Remove(uuid);
    }

    /// <summary>
    /// 添加一个收藏
    /// </summary>
    /// <param name="obj">收藏项目</param>
    public static void AddItem(CollectItemObj obj)
    {
        string? uuid = null;
        foreach (var item in Collect.Items)
        {
            if (item.Value.Source == obj.Source && item.Value.Pid == obj.Pid)
            {
                uuid = item.Key;
                break;
            }
        }
        if (uuid != null)
        {
            return;
        }

        uuid = obj.UUID = NewUUID();
        Collect.Items.Add(obj.UUID, obj);

        if (WindowManager.CollectWindow is { } window
            && window.DataContext is CollectModel model1)
        {
            if (!string.IsNullOrWhiteSpace(model1.Group)
                && Collect.Groups.TryGetValue(model1.Group, out var group))
            {
                group.Add(uuid);
            }

            model1.Update();
        }

        Save();
    }

    /// <summary>
    /// 新的收藏UUID
    /// </summary>
    /// <returns></returns>
    private static string NewUUID()
    {
        string uuid;
        do
        {
            uuid = FunctionUtils.NewUUID();
        } while (Collect.Items.Any(item => item.Key == uuid));

        return uuid;
    }

    /// <summary>
    /// 保存收藏储存
    /// </summary>
    private static void Save()
    {
        ConfigSave.AddItem(ConfigSaveObj.Build(GuiNames.NameCollectFile,
            s_local, Collect, JsonGuiType.CollectObj));
    }

    private static CollectObj Make()
    {
        return new CollectObj
        {
            Items = [],
            Groups = [],
            Modpack = true,
            Mod = true,
            ResourcePack = true,
            Shaderpack = true
        };
    }

    /// <summary>
    /// 添加分组
    /// </summary>
    /// <param name="group">分组名</param>
    public static void AddGroup(string group)
    {
        Collect.Groups.Add(group, []);
        Save();
    }

    /// <summary>
    /// 删除分组
    /// </summary>
    /// <param name="group"></param>
    public static void DeleteGroup(string group)
    {
        Collect.Groups.Remove(group);
        Save();
    }

    /// <summary>
    /// 清理所有收藏
    /// </summary>
    public static void Clear()
    {
        Collect.Items.Clear();
        foreach (var item in Collect.Groups)
        {
            item.Value.Clear();
        }

        Save();
    }

    /// <summary>
    /// 清理分组的收藏
    /// </summary>
    /// <param name="group"></param>
    public static void Clear(string group)
    {
        if (Collect.Groups.TryGetValue(group, out var list))
        {
            list.Clear();
        }

        Save();
    }

    /// <summary>
    /// 设置分类设置
    /// </summary>
    /// <param name="mod"></param>
    /// <param name="resourcepack"></param>
    /// <param name="shaderpack"></param>
    public static void Setting(bool Modpack, bool mod, bool resourcepack, bool shaderpack)
    {
        Collect.Modpack = Modpack;
        Collect.Mod = mod;
        Collect.ResourcePack = resourcepack;
        Collect.Shaderpack = shaderpack;

        Save();
    }

    /// <summary>
    /// 是否在收藏中
    /// </summary>
    /// <param name="type">下载源</param>
    /// <param name="pid">项目ID</param>
    /// <returns></returns>
    public static bool IsCollect(SourceItemObj obj)
    {
        return Collect.Items.Values.Any(item => item.Source == obj.Source 
        && item.FileType == obj.Type 
        && item.Pid == obj.Pid);
    }

    /// <summary>
    /// 删除指定分组的指定收藏
    /// </summary>
    /// <param name="group">分组</param>
    /// <param name="list">需要删除的</param>
    public static void RemoveItem(string group, IEnumerable<string> list)
    {
        if (Collect.Groups.TryGetValue(group, out var group1))
        {
            foreach (var item in list)
            {
                group1.Remove(item);
            }
        }

        Save();
    }

    /// <summary>
    /// 添加收藏到分组
    /// </summary>
    /// <param name="group">分组</param>
    /// <param name="list">收藏列表</param>
    public static void AddItem(string group, List<string> list)
    {
        if (Collect.Groups.TryGetValue(group, out var group1))
        {
            foreach (var item in list)
            {
                group1.Add(item);
            }
        }

        Save();
    }
}
