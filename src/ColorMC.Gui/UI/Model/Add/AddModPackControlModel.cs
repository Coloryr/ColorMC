using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// ������ϰ�
/// </summary>
public partial class AddModPackControlModel : TopModel, IAddControl
{
    /// <summary>
    /// ����Դ�б�
    /// </summary>
    public string[] SourceList { get; init; } = LanguageBinding.GetSourceList();

    /// <summary>
    /// ��Ϸ�汾�б�
    /// </summary>
    public ObservableCollection<string> GameVersionList { get; init; } = [];
    /// <summary>
    /// �����б�
    /// </summary>
    public ObservableCollection<string> CategorieList { get; init; } = [];
    /// <summary>
    /// �����б�
    /// </summary>
    public ObservableCollection<string> SortTypeList { get; init; } = [];
    /// <summary>
    /// ��Ŀ�б�
    /// </summary>
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = [];

    /// <summary>
    /// ����
    /// </summary>
    private readonly Dictionary<int, string> _categories = [];
    /// <summary>
    /// ѡ�е���Ŀ
    /// </summary>
    private FileItemModel? _last;
    /// <summary>
    /// �Ƿ��ڼ���
    /// </summary>
    private bool _load = false;
    /// <summary>
    /// �Ƿ�ر�
    /// </summary>
    private bool _close = false;
    /// <summary>
    /// ��һ������ID
    /// </summary>
    private string? _lastId;

    /// <summary>
    /// ����Դ
    /// </summary>
    [ObservableProperty]
    private int _source = -1;
    /// <summary>
    /// ����
    /// </summary>
    [ObservableProperty]
    private int _categorie;
    /// <summary>
    /// ����
    /// </summary>
    [ObservableProperty]
    private int _sortType;
    /// <summary>
    /// ��ǰҳ��
    /// </summary>
    [ObservableProperty]
    private int? _page = 0;
    /// <summary>
    /// ���ҳ��
    /// </summary>
    [ObservableProperty]
    private int _maxPage;
    /// <summary>
    /// ��Ϸ�汾
    /// </summary>
    [ObservableProperty]
    private string? _gameVersion;
    /// <summary>
    /// �����ı�
    /// </summary>
    [ObservableProperty]
    private string? _text;
    /// <summary>
    /// �Ƿ�ѡ������Ŀ
    /// </summary>
    [ObservableProperty]
    private bool _isSelect = false;
    /// <summary>
    /// �Ƿ�û����Ŀ
    /// </summary>
    [ObservableProperty]
    private bool _emptyDisplay = true;
    /// <summary>
    /// �Ƿ�����Դ��������
    /// </summary>
    [ObservableProperty]
    private bool _sourceLoad;
    /// <summary>
    /// �Ƿ�������һҳ
    /// </summary>
    [ObservableProperty]
    private bool _enableNextPage;

    /// <summary>
    /// �Ƿ��Ѿ���ʾ
    /// </summary>
    public bool Display { get; set; }

    /// <summary>
    /// �Ƿ�������
    /// </summary>
    private bool _keep = false;

    private readonly string _useName;

    public AddModPackControlModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "AddModPackControlModel";
    }

    /// <summary>
    /// ����ı�
    /// </summary>
    /// <param name="value"></param>
    partial void OnCategorieChanged(int value)
    {
        if (_load)
        {
            return;
        }

        Load();
    }

    /// <summary>
    /// ����ı�
    /// </summary>
    /// <param name="value"></param>
    partial void OnSortTypeChanged(int value)
    {
        if (_load)
        {
            return;
        }

        Load();
    }

    /// <summary>
    /// ��Ϸ�汾�ı�
    /// </summary>
    /// <param name="value"></param>
    partial void OnGameVersionChanged(string? value)
    {
        if (_load)
        {
            return;
        }

        GameVersionDownload = value;

        Load();
    }

    /// <summary>
    /// ҳ���ı�
    /// </summary>
    /// <param name="value"></param>
    partial void OnPageChanged(int? value)
    {
        if (_load)
            return;

        Load();
    }

    /// <summary>
    /// ����Դ�ı�
    /// </summary>
    /// <param name="value"></param>
    partial void OnSourceChanged(int value)
    {
        LoadSourceData();
    }

    /// <summary>
    /// ѡ����Ŀ
    /// </summary>
    [RelayCommand]
    public void Select()
    {
        if (_last == null)
        {
            Model.Show(App.Lang("AddModPackWindow.Error1"));
            return;
        }

        Install();
    }

    /// <summary>
    /// ˢ����Ŀ�б�
    /// </summary>
    [RelayCommand]
    public void Reload()
    {
        if (!string.IsNullOrWhiteSpace(Text) && Page != 0)
        {
            Page = 0;
            return;
        }

        Load();
    }

    /// <summary>
    /// ������ѡ��Ŀ
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Download()
    {
        if (Item == null)
            return;

        var res = await Model.ShowAsync(
            string.Format(App.Lang("AddModPackWindow.Info1"), Item.Name));
        if (res)
        {
            Install(Item);
        }
    }

    /// <summary>
    /// ��������Դ����
    /// </summary>
    public async void LoadSourceData()
    {
        if (_load)
        {
            return;
        }

        SourceLoad = false;
        _load = true;

        IsSelect = false;

        CategorieList.Clear();
        SortTypeList.Clear();

        GameVersionList.Clear();
        _categories.Clear();

        ClearList();

        switch (Source)
        {
            case 0:
            case 1:
                SortTypeList.AddRange(Source == 0 ?
                    LanguageBinding.GetCurseForgeSortTypes() :
                    LanguageBinding.GetModrinthSortTypes());

                Model.Progress(App.Lang("AddModPackWindow.Info4"));
                var list = Source == 0 ?
                    await CurseForgeHelper.GetGameVersionsAsync() :
                    await ModrinthHelper.GetGameVersionAsync();
                var list1 = Source == 0 ?
                    await CurseForgeHelper.GetCategoriesAsync(FileType.ModPack) :
                    await ModrinthHelper.GetCategoriesAsync(FileType.ModPack);
                Model.ProgressClose();
                if (list == null || list1 == null)
                {
                    _load = false;
                    LoadFail();
                    return;
                }
                GameVersionList.AddRange(list);

                _categories.Add(0, "");
                var a = 1;
                foreach (var item in list1)
                {
                    _categories.Add(a++, item.Key);
                }

                var list2 = new List<string>()
                {
                    ""
                };

                list2.AddRange(list1.Values);

                CategorieList.AddRange(list2);

                Categorie = 0;
                GameVersion = GameVersionList.FirstOrDefault();
                SortType = Source == 0 ? 1 : 0;

                Load();
                break;
        }

        SourceLoad = true;
        _load = false;
    }

    /// <summary>
    /// ѡ����Ŀ
    /// </summary>
    /// <param name="last"></param>
    public void SetSelect(FileItemModel last)
    {
        IsSelect = true;
        if (_last != null)
        {
            _last.IsSelect = false;
        }
        _last = last;
        _last.IsSelect = true;
    }

    /// <summary>
    /// ��ʼ��װ��Ŀ
    /// </summary>
    public void Install()
    {
        DisplayVersion = true;
        LoadVersion();
    }

    /// <summary>
    /// ��װ����ѹ
    /// </summary>
    /// <param name="text"></param>
    /// <param name="size"></param>
    /// <param name="all"></param>
    private void ZipUpdate(string text, int size, int all)
    {
        string temp = App.Lang("AddGameWindow.Tab1.Info21");
        Dispatcher.UIThread.Post(() => Model.ProgressUpdate($"{temp} {text} {size}/{all}"));
    }

    /// <summary>
    /// ������ʾ���ݣ���ȷ������ֵ
    /// </summary>
    /// <param name="text">����</param>
    /// <returns></returns>
    private async Task<bool> GameRequest(string text)
    {
        Model.ProgressClose();
        var test = await Model.ShowAsync(text);
        Model.Progress();
        return test;
    }

    /// <summary>
    /// ������Ϸʵ������
    /// </summary>
    /// <param name="obj">��Ҫ���ǵ���Ϸʵ��</param>
    /// <returns></returns>
    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        Model.ProgressClose();
        var test = await Model.ShowAsync(
            string.Format(App.Lang("AddGameWindow.Info2"), obj.Name));
        Model.Progress();
        return test;
    }

    /// <summary>
    /// ��ӽ���
    /// </summary>
    /// <param name="state"></param>
    private void PackState(CoreRunState state)
    {
        if (state == CoreRunState.Read)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab2.Info1"));
        }
        else if (state == CoreRunState.Init)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info2"));
        }
        else if (state == CoreRunState.GetInfo)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info3"));
        }
        else if (state == CoreRunState.Download)
        {
            Model.ProgressUpdate(-1);
            if (!ConfigBinding.WindowMode())
            {
                Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info4"));
            }
            else
            {
                Model.ProgressClose();
            }
        }
        else if (state == CoreRunState.DownloadDone)
        {
            if (ConfigBinding.WindowMode())
            {
                Model.Progress(App.Lang("AddGameWindow.Tab2.Info4"));
            }
        }
    }

    /// <summary>
    /// ������
    /// </summary>
    /// <param name="size"></param>
    /// <param name="now"></param>
    private void UpdateProcess(int size, int now)
    {
        Model.ProgressUpdate((double)now / size);
    }

    /// <summary>
    /// ������
    /// </summary>
    private async void Done(string? uuid)
    {
        Model.Notify(App.Lang("AddGameWindow.Tab1.Info7"));

        DisplayVersion = false;

        if (_keep)
        {
            return;
        }

        var model = WindowManager.MainWindow?.DataContext as MainModel;
        model?.Select(uuid);

        var res = await Model.ShowAsync(App.Lang("AddGameWindow.Tab1.Info25"));
        if (res != true)
        {
            Dispatcher.UIThread.Post(WindowClose);
        }
        else
        {
            _keep = true;
        }
    }

    /// <summary>
    /// ��������Դ��Ϣʧ��
    /// </summary>
    private async void LoadFail()
    {
        var res = await Model.ShowAsync(App.Lang("AddModPackWindow.Error4"));
        if (res)
        {
            LoadSourceData();
            return;
        }

        if (Source < SourceList.Length)
        {
            res = await Model.ShowAsync(App.Lang("AddModPackWindow.Info5"));
            if (res)
            {
                Source++;
            }
        }
    }

    /// <summary>
    /// ������Ŀ�б�
    /// </summary>
    private async void Load()
    {
        //MO����������������
        if (Source == 1 && Categorie == 4 && Text?.Length < 3)
        {
            Model.Show(App.Lang("AddModPackWindow.Error6"));
            return;
        }

        Model.Progress(App.Lang("AddModPackWindow.Info2"));
        var res = await WebBinding.GetModPackListAsync((SourceType)Source,
            GameVersion, Text, Page ?? 0, Source == 2 ? Categorie : SortType,
            Source == 2 ? "" : Categorie < 0 ? "" : _categories[Categorie]);

        //������ҳ
        if (Source == 0)
        {
            MaxPage = res.Count / 20;
            EnableNextPage = (MaxPage - Page) > 0;
        }
        else
        {
            MaxPage = int.MaxValue;
            EnableNextPage = true;
        }

        var data = res.List;

        if (data == null)
        {
            Model.Show(App.Lang("AddModPackWindow.Error2"));
            Model.ProgressClose();
            return;
        }

        DisplayList.Clear();

        //һҳ20
        int b = 0;
        for (int a = 0; a < data.Count; a++, b++)
        {
            if (b >= 20)
            {
                break;
            }
            var item = data[a];
            item.Add = this;
            DisplayList.Add(item);
        }

        OnPropertyChanged(nameof(DisplayList));

        _last = null;

        EmptyDisplay = DisplayList.Count == 0;

        Model.ProgressClose();
        Model.Notify(App.Lang("AddWindow.Info16"));
    }

    /// <summary>
    /// ��װ��ѡ��Ŀ
    /// </summary>
    /// <param name="item"></param>
    public void Install(FileItemModel item)
    {
        SetSelect(item);
        Install();
    }

    /// <summary>
    /// ������Ŀ�б�
    /// </summary>
    private void ClearList()
    {
        foreach (var item in DisplayList)
        {
            item.Close();
        }
        DisplayList.Clear();
    }

    /// <summary>
    /// ��һҳ
    /// </summary>
    public void Back()
    {
        if (_load || Page <= 0)
        {
            return;
        }

        Page -= 1;
    }

    /// <summary>
    /// ��һҳ
    /// </summary>
    public void Next()
    {
        if (_load)
        {
            return;
        }

        Page += 1;
    }

    /// <summary>
    /// F5���ذ汾�б�
    /// </summary>
    public void ReloadF5()
    {
        if (DisplayVersion)
        {
            LoadVersion();
        }
        else
        {
            Load();
        }
    }

    public override void Close()
    {
        if (DisplayVersion)
        {
            Model.PopBack();
        }

        _close = true;
        _load = true;
        Model.RemoveChoiseData(_useName);
        FileList.Clear();
        foreach (var item in DisplayList)
        {
            item.Close();
        }
        DisplayList.Clear();
        _last = null;
    }

    /// <summary>
    /// ת����������
    /// </summary>
    /// <param name="type">����Դ</param>
    /// <param name="pid">��ĿID</param>
    public async void GoFile(SourceType type, string pid)
    {
        Source = (int)type;
        await Task.Run(() =>
        {
            while ((!Display || _load) && !_close)
            {
                Thread.Sleep(100);
            }
        });

        _lastId = pid;

        _load = true;
        PageDownload = 0;
        DisplayVersion = true;
        LoadVersion();
        _load = false;
    }
}
