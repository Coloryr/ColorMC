using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab6Control : UserControl
{
    private GameSettingObj Obj;
    private FilesPageViewModel FilesPageViewModel;

    public FilesPageViewModel Files
    {
        get => FilesPageViewModel;
    }

    public Tab6Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab6.Info2"));
        var file = await BaseBinding.SaveFile(window as Window, FileType.Game, new object[]
            { Obj, FilesPageViewModel.GetUnSelectItems(), PackType.ColorMC });
        window.ProgressInfo.Close();
        if (file == null)
            return;

        if (file == false)
        {
            window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab6.Error1"));
        }
        else
        {
            window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab6.Info3"));
        }
    }

    private void Load()
    {
        FilesPageViewModel = new FilesPageViewModel(Obj);
        FileViewer.Source = Files.Source;

        if (BaseBinding.IsGameRun(Obj))
        {
            Button1.IsEnabled = false;
        }
        else
        {
            Button1.IsEnabled = true;
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

        Load();
    }
}
