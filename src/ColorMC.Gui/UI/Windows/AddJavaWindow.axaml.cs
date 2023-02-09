using Avalonia.Controls;
using ColorMC.Gui.UIBinding;
using Avalonia.Interactivity;
using ColorMC.Core.Net.Java;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ColorMC.Gui.Objs;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using System.Threading.Tasks;
using ColorMC.Core.LaunchPath;
using static ColorMC.Core.Objs.Java.AdoptiumObj;
using ColorMC.Core.Objs.Java;
using System.IO;

namespace ColorMC.Gui.UI.Windows;

public partial class AddJavaWindow : Window
{
    private List<JavaDownloadDisplayObj> List1 = new();
    private ObservableCollection<JavaDownloadDisplayObj> List = new();
    private bool load = true;
    public AddJavaWindow()
    {
        InitializeComponent();

        Head.SetWindow(this);
        this.BindFont();
        Icon = App.Icon;
        Rectangle1.MakeResizeDrag(this);

        DataGrid1.Items = List;

        ComboBox1.Items = JavaBinding.GetJavaType();

        ComboBox1.SelectedIndex = 0;

        Switch();

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;
        ComboBox3.SelectionChanged += ComboBox3_SelectionChanged;
        ComboBox4.SelectionChanged += ComboBox4_SelectionChanged;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;

        Button1.Click += Button1_Click;

        Button1_Click(null, null);
    }

    private async void DataGrid1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataGrid1.SelectedItem is not JavaDownloadDisplayObj obj)
            return;

        var res = await Info.ShowWait(string.Format("是否要下载 {0}", obj.Name));
        if (!res)
            return;

        Info1.Show("正在下载Java");
        var res1 = await JvmPath.Install(obj.File, obj.Name, obj.Sha256, obj.Url);
        Info1.Close();
        if (res1.Item1 != Core.CoreRunState.Init)
        {
            Info.Show(res1.Item2!);
        }

        Info2.Show("下载Java成功");
    }

    private void ComboBox4_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        Select();
    }

    private void ComboBox3_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        if (ComboBox1.SelectedIndex == 0)
        {
            Button1_Click(null, null);
        }
        else
        {
            Select();
        }
    }

    private void ComboBox2_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        if (ComboBox1.SelectedIndex == 0)
        {
            Button1_Click(null, null);
        }
        else
        {
            Select();
        }
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Switch();
        Button1_Click(null, null);
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Info1.Show("正在获取版本");

        load = true;

        List1.Clear();
        List.Clear();

        switch (ComboBox1.SelectedIndex)
        {
            case 0:
                if (ComboBox3.SelectedItem is not string version)
                    return;

                await GetAdoptiumList(version);
                break;
            case 1:
                await GetZuluList();
                break;
            case 2:
                await GetDragonwellList();
                break;
        }

        Select();

        load = false;

        Info1.Close();
    }

    private void Select()
    {
        List.Clear();

        string arch = "";
        string version = "";
        string os = "";
        if (ComboBox4.SelectedItem is string temp)
        {
            arch = temp;
        }
        if (ComboBox3.SelectedItem is string temp1)
        {
            version = temp1;
        }
        if (ComboBox2.SelectedItem is string temp2)
        {
            os = temp2.ToLower();
        }
        bool arch1 = !string.IsNullOrWhiteSpace(arch);
        bool version1 = !string.IsNullOrWhiteSpace(version);
        bool os1 = !string.IsNullOrWhiteSpace(os);

        var list =
               from item in List1
               where (arch1 ? (item.Arch == arch) : true)
               && (version1 ? (item.MainVersion == version) : true)
               && (ComboBox1.SelectedIndex == 0 ? true : os1 ? (item.Os == os) : true)
               select item;

        List.AddRange(list);
    }

    private void Switch()
    {
        switch (ComboBox1.SelectedIndex)
        {
            case 0:
                ComboBox3.Items = Adoptium.JavaVersion;
                ComboBox2.Items = JavaBinding.GetSystemType();
                ComboBox3.SelectedIndex = 0;
                ComboBox2.SelectedIndex = 0;
                ComboBox4.SelectedIndex = 0;
                break;
            case 1:
                ComboBox3.SelectedItem = null;
                ComboBox2.Items = JavaBinding.GetSystemType();
                ComboBox3.Items = null;
                ComboBox2.SelectedIndex = 0;
                ComboBox4.SelectedIndex = 0;
                break;
            case 2:
                ComboBox2.Items = null;
                ComboBox3.Items = null;
                ComboBox4.Items = null;
                ComboBox3.SelectedIndex = 0;
                ComboBox2.SelectedIndex = 0;
                ComboBox4.SelectedIndex = 0;
                break;
        }
    }

    private async Task GetZuluList()
    {
        try
        {
            var list = await Zulu.GetJavaList();
            if (list == null)
            {
                Info.Show("获取失败");
                return;
            }

            var list1 =
                from item in list
                group item by item.arch + '_' + item.hw_bitness into newGroup
                orderby newGroup.Key descending
                select newGroup.Key;

            var list2 = new List<string>
            {
                ""
            };
            list2.AddRange(list1);
            ComboBox4.Items = list2;
            ComboBox4.SelectedIndex = 0;

            var list3 =
                from item in list
                group item by item.java_version[0] into newGroup
                orderby newGroup.Key descending
                select newGroup.Key.ToString();

            var list4 = new List<string>
            {
                ""
            };
            list4.AddRange(list3);
            ComboBox3.Items = list4;
            ComboBox3.SelectedIndex = 0;

            foreach (var item in list)
            {
                List1.Add(new()
                {
                    Name = item.name,
                    Arch = item.arch + '_' + item.hw_bitness,
                    Os = item.os,
                    MainVersion = item.zulu_version[0].ToString(),
                    Version = ToStr(item.zulu_version),
                    Size = UIUtils.MakeFileSize1(0),
                    Url = item.url,
                    Sha256 = item.sha256_hash,
                    File = item.name
                });
            }
        }
        catch (Exception e)
        {
            App.ShowError("获取Java列表", e);
            Info.Show("获取失败");
            return;
        }
    }

    private string ToStr(List<int> list)
    {
        string a = "";
        foreach (var item in list)
        {
            a += item + ".";
        }
        return a[..^1];
    }

    private async Task GetAdoptiumList(string version)
    {
        try
        {
            var list = await Adoptium.GetJavaList(version, ComboBox2.SelectedIndex);
            if (list == null)
            {
                Info.Show("获取失败");
                return;
            }

            var list1 =
                from item in list
                group item by item.binary.architecture into newGroup
                orderby newGroup.Key descending
                select newGroup.Key;

            var list2 = new List<string>
            {
                ""
            };
            list2.AddRange(list1);
            ComboBox4.Items = list2;

            foreach (var item in list)
            {
                List1.Add(new()
                {
                    Name = item.binary.scm_ref + "_" + item.binary.image_type,
                    Arch = item.binary.architecture,
                    Os = item.binary.os,
                    MainVersion = version,
                    Version = item.version.openjdk_version,
                    Size = UIUtils.MakeFileSize1(item.binary.package.size),
                    Url = item.binary.package.link,
                    Sha256 = item.binary.package.checksum,
                    File = item.binary.package.name
                });
            }
        }
        catch (Exception e)
        {
            App.ShowError("获取Java列表", e);
            Info.Show("获取失败");
            return;
        }
    }

    private void AddDragonwell(DragonwellObj.Item item)
    {
        string main = "8";
        string version = item.version8;
        string file;
        if (item.xurl8 != null)
        {
            file = Path.GetFileName(item.xurl8);
            List1.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.xurl8,
                File = file
            });
        }
        if (item.aurl8 != null)
        {
            file = Path.GetFileName(item.aurl8);
            List1.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.aurl8,
                File = file
            });
        }
        if (item.wurl8 != null)
        {
            file = Path.GetFileName(item.wurl8);
            List1.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.wurl8,
                File = file
            });
        }

        main = "11";
        version = item.version11;
        if (item.xurl11 != null)
        {
            file = Path.GetFileName(item.xurl11);
            List1.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.xurl11,
                File = file
            });
        }
        if (item.aurl11 != null)
        {
            file = Path.GetFileName(item.aurl11);
            List1.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.aurl11,
                File = file
            });
        }
        if (item.apurl11 != null)
        {
            file = Path.GetFileName(item.apurl11);
            List1.Add(new()
            {
                Name = file,
                Arch = "x64_alpine",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.apurl11,
                File = file
            });
        }
        if (item.wurl11 != null)
        {
            file = Path.GetFileName(item.wurl11);
            List1.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.wurl11,
                File = file
            });
        }
        if (item.rurl11 != null)
        {
            file = Path.GetFileName(item.rurl11);
            List1.Add(new()
            {
                Name = file,
                Arch = "riscv64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.rurl11,
                File = file
            });
        }

        main = "17";
        version = item.version17;
        if (item.xurl17 != null)
        {
            file = Path.GetFileName(item.xurl17);
            List1.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.xurl17,
                File = file
            });
        }
        if (item.aurl17 != null)
        {
            file = Path.GetFileName(item.aurl17);
            List1.Add(new()
            {
                Name = file,
                Arch = "aarch64",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.aurl17,
                File = file
            });
        }
        if (item.apurl17 != null)
        {
            file = Path.GetFileName(item.apurl17);
            List1.Add(new()
            {
                Name = file,
                Arch = "x64_alpine",
                Os = "linux",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.apurl17,
                File = file
            });
        }
        if (item.wurl17 != null)
        {
            file = Path.GetFileName(item.wurl17);
            List1.Add(new()
            {
                Name = file,
                Arch = "x64",
                Os = "windows",
                MainVersion = main,
                Version = version,
                Size = "0",
                Url = item.wurl17,
                File = file
            });
        }
    }

    private async Task GetDragonwellList()
    {
        try
        {
            var list = await Dragonwell.GetJavaList();
            if (list == null)
            {
                Info.Show("获取失败");
                return;
            }

            AddDragonwell(list.extended);
            AddDragonwell(list.standard);
        }
        catch (Exception e)
        {
            App.ShowError("获取Java列表", e);
            Info.Show("获取失败");
            return;
        }
    }
}
