using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Interactivity;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.ConfigEdit;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.ConfigEdit;


public partial class ConfigEditControl : UserControl, IUserControl
{
    private readonly ObservableCollection<string> List = new();
    private readonly List<string> Items = new();
    private readonly GameSettingObj Obj;
    private readonly WorldObj? World;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public ConfigEditControl()
    {
        InitializeComponent();

        ComboBox1.ItemsSource = List;
        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;

        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;

        TextEditor1.Options.ShowBoxForControlCharacters = true;
        TextEditor1.TextArea.IndentationStrategy =
            new CSharpIndentationStrategy(TextEditor1.Options);

        TextBox1.PropertyChanged += TextBox1_TextInput;
    }

    public void Opened()
    {
        if (World == null)
        {
            Window.SetTitle(string.Format(App.GetLanguage("ConfigEditWindow.Title"), Obj?.Name));
        }
        else
        {
            Window.SetTitle(string.Format(App.GetLanguage("ConfigEditWindow.Title1"), Obj?.Name, World?.LevelName));
        }
    }

    public ConfigEditControl(WorldObj world) : this()
    {
        World = world;
        Obj = world.Game;
    }

    public ConfigEditControl(GameSettingObj obj) : this()
    {
        Obj = obj;
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void TextBox1_TextInput(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        var property = e.Property.Name;
        if (property == "Text")
        {
            Load1();
        }
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        if (ComboBox1.SelectedItem is not string item)
            return;

        GameBinding.SaveConfigFile(Obj, item, TextEditor1.Document.Text);
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        if (ComboBox1.SelectedItem is not string item)
            return;

        var dir = Obj.GetGamePath();
        BaseBinding.OpFile(Path.GetFullPath(dir + "/" + item));
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Load1();
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ComboBox1.SelectedItem is not string item)
            return;

        if (item.EndsWith(".dat"))
        {
            NbtViewer.IsVisible = true;
            TextEditor1.IsVisible = false;

            NbtBase? nbt;
            if (World != null)
            {
                nbt = GameBinding.ReadNbt(World, item);
            }
            else
            {
                nbt = GameBinding.ReadNbt(Obj, item);
            }

            if (nbt is not NbtCompound nbt1)
            {
                return;
            }

            NbtPageViewModel model = new(nbt1);
            NbtViewer.Source = model.Source;
        }
        else
        {
            NbtViewer.IsVisible = false;
            TextEditor1.IsVisible = true;
            string text;
            if (World != null)
            {
                text = GameBinding.ReadConfigFile(World, item);
            }
            else
            {
                text = GameBinding.ReadConfigFile(Obj, item);
            }

            TextEditor1.Document = new AvaloniaEdit.Document.TextDocument(text);
        }
    }

    private void Load()
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
        var fil = TextBox1.Text;
        List.Clear();
        if (string.IsNullOrWhiteSpace(fil))
        {
            List.AddRange(Items);
        }
        else
        {
            var list = from item in Items
                       where item.Contains(fil)
                       select item;
            List.AddRange(list);
        }

        if (List.Count != 0)
        {
            ComboBox1.SelectedIndex = 0;
        }
        else
        {
            TextEditor1.Text = "";
        }
    }

    public void Update()
    {
        if (Obj == null)
            return;

        TextBox1.Text = "";
        Load();
    }

    public void Closed()
    {
        string key;
        if (World != null)
        {
            key = Obj.UUID + ":" + World.LevelName;
        }
        else
        {
            key = Obj.UUID;
        }
        App.ConfigEditWindows.Remove(key);
    }
}
