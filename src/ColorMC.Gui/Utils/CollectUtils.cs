using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Collect;
using Newtonsoft.Json;

namespace ColorMC.Gui.Utils;

public static class CollectUtils
{
    private static string s_local;

    /// <summary>
    /// 收藏
    /// </summary>
    public static CollectObj Collect { get; set; }

    private static readonly Dictionary<string, int> s_itemUse = [];

    public static void Init(string dir)
    {
        s_local = dir + "collect.json";

        Load(s_local);
    }

    public static void Load(string file)
    {
        if (File.Exists(file))
        {
            try
            {
                var data = PathHelper.ReadText(file);
                var obj1 = JsonConvert.DeserializeObject<CollectObj>(data!);
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
                if (s_itemUse.TryGetValue(item1, out int value))
                {
                    s_itemUse[item1] = ++value;
                }
                else
                {
                    s_itemUse[item1] = 1;
                }
            }
        }

        Save();
    }

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

        if (WindowManager.CollectWindow is { } window
            && window.DataContext is CollectModel model1)
        {
            if (string.IsNullOrWhiteSpace(model1.Group))
            {
                Collect.Items.Remove(uuid);
            }
            else if (Collect.Groups.TryGetValue(model1.Group, out var gourp))
            {
                gourp.Remove(uuid);
                if (s_itemUse.TryGetValue(uuid, out var use))
                {
                    if (use == 1)
                    {
                        Collect.Items.Remove(uuid);
                    }
                    else
                    {
                        s_itemUse[uuid] = use - 1;
                    }
                }
            }

            model1.Update();
        }
        else
        {
            Collect.Items.Remove(uuid);
        }

        Save();
    }

    public static void RemoveItem(string uuid)
    {
        Collect.Items.Remove(uuid);

        foreach (var item in Collect.Groups)
        {
            item.Value.Remove(uuid);
        }

        Save();
    }

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
                && Collect.Groups.TryGetValue(model1.Group, out var gourp))
            {
                gourp.Add(uuid);
                if (s_itemUse.TryGetValue(uuid, out int value))
                {
                    s_itemUse[uuid] = ++value;
                }
                else
                {
                    s_itemUse[uuid] = 1;
                }
            }

            model1.Update();
        }

        Save();
    }

    private static string NewUUID()
    {
        string uuid;
        do
        {
            uuid = FuntionUtils.NewUUID();
        } while (Collect.Items.Any(item => item.Key == uuid));

        return uuid;
    }

    public static void Save()
    {
        ConfigSave.AddItem(new()
        {
            File = s_local,
            Name = "Collect",
            Obj = Collect
        });
    }

    private static CollectObj Make()
    {
        return new()
        {
            Items = [],
            Groups = [],
            Mod = true,
            ResourcePack = true,
            Shaderpack = true
        };
    }

    public static void AddGroup(string group)
    {
        Collect.Groups.Add(group, []);
        Save();
    }

    private static void RemoveGroupItem(string group)
    {
        if (Collect.Groups.TryGetValue(group, out var list))
        {
            foreach (var item in list)
            {
                if (s_itemUse.TryGetValue(item, out var use))
                {
                    if (use == 1)
                    {
                        Collect.Items.Remove(item);
                    }
                    else
                    {
                        s_itemUse[item] = use - 1;
                    }
                }
            }
        }
    }

    public static void DeleteGroup(string group)
    {
        RemoveGroupItem(group);

        Collect.Groups.Remove(group);

        Save();
    }

    public static void Clear()
    {
        Collect.Items.Clear();
        foreach (var item in Collect.Groups)
        {
            item.Value.Clear();
        }

        Save();
    }

    public static void Clear(string group)
    {
        RemoveGroupItem(group);

        if (Collect.Groups.TryGetValue(group, out var list))
        {
            list.Clear();
        }

        Save();
    }

    public static void Setting(bool mod, bool resourcepack, bool shaderpack)
    {
        Collect.Mod = mod;
        Collect.ResourcePack = resourcepack;
        Collect.Shaderpack = shaderpack;

        Save();
    }

    public static bool IsStar(SourceType type, string pid)
    {
        return Collect.Items.Values.Any(item => item.Source == type && item.Pid == pid);
    }
}
