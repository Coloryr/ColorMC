using ColorMC.Core.Game;
using ColorMC.Core.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel
{
    /// <summary>
    /// 自定义文本
    /// </summary>
    [ObservableProperty]
    private string _text;

    /// <summary>
    /// 是否在加载配置信息
    /// </summary>
    private bool _loadConfig;

    partial void OnTextChanged(string value)
    {
        SaveConfig();
    }

    /// <summary>
    /// 加载配置信息
    /// </summary>
    public void LoadConfig()
    {
        _loadConfig = true;
        Text = Obj.Text;
        _loadConfig = false;
    }

    /// <summary>
    /// 保存配置信息
    /// </summary>
    private void SaveConfig()
    {
        if (_loadConfig)
        {
            return;
        }

        Obj.Save();
    }
}
