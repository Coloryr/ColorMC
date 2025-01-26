using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Collect;

public partial class CollectModel : TopModel, ICollectWindow
{
    public ObservableCollection<CollectItemModel> CollectList { get; init; } = [];
    public ObservableCollection<string> Groups { get; init; } = [];
    
    private List<CollectItemModel> _list = [];

    [ObservableProperty]
    private bool _modpack;
    [ObservableProperty]
    private bool _mod;
    [ObservableProperty]
    private bool _resourcepack;
    [ObservableProperty]
    private bool _shaderpack;

    [ObservableProperty]
    private bool _groupDelete;
    [ObservableProperty]
    private bool _emptyDisplay;

    [ObservableProperty]
    private string _group;

    private CollectItemModel? _select;

    private readonly string _useName;

    public CollectModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "CollectModel";

        var conf = CollectUtils.Collect;

        _modpack = conf.ModPack;
        _mod = conf.Mod;
        _resourcepack = conf.ResourcePack;
        _shaderpack = conf.Shaderpack;

        Groups.Add("");

        foreach (var item in conf.Groups)
        {
            Groups.Add(item.Key);
        }

        LoadItems();
        Load();
    }

    partial void OnModpackChanged(bool value)
    {
        CollectUtils.Setting(Modpack, Mod, Resourcepack, Shaderpack);

        Load();
    }

    partial void OnModChanged(bool value)
    {
        CollectUtils.Setting(Modpack, Mod, Resourcepack, Shaderpack);

        Load();
    }

    partial void OnResourcepackChanged(bool value)
    {
        CollectUtils.Setting(Modpack, Mod, Resourcepack, Shaderpack);

        Load();
    }

    partial void OnShaderpackChanged(bool value)
    {
        CollectUtils.Setting(Modpack, Mod, Resourcepack, Shaderpack);

        Load();
    }

    partial void OnGroupChanged(string value)
    {
        GroupDelete = !string.IsNullOrWhiteSpace(value);

        Load();
    }

    [RelayCommand]
    public async Task AddGroup()
    {
        var (Cancel, Text) = await Model.ShowInputOne(App.Lang("CollectWindow.Info1"), false);
        if (Cancel || string.IsNullOrWhiteSpace(Text))
        {
            return;
        }
        if (CollectUtils.Collect.Groups.ContainsKey(Text))
        {
            Model.Show(App.Lang("CollectWindow.Error1"));
            return;
        }
        CollectUtils.AddGroup(Text);
        Groups.Add(Text);
        Group = Text;
    }

    [RelayCommand]
    public async Task DeleteGroup()
    {
        var res = await Model.ShowWait(App.Lang("CollectWindow.Info4"));
        if (!res)
        {
            return;
        }
        CollectUtils.DeleteGroup(Group);
        Groups.Remove(Group);
    }

    [RelayCommand]
    public async Task ClearGroup()
    {
        if (string.IsNullOrWhiteSpace(Group))
        {
            var res = await Model.ShowWait(App.Lang("CollectWindow.Info2"));
            if (res == true)
            {
                CollectUtils.Clear();
                Close();
            }
        }
        else
        {
            var res = await Model.ShowWait(App.Lang("CollectWindow.Info3"));
            if (res == true)
            {
                CollectUtils.Clear(Group);
                Load();
            }
        }
    }

    private void Load()
    {
        CollectList.Clear();

        if (string.IsNullOrWhiteSpace(Group))
        {
            foreach (var item in _list)
            {
                if (item.Obj.FileType == FileType.ModPack && Modpack)
                {
                    CollectList.Add(item);
                }
                else if (item.Obj.FileType == FileType.Mod && Mod)
                {
                    CollectList.Add(item);
                }
                else if (item.Obj.FileType == FileType.Resourcepack && Resourcepack)
                {
                    CollectList.Add(item);
                }
                else if (item.Obj.FileType == FileType.Shaderpack && Shaderpack)
                {
                    CollectList.Add(item);
                }
            }
        }

        EmptyDisplay = CollectList.Count == 0;
    }


    private void LoadItems()
    {
        foreach (var item in CollectUtils.Collect.Items)
        {
            var model = new CollectItemModel(item.Value);
            model.Add = this;
            _list.Add(model);
        }
    }

    public override void Close()
    {
        CollectList.Clear();
        foreach (var item in _list)
        {
            item.Close();
        }

        _list.Clear();
    }

    public void Update()
    {
        Close();
        LoadItems();
        Load();
    }

    public void SetSelect(CollectItemModel item)
    {
        if (_select != null)
        {
            _select.IsSelect = false;
        }
        _select = item;
        item.IsSelect = true;
    }

    public void Install(CollectItemModel item)
    {
        
    }
}
