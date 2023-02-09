using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using DynamicData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Hello;

public partial class Tab3Control : UserControl
{
    private HelloWindow Window;

    private ObservableCollection<JavaInfoObj> List = new();
    public Tab3Control()
    {
        InitializeComponent();
        List_Java.Items = List;

        Button_SelectFile.Click += Button_SelectFile_Click;
        Button_Add.Click += Button_Add_Click;
        Button_Refash.Click += Button_Refash_Click;
        Button_Next.Click += Button_Next_Click;
        Button_Delete.Click += Button_Delete_Click;
        Button1.Click += Button1_Click;

        Load();
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
        var name = TextBox_Name.Text;
        var local = TextBox_Local.Text;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(local))
        {
            Window.Info.Show(Localizer.Instance["HelloWindow.Tab3.Error1"]);
            return;
        }

        try
        {
            Window.Info1.Show(Localizer.Instance["HelloWindow.Tab3.Info1"]);

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
        var file = await Window.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = Localizer.Instance["HelloWindow.Tab3.Info2"],
            SuggestedStartLocation = JavaBinding.GetSuggestedStartLocation(),
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>()
            {
                new("Java")
                {
                     Patterns = SystemInfo.Os == OsType.Windows ? new List<string>()
                     {
                        "*.exe"
                     } : new()
                }
            }
        });

        if (file?.Any() == true)
        {
            var item = file[0];
            TextBox_Local.Text = item.GetPath();
            var info = JavaBinding.GetJavaInfo(item.GetPath());
            if (info != null)
            {
                TextBox_Name.Text = info.Type + "_" + info.Version;
            }
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
