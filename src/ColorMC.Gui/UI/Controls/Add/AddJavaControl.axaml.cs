using System.Threading.Tasks;
using Avalonia.Input;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Add;

/// <summary>
/// Java���ش���
/// </summary>
public partial class AddJavaControl : BaseUserControl
{
    /// <summary>
    /// ��Ҫ���ص�JAVA���汾
    /// </summary>
    public int NeedJava { get; set; }

    public AddJavaControl() : base(WindowManager.GetUseName<AddJavaControl>())
    {
        InitializeComponent();

        Title = App.Lang("AddJavaWindow.Title");

        JavaFiles.DoubleTapped += JavaFiles_DoubleTapped;
    }

    public override Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            (DataContext as AddJavaControlModel)!.Load();

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public override void Opened()
    {
        (DataContext as AddJavaControlModel)!.TypeIndex = 0;
    }

    public override void Closed()
    {
        WindowManager.AddJavaWindow = null;
    }

    protected override TopModel GenModel(BaseModel model)
    {
        return new AddJavaControlModel(model, NeedJava);
    }

    private void JavaFiles_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (JavaFiles.SelectedItem is not JavaDownloadModel obj)
            return;

        (DataContext as AddJavaControlModel)!.Install(obj);
    }
}
