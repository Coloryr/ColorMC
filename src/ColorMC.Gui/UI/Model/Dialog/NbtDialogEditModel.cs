using System.Collections.ObjectModel;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class NbtDialogEditModel(BaseModel model, string usename) : ObservableObject
{
    [ObservableProperty]
    private bool _displayEdit;
    [ObservableProperty]
    private string _dataType;
    [ObservableProperty]
    private NbtDataItemModel _dataItem;
    [ObservableProperty]
    private bool _hexEdit;

    public ObservableCollection<NbtDataItemModel> DataList { get; init; } = [];

    partial void OnHexEditChanged(bool value)
    {
        foreach (var item in DataList)
        {
            item.ChangeHex(value);
        }
    }

    [RelayCommand]
    public void DataEditDone()
    {
        DialogHost.Close(usename);
    }

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

    public void DataEdit()
    {
        try
        {
            if (DataType == "Byte")
            {
                DataItem.Value = (byte)DataItem.GetValue();
            }
            else if (DataType == "Int")
            {
                DataItem.Value = (int)DataItem.GetValue();
            }
            else if (DataType == "Long")
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
            if (DataType == "Byte")
            {
                DataList.Add(new(0, (byte)0, HexEdit));
            }
            else if (DataType == "Int")
            {
                DataList.Add(new(0, 0, HexEdit));
            }
            else if (DataType == "Long")
            {
                DataList.Add(new(0, (long)0, HexEdit));
            }
        }
    }
}
