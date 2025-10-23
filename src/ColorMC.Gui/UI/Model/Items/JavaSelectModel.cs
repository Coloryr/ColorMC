namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 可选中Java列表项目
/// </summary>
public partial class JavaSelectModel : JavaDisplayModel
{
    /// <summary>
    /// 是否选中
    /// </summary>
    public bool IsSelect { get; set; }

    public JavaSelectModel(JavaDisplayModel model)
    {
        Name = model.Name;
        Path = model.Path;
        MajorVersion = model.MajorVersion;
        Version = model.Version;
        Type = model.Type;
        Arch = model.Arch;
    }
}
