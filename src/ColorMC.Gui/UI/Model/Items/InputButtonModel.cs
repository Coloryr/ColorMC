using System.Text;
using Avalonia.Input;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class InputButtonModel(SettingModel setting) : ObservableObject
{
    [ObservableProperty]
    private byte _inputKey;

    public string Bind => Make();
    public string Icon => IconConverter.GetInputKeyIcon(InputKey);

    public InputKeyObj Obj;

    public void Setting()
    {
        setting.SetKeyButton(this);
    }

    public void Delete()
    {
        setting.DeleteInput(this);
    }

    private string Make()
    {
        if (Obj == null)
        {
            return "";
        }

        var builder = new StringBuilder();

        if (Obj.KeyModifiers == KeyModifiers.Alt)
        {
            builder.Append("Alt + ");
        }
        else if (Obj.KeyModifiers == KeyModifiers.Control)
        {
            builder.Append("Ctrl + ");
        }
        else if (Obj.KeyModifiers == KeyModifiers.Shift)
        {
            builder.Append("Shift + ");
        }

        if (Obj.Key != Key.None)
        {
            builder.Append("Keyboard ").Append(Obj.Key.ToString());
        }
        else if (Obj.MouseButton != MouseButton.None)
        {
            builder.Append("Mouse ").Append(Obj.MouseButton.ToString());
        }

        return builder.ToString();
    }

    public void Update()
    {
        OnPropertyChanged(nameof(Bind));
    }
}
