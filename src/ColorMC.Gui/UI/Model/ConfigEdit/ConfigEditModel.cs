using Avalonia.Controls;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Model.ConfigEdit;

public partial class ConfigEditModel : ObservableObject
{
    private readonly List<string> Items = new();

    public GameSettingObj Obj { get; init; }
    public WorldObj? World { get; init; }


    public ObservableCollection<string> FileList { get; init; } = new();

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<NbtNodeModel> source;

    [ObservableProperty]
    private int select = -1;
    [ObservableProperty]
    private string file;
    [ObservableProperty]
    private bool nbtEnable;
    [ObservableProperty]
    private string? name;
    [ObservableProperty]
    private TextDocument text;

    public ConfigEditModel(GameSettingObj obj, WorldObj? world)
    {
        Obj = obj;
        World = world;

        text = new();
    }

    partial void OnNameChanged(string? value)
    {
        Load1();
    }

    partial void OnFileChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var info = new FileInfo(value);
        if (info.Extension is ".dat" or ".dat_old")
        {
            NbtEnable = true;

            NbtBase? nbt;
            if (World != null)
            {
                nbt = GameBinding.ReadNbt(World, value);
            }
            else
            {
                nbt = GameBinding.ReadNbt(Obj, value);
            }

            if (nbt is not NbtCompound nbt1)
            {
                return;
            }

            NbtPageViewModel model = new(nbt1);
            Source = model.Source;
        }
        else
        {
            NbtEnable = false;

            string text;
            if (World != null)
            {
                text = GameBinding.ReadConfigFile(World, value);
            }
            else
            {
                text = GameBinding.ReadConfigFile(Obj, value);
            }

            Text = new(text);
        }
    }

    [RelayCommand]
    public void Open()
    {
        var dir = Obj.GetGamePath();
        BaseBinding.OpFile(Path.GetFullPath(dir + "/" + File));
    }

    [RelayCommand]
    public void Save()
    {
        GameBinding.SaveConfigFile(Obj, File, Text?.Text);
    }

    [RelayCommand]
    public void Reload()
    {
        Load();
    }

    public void Load()
    {
        if (Obj == null)
            return;

        Items.Clear();
        if (World != null)
        {
            var list = GameBinding.GetAllConfig(World);
            Items.AddRange(list);
        }
        else
        {
            var list = GameBinding.GetAllConfig(Obj);
            Items.AddRange(list);
        }
        Load1();
    }

    private void Load1()
    {
        FileList.Clear();
        if (string.IsNullOrWhiteSpace(Name))
        {
            FileList.AddRange(Items);
        }
        else
        {
            var list = from item in Items
                       where item.Contains(Name)
                       select item;
            FileList.AddRange(list);
        }

        if (FileList.Count != 0)
        {
            Select = 0;
        }
        else
        {
            Text.Text = "";
        }
    }
}
