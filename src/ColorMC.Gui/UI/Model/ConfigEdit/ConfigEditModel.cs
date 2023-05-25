using Avalonia.Controls;
using Avalonia.Controls.Selection;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.ConfigEdit;

public partial class ConfigEditModel : ObservableObject
{
    private readonly IUserControl Con;
    private readonly List<string> Items = new();

    public GameSettingObj Obj { get; init; }
    public WorldObj? World { get; init; }

    private readonly Semaphore semaphore = new(0, 2);
    public bool cancel;

    public ObservableCollection<string> FileList { get; init; } = new();

    public List<string> TypeSource { get; init; } = new()
    {
        "NbtEnd",
        "NbtByte",
        "NbtShort",
        "NbtInt",
        "NbtLong",
        "NbtFloat",
        "NbtDouble",
        "NbtByteArray",
        "NbtString",
        "NbtList",
        "NbtCompound",
        "NbtIntArray",
        "NbtLongArray",
    };

    private NbtPageViewModel nbtView;

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

    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string title1;
    [ObservableProperty]
    private bool displayAdd;
    [ObservableProperty]
    private bool displayType;
    [ObservableProperty]
    private string key;
    [ObservableProperty]
    private int type;
    

    public ConfigEditModel(IUserControl con, GameSettingObj obj, WorldObj? world)
    {
        Con = con;
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

            nbtView = new(nbt1);
            Source = nbtView.Source;
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
        var info = new FileInfo(File);
        if (info.Extension is ".dat" or ".dat_old")
        {
            if (World != null)
            {
                GameBinding.SaveNbtFile(World, File, nbtView.Nbt);
            }
            else
            {
                GameBinding.SaveNbtFile(Obj, File, nbtView.Nbt);
            }
        }
        else
        {
            if (World != null)
            {
                GameBinding.SaveConfigFile(World, File, Text?.Text);
            }
            else
            {
                GameBinding.SaveConfigFile(Obj, File, Text?.Text);
            }
        }

        var window = Con.Window;
        window.NotifyInfo.Show(App.GetLanguage("Gui.Info10"));
    }

    [RelayCommand]
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

    [RelayCommand]
    public void AddConfirm()
    {
        cancel = false;
        semaphore.Release();
    }

    [RelayCommand]
    public void AddCancel()
    {
        cancel = true;
        semaphore.Release();
    }

    public void Pressed(Control con)
    {
        var item = Source.Selection;
        if (item != null)
        {
            _ = new ConfigFlyout1(con, item, this);
        }
    }

    public async void AddItem(NbtNodeModel model)
    {
        if (model.NbtType == NbtType.NbtCompound)
        {
            var list = (model.Nbt as NbtCompound)!;
            var window = Con.Window;
            Key = "";
            Type = 0;
            DisplayType = true;
            Title = App.GetLanguage("ConfigEditWindow.Info2");
            Title1 = App.GetLanguage("ConfigEditWindow.Info3");
            DisplayAdd = true;
            await Task.Run(semaphore.WaitOne);
            DisplayAdd = false;
            if (cancel)
                return;
            if (string.IsNullOrWhiteSpace(Key))
            {
                window.OkInfo.Show(App.GetLanguage("ConfigEditWindow.Error1"));
                return;
            }
            else if (list.HaveKey(Key))
            {
                window.OkInfo.Show(App.GetLanguage("ConfigEditWindow.Error2"));
                return;
            }

            model.Add(Key, (NbtType)Type);
        }
        else if (model.NbtType == NbtType.NbtList)
        {
            model.Add("", NbtType.NbtEnd);
        }
    }

    public async void Delete(NbtNodeModel model)
    {
        if (model.Top == null)
            return;

        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(App.GetLanguage("ConfigEditWindow.Info1"));
        if (!res)
            return;

        model.Top.Remove(model);
    }

    public async void Delete(IReadOnlyList<NbtNodeModel>? list)
    {
        if (list == null)
            return;

        var list1 = new List<NbtNodeModel>(list);

        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(App.GetLanguage("ConfigEditWindow.Info1"));
        if (!res)
            return;

        foreach (var item in list1)
        {
            item.Top?.Remove(item);
        }
    }

    public async void SetKey(NbtNodeModel model)
    {
        if (model.Top == null)
            return;

        var list = (model.Top.Nbt as NbtCompound)!;
        var window = Con.Window;
        Key = model.Key!;
        DisplayType = false;
        Title = App.GetLanguage("ConfigEditWindow.Info5");
        Title1 = App.GetLanguage("ConfigEditWindow.Info3");
        DisplayAdd = true;
        await Task.Run(semaphore.WaitOne);
        DisplayAdd = false;
        if (cancel)
            return;
        if (string.IsNullOrWhiteSpace(Key))
        {
            window.OkInfo.Show(App.GetLanguage("ConfigEditWindow.Error1"));
            return;
        }
        else if (Key == model.Key)
        {
            return;
        }
        else if (list.HaveKey(Key))
        {
            window.OkInfo.Show(App.GetLanguage("ConfigEditWindow.Error2"));
            return;
        }

        model.EditKey(model.Key!, Key);
    }

    public async void SetValue(NbtNodeModel model)
    {
        var list = (model.Nbt as NbtCompound)!;
        var window = Con.Window;
        Key = model.Nbt.Value.ToString();
        DisplayType = false;
        Title = App.GetLanguage("ConfigEditWindow.Info6");
        Title1 = App.GetLanguage("ConfigEditWindow.Info4");
        DisplayAdd = true;
        await Task.Run(semaphore.WaitOne);
        DisplayAdd = false;
        if (cancel)
            return;
        if (string.IsNullOrWhiteSpace(Key))
        {
            window.OkInfo.Show(App.GetLanguage("ConfigEditWindow.Error1"));
            return;
        }

        try
        {
            model.SetValue(Key);
        }
        catch
        {
            window.OkInfo.Show(App.GetLanguage("ConfigEditWindow.Error3"));
        }
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
