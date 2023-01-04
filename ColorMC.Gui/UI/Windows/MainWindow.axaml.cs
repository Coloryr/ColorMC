using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public partial class MainWindow : Window
{
    private readonly List<GamesControl> Groups = new();
    private GamesControl DefaultGroup;
    private GameSettingObj? Obj;

    public delegate void UserEditHandler();
    public static event UserEditHandler UserEdit;

    public MainWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;

        ItemInfo.SetWindow(this);

        Task.Run(Load);
        Task.Run(Load1);

        CoreMain.GameLaunch = GameLunch;
        CoreMain.GameDownload = GameDownload;

        UserEdit += MainWindow_OnUserEdit;
        Opened += MainWindow_Opened;
        Closed += MainWindow_Closed;

        Update();
    }

    public async void Launch(bool debug)
    {
        Info1.Show("����������Ϸ");
        var res = await GameBinding.Launch(Obj, debug);
        if (res.Item1 == false)
        {
            Info1.Close();
            Info.Show(res.Item2!);
        }
    }

    public static void OnUserEdit()
    {
        if (UserEdit != null)
        {
            UserEdit();
        }
    }

    private void MainWindow_OnUserEdit()
    {
        Task.Run(Load1);
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        App.MainWindow = null;
        CoreMain.GameLaunch = null;
        CoreMain.GameDownload = null;
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        ItemInfo.Expander1.MakeTran();
    }

    public void GameItemSelect(GameSettingObj? obj)
    {
        Obj = obj;
        ItemInfo.SetGame(obj);
    }

    private async Task<bool> GameDownload(LaunchState state, GameSettingObj obj)
    {
        switch (state)
        {
            default:
            case LaunchState.LostGame:
            case LaunchState.LostVersion:
                return await Info.ShowWait("�Ƿ�����ȱʧ����Ϸ�ļ�");
            case LaunchState.LostLib:
                return await Info.ShowWait("�Ƿ�����ȱʧ�����п��ļ�");
            case LaunchState.LostLoader:
                return await Info.ShowWait("�Ƿ�����ȱʧ��Mod������");
            case LaunchState.LostLoginCore:
                return await Info.ShowWait("�Ƿ�����ȱʧ�����õ�½��");
        } 
    }

    private void GameLunch(GameSettingObj obj, LaunchState state)
    {
        switch (state)
        {
            case LaunchState.Login:
                Info1.NextText("���ڵ�¼�˻�");
                break;
            case LaunchState.Check:
                Info1.NextText("���ڼ����Ϸ�ļ�");
                break;
            case LaunchState.CheckVersion:
                Info1.NextText("���ڼ����Ϸ�����ļ�");
                break;
            case LaunchState.CheckLib:
                Info1.NextText("���ڼ����Ϸ���п�");
                break;
            case LaunchState.CheckAssets:
                Info1.NextText("���ڼ����Ϸ��Դ�ļ�");
                break;
            case LaunchState.CheckLoader:
                Info1.NextText("���ڼ����ϷMod������");
                break;
            case LaunchState.CheckLoginCore:
                Info1.NextText("���ڼ����Ϸ���õ�½��");
                break;
            case LaunchState.Download:
                Info1.NextText("�������������ļ�");
                break;
            case LaunchState.JvmPrepare:
                Info1.NextText("����׼��Jvm����");
                break;

            case LaunchState.LoginFail:
                Info1.Close();
                Info.Show("�˻���¼ʧ��");
                break;
        }
    }

    private void Load1()
    {
        Dispatcher.UIThread.Post(() => { ItemInfo.SetUser(UserBinding.GetLastUser()); });
    }

    private async void Load()
    {
        var list = GameBinding.GetGameGroups();

        Groups.Clear();
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            DefaultGroup = new();
        });
        DefaultGroup.SetWindow(this);
        foreach (var item in list)
        {
            if (item.Key == " ")
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    DefaultGroup.SetItems(item.Value);
                    DefaultGroup.SetName("Ĭ�Ϸ���");
                });
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var group = new GamesControl();
                    group.SetItems(item.Value);
                    group.SetName(item.Key);
                    group.SetWindow(this);
                    Groups.Add(group);
                });
            }
        }

        Dispatcher.UIThread.Post(() =>
        {
            GameGroups.Children.Clear();
            foreach (var item in Groups)
            {
                GameGroups.Children.Add(item);
            }
            GameGroups.Children.Add(DefaultGroup);
        });
    }

    public void Update()
    {
        App.Update(this, Image_Back, Rectangle1);
    }
}
