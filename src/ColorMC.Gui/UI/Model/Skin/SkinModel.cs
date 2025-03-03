using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinecraftSkinRender;
using System.Numerics;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Skin;

public partial class SkinModel(BaseModel model) : TopModel(model)
{
    public const string RotateName = "Rotate";
    public const string ScollName = "Scoll";
    public const string PosName = "Pos";
    public const string RotName = "Rot";
    public const string LoadName = "Load";
    public const string ResetName = "Reset";

    public string[] SkinTypeList { get; init; } = LanguageBinding.GetSkinType();
    public string[] SkinRotateList { get; init; } = LanguageBinding.GetSkinRotateName();

    [ObservableProperty]
    private int _type;
    [ObservableProperty]
    private int _rotateType;

    [ObservableProperty]
    private string _info;
    [ObservableProperty]
    private string text;
    [ObservableProperty]
    private string _nowFps;

    [ObservableProperty]
    private bool _haveSkin;
    [ObservableProperty]
    private bool _enableAnimation = true;
    [ObservableProperty]
    private bool _enableCape = true;
    [ObservableProperty]
    private bool _enableTop = true;
    [ObservableProperty]
    private bool _enableZ;
    [ObservableProperty]
    private bool _enableMSAA = false;

    [ObservableProperty]
    private SkinType _steveModelType;

    [ObservableProperty]
    private float _rotateX;
    [ObservableProperty]
    private float _rotateY;
    [ObservableProperty]
    private float _rotateZ;

    [ObservableProperty]
    private float _minX;
    [ObservableProperty]
    private float _minY = -360;
    [ObservableProperty]
    private float _minZ;

    public Vector3 ArmRotate;
    public Vector3 LegRotate;
    public Vector3 HeadRotate;

    public float X;
    public float Y;

    private bool _load;

    public int Fps
    {
        set
        {
            NowFps = $"{value}Fps";
        }
    }

    partial void OnSteveModelTypeChanged(SkinType value)
    {
        _load = true;
        Type = (int)value;
        _load = false;
    }

    partial void OnTypeChanged(int value)
    {
        if (_load)
        {
            return;
        }
        SteveModelType = (SkinType)value;
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

        OnPropertyChanged(RotateName);
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

        OnPropertyChanged(RotateName);
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

        OnPropertyChanged(RotateName);
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

    [RelayCommand]
    public void Move(object comm)
    {
        switch (comm)
        {
            case MoveType.LeftUp:
                X = -0.05f;
                Y = 0.05f;
                break;
            case MoveType.Up:
                X = 0;
                Y = 0.05f;
                break;
            case MoveType.RightUp:
                X = 0.05f;
                Y = 0.05f;
                break;
            case MoveType.Left:
                X = -0.05f;
                Y = 0;
                break;
            case MoveType.Right:
                X = 0.05f;
                Y = 0;
                break;
            case MoveType.LeftDown:
                X = -0.05f;
                Y = -0.05f;
                break;
            case MoveType.Down:
                X = 0;
                Y = -0.05f;
                break;
            case MoveType.RightDown:
                X = 0.05f;
                Y = -0.05f;
                break;
        }
        OnPropertyChanged(PosName);
    }

    [RelayCommand]
    public void Rot(object comm)
    {
        switch (comm)
        {
            case MoveType.LeftUp:
                X = -10;
                Y = -10;
                break;
            case MoveType.Up:
                X = -10;
                Y = 0;
                break;
            case MoveType.RightUp:
                X = -10;
                Y = 10;
                break;
            case MoveType.Left:
                X = 0;
                Y = -10;
                break;
            case MoveType.Right:
                X = 0;
                Y = 10;
                break;
            case MoveType.LeftDown:
                X = 10;
                Y = -10;
                break;
            case MoveType.Down:
                X = 10;
                Y = 0;
                break;
            case MoveType.RightDown:
                X = 10;
                Y = 10;
                break;
        }
        OnPropertyChanged("Rot");
    }

    [RelayCommand]
    public void Scoll(object comm)
    {
        switch (comm)
        {
            case MoveType.Up:
                X = 0.05f;
                break;
            case MoveType.Down:
                X = -0.05f;
                break;
        }

        OnPropertyChanged(ScollName);
    }

    [RelayCommand]
    public async Task Save()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.SaveFile(top, FileType.Skin, null);
        if (res == true)
        {
            Model.Notify(App.Lang("ConfigEditWindow.Info9"));
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

        OnPropertyChanged(RotateName);
    }

    [RelayCommand]
    public async Task Load()
    {
        await UserBinding.LoadSkin();

        OnPropertyChanged(LoadName);
    }

    [RelayCommand]
    public void Edit()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        UserBinding.EditSkin(top);
    }

    [RelayCommand]
    public void ResetPos()
    {
        OnPropertyChanged(ResetName);
    }

    public override void Close()
    {

    }
}
