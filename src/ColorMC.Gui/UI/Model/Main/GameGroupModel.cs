using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 游戏分组
/// </summary>
public partial class GameGroupModel : ControlModel
{
    /// <summary>
    /// 游戏列表
    /// </summary>
    public ObservableCollection<GameItemModel> GameList { get; init; } = [];

    /// <summary>
    /// 标题
    /// </summary>
    public string Header { get; }
    /// <summary>
    /// 键
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 是否展开
    /// </summary>
    [ObservableProperty]
    private bool _expander = true;

    /// <summary>
    /// 游戏分组操作
    /// </summary>
    public readonly IMutTop Top;

    /// <summary>
    /// 游戏项目列表
    /// </summary>
    public readonly Dictionary<Guid, GameItemModel> Items = [];
    /// <summary>
    /// 添加项目用
    /// </summary>
    private readonly GameItemModel _addItem;

    public GameGroupModel(WindowModel model, IMutTop top, string key, string name, List<GameSettingObj> list) : base(model)
    {
        Top = top;
        Header = name;
        Key = key;
        GameList.Clear();
        Items.Clear();
        foreach (var item in list)
        {
            var model1 = new GameItemModel(Window, Top, item);
            Items.Add(item.UUID, model1);
        }
        _addItem = new(Window, Key == Names.NameDefaultGroup ? null : Key);
        Task.Run(() =>
        {
            bool res = false;
            int index = 1;
            if (Items.Count == 5)
            {
                var random = new Random();
                if (random.Next(2000) == 666)
                {
                    res = true;
                }
            }
            //if (GuiConfigUtils.Config.Simple)
            //{
            //    res = false;
            //}

            var list1 = new List<GameItemModel>();

            //检查是否星标
            foreach (var item in Items)
            {
                if (!GameManager.IsStar(item.Value.Obj))
                {
                    list1.Add(item.Value);
                    continue;
                }
                Thread.Sleep(50);
                Dispatcher.UIThread.Invoke(() =>
                {
                    GameList.Add(item.Value);
                    if (res)
                    {
                        item.Value.Index = index++;
                    }
                });
            }

            bool fast = list1.Count >= 100;
            if (fast)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    foreach (var item in list1)
                    {
                        GameList.Add(item);
                        if (res)
                        {
                            item.Index = index++;
                        }
                    }
                });
            }
            else
            {
                foreach (var item in list1)
                {
                    Thread.Sleep(50);
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        GameList.Add(item);
                        if (res)
                        {
                            item.Index = index++;
                        }
                    });
                }
            }

            Dispatcher.UIThread.Post(() =>
            {
                GameList.Add(_addItem);
            });
        });
    }

    /// <summary>
    /// 启动所有
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task LaunchAll()
    {
        var res = await Window.ShowChoice(LangUtils.Get("MainWindow.Text77"));
        if (!res)
        {
            return;
        }
        Top.Launch(Items.Values);
    }

    /// <summary>
    /// 拖拽
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool DropIn(IDataTransfer data)
    {
        if (data.TryGetValue(BaseBinding.DrapType) is not { } c)
        {
            return false;
        }
        var uuid = Guid.Parse(c);
        return !Items.ContainsKey(uuid);
    }

    /// <summary>
    /// 拖拽
    /// </summary>
    /// <param name="data"></param>
    public void Drop(IDataTransfer data)
    {
        if (data.TryGetValue(BaseBinding.DrapType) is not { } c)
        {
            return;
        }
        var uuid = Guid.Parse(c);
        var game = Top.GetGame(uuid);
        if (game == null)
        {
            return;
        }
        game.IsDrop = false;
        if (Items.ContainsKey(uuid))
        {
            return;
        }

        GameBinding.MoveGameGroup(game.Obj, Key);
    }

    /// <summary>
    /// 查找游戏实例
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns></returns>
    public GameItemModel? Find(Guid? uuid)
    {
        if (uuid is not { } guid)
        {
            return null;
        }
        if (Items.TryGetValue(guid, out var item))
        {
            Expander = true;
            return item;
        }

        return null;
    }

    /// <summary>
    /// 设置游戏实例项目
    /// </summary>
    /// <param name="list"></param>
    public void SetItems(List<GameSettingObj> list)
    {
        var remove = new List<Guid>();
        var ins = new List<GameSettingObj>();

        foreach (var item in list)
        {
            if (Items.ContainsKey(item.UUID))
            {
                continue;
            }
            ins.Add(item);
        }

        //筛选删除的内容
        foreach (var item in Items.Keys)
        {
            if (list.Any(item1 => item1.UUID == item))
            {
                continue;
            }
            remove.Add(item);
        }

        if (GameList.Count == 0 || GameList.Count - 1 < 0)
        {
            return;
        }
        GameList.RemoveAt(GameList.Count - 1);

        foreach (var item in remove)
        {
            Items.Remove(item);
            var model = GameList.FirstOrDefault(item1 => item1.Obj.UUID == item);
            if (model != null)
            {
                model.Close();
                GameList.Remove(model);
            }
        }

        //筛选添加的内容
        foreach (var item in ins)
        {
            var model1 = new GameItemModel(Window, Top, item);
            Items.Add(item.UUID, model1);
        }

        Task.Run(() =>
        {
            foreach (var item in ins)
            {
                Thread.Sleep(50);
                Dispatcher.UIThread.Invoke(() =>
                {
                    GameList.Add(Items[item.UUID]);
                });
            }

            Dispatcher.UIThread.Post(() =>
            {
                GameList.Add(_addItem);
            });
        });
    }

    public override void Close()
    {
        foreach (var item in GameList)
        {
            item.Close();
        }
        GameList.Clear();
        Items.Clear();
    }

    /// <summary>
    /// 显示所有项目
    /// </summary>
    public void DisplayAll()
    {
        foreach (var item in GameList)
        {
            item.IsDisplay = true;
        }
    }

    /// <summary>
    /// 显示指定名字的项目
    /// </summary>
    /// <param name="value">名字</param>
    public void Display(string value)
    {
        foreach (var item in GameList)
        {
            item.IsDisplay = !item.IsNew && item.Name.Contains(value);
        }
    }

    /// <summary>
    /// 星标
    /// </summary>
    /// <param name="uuid">游戏实例UUID</param>
    /// <returns></returns>
    public bool Star(Guid uuid)
    {
        foreach (var item in GameList)
        {
            if (item.IsNew || item.UUID != uuid)
            {
                continue;
            }

            GameList.Move(GameList.IndexOf(item), 0);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 取消星标
    /// </summary>
    /// <param name="uuid">游戏实例UUID</param>
    /// <returns></returns>
    public bool UnStar(Guid uuid)
    {
        foreach (var item in GameList)
        {
            if (item.IsNew || item.UUID != uuid)
            {
                continue;
            }

            GameList.Move(GameList.IndexOf(item), GameList.Count - 2);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 全选所有
    /// </summary>
    public void MutAll()
    {
        foreach (var item in GameList)
        {
            item.IsCheck = true;
        }
    }
}
