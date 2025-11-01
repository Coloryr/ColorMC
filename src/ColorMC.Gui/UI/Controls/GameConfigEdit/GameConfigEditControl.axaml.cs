using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameConfigEdit;

namespace ColorMC.Gui.UI.Controls.GameConfigEdit;

/// <summary>
/// 游戏实例配置修改窗口
/// </summary>
public partial class GameConfigEditControl : BaseUserControl
{
    /// <summary>
    /// 存档
    /// </summary>
    private readonly SaveObj _world;
    /// <summary>
    /// 游戏实例
    /// </summary>
    private readonly GameSettingObj _obj;

    //private readonly TextMate.Installation textMateInstallation;
    //private readonly RegistryOptions registryOptions;

    //显示用

    public GameConfigEditControl() : base(WindowManager.GetUseName<GameConfigEditControl>())
    {
        InitializeComponent();
    }

    public GameConfigEditControl(SaveObj world) : base(WindowManager.GetUseName<GameConfigEditControl>(world.Game) + ":" + world.LevelName)
    {
        InitializeComponent();

        _world = world;
        _obj = world.Game;

        Title = string.Format(LanguageUtils.Get("ConfigEditWindow.Title1"),
                world.Game.Name, world.LevelName);

        Hook();
    }

    public GameConfigEditControl(GameSettingObj obj) : base(WindowManager.GetUseName<GameConfigEditControl>(obj))
    {
        InitializeComponent();

        _obj = obj;

        Title = string.Format(LanguageUtils.Get("ConfigEditWindow.Title"), obj.Name);

        Hook();
    }

    private void Hook()
    {
        NbtViewer.PointerPressed += NbtViewer_PointerPressed;
        NbtViewer.KeyDown += NbtViewer_KeyDown;

        TextEditor1.KeyDown += NbtViewer_KeyDown;
        TextEditor1.TextArea.TextEntered += TextEditor1_TextInput;

        EventManager.GameIconChange += EventManager_GameIconChange;
        EventManager.GameNameChange += EventManager_GameNameChange;
        EventManager.GameDelete += EventManager_GameDelete;

        //registryOptions = new RegistryOptions(ThemeManager.NowTheme == PlatformThemeVariant.Light ? ThemeName.LightPlus : ThemeName.DarkPlus);
        //textMateInstallation = TextEditor1.InstallTextMate(registryOptions);
    }

    private void EventManager_GameDelete(object? sender, string uuid)
    {
        if (uuid != _obj.UUID)
        {
            return;
        }

        Window?.Close();
    }

    private void EventManager_GameNameChange(object? sender, string uuid)
    {
        if (uuid != _obj.UUID)
        {
            return;
        }

        if (_world == null)
        {
            Title = string.Format(LanguageUtils.Get("ConfigEditWindow.Title"),
                    _obj.Name);
        }
        else
        {
            Title = string.Format(LanguageUtils.Get("ConfigEditWindow.Title1"),
                    _world.Game.Name, _world.LevelName);
        }
    }

    private void EventManager_GameIconChange(object? sender, string uuid)
    {
        if (uuid != _obj.UUID)
        {
            return;
        }

        ReloadIcon();
    }

    private void TextEditor1_TextInput(object? sender, TextInputEventArgs e)
    {
        if (DataContext is GameConfigEditModel model)
        {
            model.Edit();
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameConfigEditModel.TurnTo))
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
    }

    private void Flyout1(Control control)
    {
        if (DataContext is not GameConfigEditModel model)
        {
            return;
        }
        var item = model.Source.Selection;
        if (item != null)
        {
            ConfigFlyout1.Show(control, item, model);
        }
    }

    public override void Opened()
    {
        var model = (DataContext as GameConfigEditModel)!;
        model.Load();
    }

    public override void Closed()
    {
        string key;
        var model = (DataContext as GameConfigEditModel)!;
        if (model.World != null)
        {
            key = model.World.Game.UUID + ":" + model.World.Local;
        }
        else
        {
            key = model.Obj.UUID;
        }

        EventManager.GameIconChange -= EventManager_GameIconChange;
        EventManager.GameNameChange -= EventManager_GameNameChange;
        EventManager.GameDelete -= EventManager_GameDelete;

        WindowManager.GameConfigEditWindows.Remove(key);
    }

    protected override TopModel GenModel(BaseModel model)
    {
        var amodel = new GameConfigEditModel(model, _obj, _world);
        amodel.PropertyChanged += Model_PropertyChanged;
        return amodel;
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GetGameIcon(_obj) ?? ImageManager.GameIcon;
    }
}
