using System.Collections.ObjectModel;
using System.Linq;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.GameEdit;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class ModNodeModel : ModDisplayModel
{
    /// <summary>
    /// 分类下的文件
    /// </summary>
    public ObservableCollection<ModNodeModel> Children { get; init; } = [];

    /// <summary>
    /// 是否有子项目
    /// </summary>
    public bool HasChildren => Children.Count > 0;
    /// <summary>
    /// 是否展开
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded;
    /// <summary>
    /// 分组名字
    /// </summary>
    [ObservableProperty]
    private string _group;

    public ModNodeModel(ModObj obj, ModInfoObj? obj1, GameEditModel? top) : base(obj, obj1, top)
    {

    }

    /// <summary>
    /// 分组
    /// </summary>
    /// <param name="group"></param>
    public ModNodeModel(string group) : base()
    {
        _group = group;
    }

    public void Update()
    {
        if (IsGroup)
        {
            Enable = !Children.Any(x => x.Enable != true);
        }
    }
}
