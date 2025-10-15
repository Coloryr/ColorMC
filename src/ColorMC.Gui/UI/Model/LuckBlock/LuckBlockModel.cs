using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.LuckBlock;

public partial class LuckBlockModel : TopModel
{
    private readonly Random _random = new();

    private CancellationTokenSource? _decelerationCancellation;

    [ObservableProperty]
    private bool _isAnimating;
    [ObservableProperty]
    private double _scrollSpeed = 5;
    [ObservableProperty]
    private bool _showResult;
    [ObservableProperty]
    private bool _canRun;
    [ObservableProperty]
    private bool _canStart;
    [ObservableProperty]
    private BlockItemModel? _selectedItem;

    public ObservableCollection<BlockItemModel> LotteryItems { get; init; } = [];
    public double ContainerWidth { get; set; } = 800;
    public double Left { get; set; } = 0;

    public LuckBlockModel(BaseModel model) : base(model)
    {
        
    }

    [RelayCommand]
    public void Start()
    {
        StartLottery();
    }

    [RelayCommand]
    public void Backpack()
    {
        WindowManager.ShowBlockBackpack();
    }

    public void StartLottery()
    {
        CanStart = false;
        IsAnimating = true;
        ScrollSpeed = _random.Next(80, 120);
        SelectedItem = null;
        ShowResult = false;

        Reset();

        DispatcherTimer.RunOnce(StopLottery, TimeSpan.FromMilliseconds(_random.Next(1000, 3000)));
    }

    private void Reset()
    {
        // 重置所有物品位置和状态
        double xPosition = 0;
        foreach (var itemVm in LotteryItems)
        {
            itemVm.Left = xPosition;
            xPosition += 140;
        }
    }

    public async void LoadBlocks()
    {
        Model.Progress(App.Lang("LuckBlockWindow.Info1"));
        var res = await BaseBinding.StartLoadBlock();
        Model.ProgressClose();
        if (!res.State)
        {
            Model.ShowWithOk(res.Data!, Close);
            return;
        }

        var list = await BaseBinding.BuildLotteryItems();
        if (list == null)
        {
            Model.ShowWithOk(App.Lang("LuckBlockWindow.Error4"), Close);

        }

        LotteryItems.AddRange(list);

        Reset();

        if (BlockTexUtils.IsGet())
        {
            CanRun = false;

            var block = BlockTexUtils.Unlocks.Today;
            SelectedItem = LotteryItems.FirstOrDefault(item => item.Key == block);
            ShowResult = true;
        }
        else
        {
            CanStart = true;
            CanRun = true;
        }
        IsAnimating = true;
    }

    public async void StopLottery()
    {
        _decelerationCancellation?.Cancel();
        _decelerationCancellation = new CancellationTokenSource();

        try
        {
            double startSpeed = ScrollSpeed;
            double duration = 2000;
            double startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            while (ScrollSpeed > 0)
            {
                if (_decelerationCancellation.Token.IsCancellationRequested)
                    break;

                double currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                double elapsed = currentTime - startTime;
                double progress = Math.Min(elapsed / duration, 1.0);

                progress = -1.0 * progress * (progress - 2);
                ScrollSpeed = startSpeed * (1 - progress);

                if (progress >= 1.0)
                {
                    ScrollSpeed = 0;
                    break;
                }

                await Task.Delay(16);
            }

            IsAnimating = false;
            EnsureCenterItemSelected();
            double containerCenter = ContainerWidth / 2 - Left;
            double fix = containerCenter - SelectedItem!.Left - 70;
            bool dis = fix > 0;
            while (double.Abs(fix) > 1)
            {
                ScrollSpeed = dis ? -1 : 1;
                UpdateItemsPosition();
                await Task.Delay(10);
                fix += ScrollSpeed;
            }

            await Task.Delay(200);

            ShowResult = true;
            CanRun = false;
            if (SelectedItem != null)
            {
                BlockTexUtils.SetToday(SelectedItem.Key);
            }
        }
        catch (TaskCanceledException)
        {
            // 动画被取消
        }
    }

    public void UpdateItemsPosition()
    {
        //if (!IsAnimating) return;

        foreach (var itemVm in LotteryItems)
        {
            double newLeft = itemVm.Left - ScrollSpeed;

            if (newLeft + 120 < -120) // 120是物品宽度
            {
                newLeft = LotteryItems.Last().Left + 140;
            }

            itemVm.Left = newLeft;
        }
        //if (CanRun)
        //{
        //    CheckCenterItem();
        //}
    }

    //private void CheckCenterItem()
    //{
    //    if (ContainerWidth <= 0) return;

    //    double containerCenter = ContainerWidth / 2;
    //    BlockItemModel? closestItem = null;
    //    double closestDistance = double.MaxValue;

    //    foreach (var itemVm in LotteryItems)
    //    {
    //        double itemCenter = itemVm.Left + 60; // 物品宽度的一半
    //        double distance = Math.Abs(itemCenter - containerCenter);

    //        if (distance < closestDistance)
    //        {
    //            closestDistance = distance;
    //            closestItem = itemVm;
    //        }
    //    }

    //    if (closestItem != null && closestDistance <= 10) // 容差范围
    //    {
    //        SelectedItem = closestItem;
    //    }
    //}

    private void EnsureCenterItemSelected()
    {
        if (ContainerWidth <= 0) return;

        double containerCenter = ContainerWidth / 2 - Left;
        BlockItemModel? centerItem = null;
        double minDistance = double.MaxValue;

        foreach (var itemVm in LotteryItems)
        {
            double itemCenter = itemVm.Left + 60;
            double distance = Math.Abs(itemCenter - containerCenter);

            if (distance < minDistance)
            {
                minDistance = distance;
                centerItem = itemVm;
            }
        }

        if (centerItem != null)
        {
            SelectedItem = centerItem;
        }
    }

    public override void Close()
    {

    }
}
