using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Utils;
using ColorMC.UIBinding;
using DynamicData;
using System.Collections.ObjectModel;

namespace ColorMC.UI.Views.Hello;

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

        Load();
    }

    private void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        string name = Input_Name.Text;
        string local = Input_Local.Text;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(local))
        {
            Window.Info.Show("请输入必要信息");
            return;
        }

        try
        {
            Window.Info1.Show("正在检查Java");

            var res = JavaBinding.AddJava(name, local);
            if (res.Item1 == null)
            {
                Window.Info1.Close();
                Window.Info.Show(res.Item2);
                return;
            }

            List.Add(res.Item1);
            Input_Name.Text = "";
            Input_Local.Text = "";
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
            Title = "选择Java",
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
            Input_Local.Text = item;
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
