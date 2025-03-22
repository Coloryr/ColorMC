using System.Collections.ObjectModel;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// Nbt标签修改
/// </summary>
/// <param name="model">窗口</param>
/// <param name="usename">窗口Id</param>
public partial class NbtDialogEditModel(BaseModel model, string usename) : ObservableObject
{
    /// <summary>
    /// 是否显示修改
    /// </summary>
    [ObservableProperty]
    private bool _displayEdit;
    /// <summary>
    /// 数据类型
    /// </summary>
    [ObservableProperty]
    private string _dataType;
    /// <summary>
    /// Nbt项目
    /// </summary>
    [ObservableProperty]
    private NbtDataItemModel _dataItem;
    /// <summary>
    /// 是否为Hex修改
    /// </summary>
    [ObservableProperty]
    private bool _hexEdit;

    /// <summary>
    /// 数据列表
    /// </summary>
    public ObservableCollection<NbtDataItemModel> DataList { get; init; } = [];

    /// <summary>
    /// HEX模式切换
    /// </summary>
    /// <param name="value"></param>
    partial void OnHexEditChanged(bool value)
    {
        foreach (var item in DataList)
        {
            item.ChangeHex(value);
        }
    }

    /// <summary>
    /// 数据编辑完毕
    /// </summary>
    [RelayCommand]
    public void DataEditDone()
    {
        DialogHost.Close(usename);
    }

    /// <summary>
    /// 删除一个Nbt项目
    /// </summary>
    /// <param name="item">Nbt项目</param>
    public void DeleteItem(NbtDataItemModel item)
    {
        if (item.Key == 0)
        {
            return;
        }

        DataList.Remove(item);
        int a = 1;
        foreach (var item1 in DataList)
        {
            if (item1.Key == 0)
            {
                continue;
            }
            item1.Key = a++;
        }
    }

    /// <summary>
    /// 数据修改
    /// </summary>
    public void DataEdit()
    {
        try
        {
            if (DataType is GuiNames.NameTypeByte)
            {
                DataItem.Value = (byte)DataItem.GetValue();
            }
            else if (DataType is GuiNames.NameTypeInt)
            {
                DataItem.Value = (int)DataItem.GetValue();
            }
            else if (DataType is GuiNames.NameTypeLong)
            {
                DataItem.Value = (long)DataItem.GetValue();
            }
        }
        catch
        {
            model.Show(App.Lang("ConfigEditWindow.Error3"));
            DataItem.Value = 0;
            return;
        }

        if (DataItem.Key == 0)
        {
            DataItem.Key = DataList.Count;
            if (DataType is GuiNames.NameTypeByte)
            {
                DataList.Add(new(0, (byte)0, HexEdit));
            }
            else if (DataType is GuiNames.NameTypeInt)
            {
                DataList.Add(new(0, 0, HexEdit));
            }
            else if (DataType is GuiNames.NameTypeLong)
            {
                DataList.Add(new(0, (long)0, HexEdit));
            }
        }
    }
}
