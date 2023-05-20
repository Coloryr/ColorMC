using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Skin;

public partial class SkinModel : ObservableObject
{
    private IUserControl Con;
    public List<string> SkinTypeList { get; } = BaseBinding.GetSkinType();
    public List<string> SkinRotateList { get; } = BaseBinding.GetSkinRotateName();

    [ObservableProperty]
    private int type;
    [ObservableProperty]
    private int rotateType;

    [ObservableProperty]
    private string info;
    [ObservableProperty]
    private string text;

    [ObservableProperty]
    private bool haveSkin;
    [ObservableProperty]
    private bool enableAnimation = true;
    [ObservableProperty]
    private bool enableCape = true;
    [ObservableProperty]
    private bool enableTop = true;
    [ObservableProperty]
    private bool enableZ;

    [ObservableProperty]
    private SkinType steveModelType;

    [ObservableProperty]
    private float rotateX;
    [ObservableProperty]
    private float rotateY;
    [ObservableProperty]
    private float rotateZ;

    [ObservableProperty]
    private float minX;
    [ObservableProperty]
    private float minY = -360;
    [ObservableProperty]
    private float minZ;

    public bool IsLoad;
    public bool IsLoading;

    public Vector3 ArmRotate;
    public Vector3 LegRotate;
    public Vector3 HeadRotate;

    public SkinModel(IUserControl con)
    {
        Con = con;
    }

    partial void OnRotateXChanged(float value)
    {
        switch (RotateType)
        {
            case 0:
                ArmRotate.X = value;
                break;
            case 1:
                LegRotate.X = value;
                break;
            case 2:
                HeadRotate.X = value;
                break;
            default:
                return;
        }

        OnPropertyChanged("Rotate");
    }

    partial void OnRotateYChanged(float value)
    {
        switch (RotateType)
        {
            case 0:
                ArmRotate.Y = value;
                break;
            case 1:
                LegRotate.Y = value;
                break;
            case 2:
                HeadRotate.Y = value;
                break;
            default:
                return;
        }

        OnPropertyChanged("Rotate");
    }

    partial void OnRotateZChanged(float value)
    {
        switch (RotateType)
        {
            case 0:
                ArmRotate.Z = value;
                break;
            case 1:
                LegRotate.Z = value;
                break;
            case 2:
                HeadRotate.Z = value;
                break;
            default:
                return;
        }

        OnPropertyChanged("Rotate");
    }

    partial void OnRotateTypeChanged(int value)
    {
        Vector3 rotate;
        switch (RotateType)
        {
            case 0:
                rotate = ArmRotate;
                MinX = 0;
                MinY = -360;
                MinZ = 0;
                EnableZ = false;
                break;
            case 1:
                rotate = LegRotate;
                MinX = 0;
                MinY = -360;
                MinZ = 0;
                EnableZ = false;
                break;
            case 2:
                rotate = HeadRotate;
                MinX = -360;
                MinY = -360;
                MinZ = -360;
                EnableZ = true;
                break;
            default:
                return;
        }

        RotateX = rotate.X;
        RotateY = rotate.Y;
        RotateZ = rotate.Z;
    }

    partial void OnTypeChanged(int value)
    {
        var window = Con.Window;
        if (Type == (int)SkinType.Unkonw)
        {
            window.OkInfo.Show(App.GetLanguage("SkinWindow.Info1"));
            Type = (int)SteveModelType;
            return;
        }
        if (IsLoading == false)
        {
            SteveModelType = (SkinType)Type;
        }
    }

    [RelayCommand]
    public async void Save()
    {
        var window = Con.Window;
        var res = await BaseBinding.SaveFile(window, FileType.Skin, null);
        if (res == true)
        {
            window.NotifyInfo.Show(App.GetLanguage("Gui.Info10"));
        }
    }

    [RelayCommand]
    public void Reset()
    {
        switch (RotateType)
        {
            case 0:
                ArmRotate.X = 0;
                ArmRotate.Y = 0;
                ArmRotate.Z = 0;
                break;
            case 1:
                LegRotate.X = 0;
                LegRotate.Y = 0;
                LegRotate.Z = 0;
                break;
            case 2:
                HeadRotate.X = 0;
                HeadRotate.Y = 0;
                HeadRotate.Z = 0;
                break;
            default:
                return;
        }
        RotateX = 0;
        RotateY = 0;
        RotateZ = 0;

        OnPropertyChanged("Rotate");
    }

    [RelayCommand]
    public async void Load()
    {
        await UserBinding.LoadSkin();
    }

    [RelayCommand]
    public void Edit()
    {
        UserBinding.EditSkin(Con.Window);
    }
}
