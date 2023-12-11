using ColorMC.Core.Nbt;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class NbtDialogAddModel(string usename) : ObservableObject
{
    public string[] TypeSource { get; init; } = LanguageBinding.GetNbtName();

    public bool Cancel { get; set; }

    [ObservableProperty]
    private string _title;
    [ObservableProperty]
    private string _title1;
    [ObservableProperty]
    private string? _key;

    [ObservableProperty]
    private bool _displayType;

    [ObservableProperty]
    private NbtType _type = NbtType.NbtString;

    [RelayCommand]
    public void AddConfirm()
    {
        Cancel = false;
        DialogHost.Close(usename);
    }

    [RelayCommand]
    public void AddCancel()
    {
        Cancel = true;
        DialogHost.Close(usename);
    }
}
