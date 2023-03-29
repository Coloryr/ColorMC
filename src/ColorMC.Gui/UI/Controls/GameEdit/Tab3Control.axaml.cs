using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.TextMate;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TextMateSharp.Grammars;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab3Control : UserControl
{
    private readonly ObservableCollection<string> List = new();
    private readonly List<string> Items = new();
    private GameSettingObj Obj;
    private TextMate.Installation textMateInstallation;
    private RegistryOptions registryOptions;
    public Tab3Control()
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

        registryOptions = new RegistryOptions(ThemeName.LightPlus);
        textMateInstallation = TextEditor1.InstallTextMate(registryOptions);

        TextBox1.PropertyChanged += TextBox1_TextInput;
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

        var text = GameBinding.ReadConfigFile(Obj, item);
        var ex = item[item.LastIndexOf('.')..];

        TextEditor1.Document = new AvaloniaEdit.Document.TextDocument(text);
        EditGa(ex);
    }

    public void EditGa(string name)
    {
        if (name == ".json5")
        {
            name = ".json";
        }
        var item = registryOptions.GetLanguageByExtension(name);
        if (item == null)
        {
            textMateInstallation.SetGrammar(null);
            return;
        }
        var item1 = registryOptions.GetScopeByLanguageId(item.Id);
        if (item1 == null)
            return;
        textMateInstallation.SetGrammar(item1);
    }

    private void Load()
    {
        Items.Clear();
        var list = GameBinding.GetAllConfig(Obj);
        Items.AddRange(list);
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

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void Update()
    {
        if (Obj == null)
            return;

        TextBox1.Text = "";
        Load();
    }
}
