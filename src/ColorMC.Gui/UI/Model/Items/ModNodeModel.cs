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

    /// <summary>
    /// 更新分组信息
    /// </summary>
    public void UpdateGroup()
    {
        if (IsGroup)
        {
            bool havedis = false;
            bool haveena =false;
            foreach (var item in Children)
            {
                if (item.Enable == true)
                {
                    haveena = true;
                }
                else if (item.Enable == false)
                {
                    havedis = true;
                }
            }

            if (havedis && haveena)
            {
                Enable = null;
            }
            else if (haveena)
            {
                Enable = true;
            }
            else
            {
                Enable = false;
            }
        }
    }
}
