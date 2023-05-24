using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab3Model : ObservableObject
{
    private readonly IUserControl Con;

    private bool load;

    public List<string> SourceList => BaseBinding.GetDownloadSources();

    [ObservableProperty]
    private int source;
    [ObservableProperty]
    private int thread;

    [ObservableProperty]
    private string iP;
    [ObservableProperty]
    private ushort port;
    [ObservableProperty]
    private string user;
    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private bool isDownload;
    [ObservableProperty]
    private bool loginProxy;
    [ObservableProperty]
    private bool downloadProxy;
    [ObservableProperty]
    private bool gameProxy;
    [ObservableProperty]
    private bool checkFile;
    [ObservableProperty]
    private bool checkUpdate;
    [ObservableProperty]
    private bool autoDownload;

    public SettingTab3Model(IUserControl con)
    {
        Con = con;
    }

    partial void OnCheckFileChanged(bool value)
    {
        SetCheck();
    }

    partial void OnCheckUpdateChanged(bool value)
    {
        SetCheck();
    }

    partial void OnAutoDownloadChanged(bool value)
    {
        SetCheck();
    }

    partial void OnDownloadProxyChanged(bool value)
    {
        SetProxyEnable();
    }

    partial void OnLoginProxyChanged(bool value)
    {
        SetProxyEnable();
    }

    partial void OnGameProxyChanged(bool value)
    {
        SetProxyEnable();
    }

    partial void OnThreadChanged(int value)
    {
        ConfigBinding.SetDownloadThread(value);
    }

    partial void OnSourceChanged(int value)
    {
        ConfigBinding.SetDownloadSource((SourceLocal)value);
    }

    [RelayCommand]
    public void OpenDownload()
    {
        BaseBinding.OpenDownloadPath();
    }

    [RelayCommand]
    public void OpenPic()
    {
        BaseBinding.OpenPicPath();
    }

    [RelayCommand]
    public void StartUpdate()
    {
        UpdateChecker.StartUpdate();
    }

    [RelayCommand]
    public async void StartCheck()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab3.Info1"));
        var res = await UpdateChecker.CheckOne();
        window.ProgressInfo.Close();
        if (res.Item1 == null)
        {
            window.OkInfo.Show(App.GetLanguage("Gui.Error21"));
            return;
        }
        else if (res.Item1 == true)
        {
            var res1 = await window.TextInfo.ShowWait(App.GetLanguage("SettingWindow.Tab3.Info2"), res.Item2!);
            if (!res1)
            {
                UpdateChecker.StartUpdate();
            }
        }
        else
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab3.Info3"));
        }
    }

    [RelayCommand]
    public void SetProxy()
    {
        ConfigBinding.SetDownloadProxy(IP, Port, User, Password);
    }

    public void Load()
    {
        load = true;

        IsDownload = BaseBinding.IsDownload;

        var config = ConfigBinding.GetAllConfig();
        if (config.Item1 != null)
        {
            Source= (int)config.Item1.Http.Source;

            Thread = config.Item1.Http.DownloadThread;

            IP = config.Item1.Http.ProxyIP;
            Port = config.Item1.Http.ProxyPort;
            User = config.Item1.Http.ProxyUser;
            Password = config.Item1.Http.ProxyPassword;

            LoginProxy = config.Item1.Http.LoginProxy;
            DownloadProxy = config.Item1.Http.DownloadProxy;
            GameProxy = config.Item1.Http.GameProxy;
            CheckFile = config.Item1.Http.CheckFile;
            AutoDownload = config.Item1.Http.AutoDownload;
            CheckUpdate = config.Item1.Http.CheckUpdate;
        }
        load = false;
    }

    private void SetCheck()
    {
        if (load)
            return;

        ConfigBinding.SetDownloadCheck(CheckFile, AutoDownload, CheckUpdate);
    }

    private void SetProxyEnable()
    {
        if (load)
            return;

        ConfigBinding.SetDownloadProxyEnable(LoginProxy , DownloadProxy, GameProxy);
    }
}
