using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;
using DynamicData;
using ColorMC.Gui.UI.Windows;
using System.Collections.ObjectModel;
using ColorMC.Gui.Utils.LaunchSetting;

namespace ColorMC.Gui.UI.Controls.Hello;

public partial class Tab2Control : UserControl
{
    private HelloWindow Window;

    private ObservableCollection<JavaInfoObj> List = new();
    public Tab2Control()
    {
        InitializeComponent();
        List_Java.Items = List;

        Button_SelectFile.Click += Button_SelectFile_Click;
        Button_Add.Click += Button_Add_Click;
        Button_Refash.Click += Button_Refash_Click;
        Button_Next.Click += Button_Next_Click;
        Button_Delete.Click += Button_Delete_Click;

        Load();
    }

    private void Button_Delete_Click(object? sender, RoutedEventArgs e)
    {
        var item = List_Java.SelectedItem as JavaInfoObj;
        if (item == null)
            return;

        JavaBinding.RemoveJava(item.Name);
        Load();
    }

    private void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        string name = TextBox_Name.Text;
        string local = TextBox_Local.Text;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(local))
        {
            Window.Info.Show(Localizer.Instance["Tab2Control.Error1"]);
            return;
        }

        try
        {
            Window.Info1.Show(Localizer.Instance["Tab2Control.Info1"]);

            var res = JavaBinding.AddJava(name, local);
            if (res.Item1 == null)
            {
                Window.Info1.Close();
                Window.Info.Show(res.Item2!);
                return;
            }

            List.Add(res.Item1);
            TextBox_Name.Text = "";
            TextBox_Local.Text = "";
            Window.Info1.Close();
        }
        finally
        {

        }
    }

    private async void Button_SelectFile_Click(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog openFile = new()
        {
            Title = Localizer.Instance["Tab2Control.Info2"],
            AllowMultiple = false,
            Filters = SystemInfo.Os == OsType.Windows ? new()
            {
                new FileDialogFilter()
                {
                    Name =  "javaw" ,
                    Extensions =new()
                    {
                        "exe"
                    }
                }
            } : new()
        };

        var file = await openFile.ShowAsync(Window);
        if (file?.Length > 0)
        {
            var item = file[0];
            TextBox_Local.Text = item;
        }
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        Window.Next();
    }

    private void Button_Refash_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    public void Load()
    {
        List.Clear();
        List.AddRange(JavaBinding.GetJavaInfo());
    }

    public void SetWindow(HelloWindow window)
    {
        Window = window;
    }
}
