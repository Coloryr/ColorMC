using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Controls.Hello;

public partial class Tab3Control : UserControl
{
    private readonly ObservableCollection<JavaInfoObj> List = new();
    public Tab3Control()
    {
        InitializeComponent();
        List_Java.ItemsSource = List;

        Button_SelectFile.Click += Button_SelectFile_Click;
        Button_Add.Click += Button_Add_Click;
        Button_Refash.Click += Button_Refash_Click;
        Button_Next.Click += Button_Next_Click;
        Button_Delete.Click += Button_Delete_Click;
        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;

        Load();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var list = JavaBinding.FindJava();
        if (list == null)
        {
            window.Info.Show(App.GetLanguage("HelloWindow.Tab3.Error2"));
            return;
        }

        list.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        Load();
        window.Info2.Show(App.GetLanguage("HelloWindow.Tab3.Info3"));
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowAddJava();
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
        var window = App.FindRoot(VisualRoot);
        var name = TextBox_Name.Text;
        var local = TextBox_Local.Text;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(local))
        {
            window.Info.Show(App.GetLanguage("HelloWindow.Tab3.Error1"));
            return;
        }

        try
        {
            window.Info1.Show(App.GetLanguage("HelloWindow.Tab3.Info1"));

            var res = JavaBinding.AddJava(name, local);
            if (res.Item1 == null)
            {
                window.Info1.Close();
                window.Info.Show(res.Item2!);
                return;
            }

            List.Add(res.Item1);
            TextBox_Name.Text = "";
            TextBox_Local.Text = "";
            window.Info1.Close();
        }
        finally
        {

        }
    }

    private async void Button_SelectFile_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var file = await BaseBinding.OpFile(window, FileType.Java);

        if (file != null)
        {
            TextBox_Local.Text = file;
            var info = JavaBinding.GetJavaInfo(file);
            if (info != null)
            {
                TextBox_Name.Text = info.Type + "_" + info.Version;
            }
        }
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        (window.Con as HelloControl)?.Next();
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
}
