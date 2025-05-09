using System.Text;
using Avalonia.Input;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Setting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 手柄按钮
/// </summary>
/// <param name="setting"></param>
public partial class InputButtonModel(SettingModel setting) : ObservableObject
{
    /// <summary>
    /// 按钮
    /// </summary>
    [ObservableProperty]
    private byte _inputKey;

    /// <summary>
    /// 显示绑定的按钮
    /// </summary>
    public string Bind => Make();
    /// <summary>
    /// 图标
    /// </summary>
    public string Icon => IconConverter.GetInputKeyIcon(InputKey);

    /// <summary>
    /// 设置的按钮
    /// </summary>
    public InputKeyObj Obj;

    /// <summary>
    /// 开始设置
    /// </summary>
    public void Setting()
    {
        setting.SetKeyButton(this);
    }

    /// <summary>
    /// 删除按钮
    /// </summary>
    public void Delete()
    {
        setting.DeleteInput(this);
    }

    /// <summary>
    /// 创建按钮显示
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 更新按钮显示
    /// </summary>
    public void Update()
    {
        OnPropertyChanged(nameof(Bind));
    }
}
