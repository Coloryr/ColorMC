using Avalonia.Controls;
using AvaloniaEdit.Indentation.CSharp;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.ConfigEdit;
using ColorMC.Gui.UI.Windows;
using Avalonia.Input;

namespace ColorMC.Gui.UI.Controls.ConfigEdit;

public partial class ConfigEditControl : UserControl, IUserControl
{
    private readonly ConfigEditModel model;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public ConfigEditControl()
    {
        InitializeComponent();

        NbtViewer.PointerPressed += NbtViewer_PointerPressed;

        TextEditor1.Options.ShowBoxForControlCharacters = true;
        TextEditor1.TextArea.IndentationStrategy =
            new CSharpIndentationStrategy(TextEditor1.Options);
    }

    private void NbtViewer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            model.Pressed(this);
        }
    }

    public void Opened()
    {
        if (model.World == null)
        {
            Window.SetTitle(string.Format(App.GetLanguage("ConfigEditWindow.Title"),
                model.Obj?.Name));
        }
        else
        {
            Window.SetTitle(string.Format(App.GetLanguage("ConfigEditWindow.Title1"),
                model.Obj?.Name, model.World?.LevelName));
        }
    }

    public ConfigEditControl(WorldObj world) : this()
    {
        model = new(world.Game, world);
        DataContext = model;
    }

    public ConfigEditControl(GameSettingObj obj) : this()
    {
        model = new(obj, null);
        DataContext = model;
    }

    public void Update()
    {
        model.Load();
    }

    public void Closed()
    {
        string key;
        if (model.World != null)
        {
            key = model.Obj.UUID + ":" + model.World.LevelName;
        }
        else
        {
            key = model.Obj.UUID;
        }
        App.ConfigEditWindows.Remove(key);
    }
}
