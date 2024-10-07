using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameConfigEdit;

namespace ColorMC.Gui.UI.Controls.GameConfigEdit;

public partial class GameConfigEditControl : BaseUserControl
{
    private readonly WorldObj _world;
    private readonly GameSettingObj _obj;

    //private readonly TextMate.Installation textMateInstallation;
    //private readonly RegistryOptions registryOptions;

    public GameConfigEditControl()
    {
        InitializeComponent();

        NbtViewer.PointerPressed += NbtViewer_PointerPressed;
        NbtViewer.KeyDown += NbtViewer_KeyDown;

        TextEditor1.KeyDown += NbtViewer_KeyDown;
        TextEditor1.TextArea.TextEntered += TextEditor1_TextInput;


        //registryOptions = new RegistryOptions(ThemeManager.NowTheme == PlatformThemeVariant.Light ? ThemeName.LightPlus : ThemeName.DarkPlus);
        //textMateInstallation = TextEditor1.InstallTextMate(registryOptions);
    }

    private void TextEditor1_TextInput(object? sender, TextInputEventArgs e)
    {
        if (DataContext is GameConfigEditModel model)
        {
            model.Edit();
        }
    }

    public GameConfigEditControl(WorldObj world) : this()
    {
        _world = world;
        _obj = world.Game;

        Title = string.Format(App.Lang("ConfigEditWindow.Title1"),
                world.Game.Name, world.LevelName);

        UseName = (ToString() ?? "GameConfigEditControl")
            + ":" + world.Game.UUID + ":" + world.LevelName;
    }

    public GameConfigEditControl(GameSettingObj obj) : this()
    {
        _obj = obj;

        UseName = (ToString() ?? "GameConfigEditControl") + ":" + obj.UUID;

        Title = string.Format(App.Lang("ConfigEditWindow.Title"),
                obj.Name);
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "TurnTo")
        {
            NbtViewer.Scroll!.Offset = new(0, (DataContext as GameConfigEditModel)!.TurnTo * 25);
        }
        //else if (e.PropertyName == nameof(GameConfigEditModel.File))
        //{
        //    var model = (DataContext as GameConfigEditModel)!;
        //    var info = new FileInfo(model.File);
        //    var lang = registryOptions.GetLanguageByExtension(info.Extension);
        //    if (lang == null)
        //    {
        //        return;
        //    }
        //    var temp = registryOptions.GetScopeByLanguageId(lang.Id);
        //    textMateInstallation.SetGrammar(temp);
        //}
    }

    private void NbtViewer_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.S && e.KeyModifiers == KeyModifiers.Control)
        {
            (DataContext as GameConfigEditModel)!.Save();
        }
    }

    private void NbtViewer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout1((sender as Control)!);
        }
        else
        {
            LongPressed.Pressed(() => Flyout1((sender as Control)!));
        }
    }

    private void Flyout1(Control control)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var model = (DataContext as GameConfigEditModel)!;
            var item = model.Source.Selection;
            if (item != null)
            {
                _ = new ConfigFlyout1(control, item, model);
            }
        });
    }

    public override void Opened()
    {
        Window.SetTitle(Title);

        var model = (DataContext as GameConfigEditModel)!;
        model.Load();
    }

    public override void Closed()
    {
        string key;
        var model = (DataContext as GameConfigEditModel)!;
        if (model.World != null)
        {
            key = model.World.Game.UUID + ":" + model.World.LevelName;
        }
        else
        {
            key = model.Obj.UUID;
        }
        WindowManager.ConfigEditWindows.Remove(key);
    }

    public override TopModel GenModel(BaseModel model)
    {
        var amodel = new GameConfigEditModel(model, _obj, _world);
        amodel.PropertyChanged += Model_PropertyChanged;

        var icon = ImageManager.GetGameIcon(_obj);
        if (icon != null)
        {
            model.Icon = icon;
        }

        return amodel;
    }

    public override Bitmap GetIcon()
    {
        var icon = ImageManager.GetGameIcon(_obj);
        return icon ?? ImageManager.GameIcon;
    }
}
