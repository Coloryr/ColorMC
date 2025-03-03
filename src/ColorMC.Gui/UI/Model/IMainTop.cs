using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model;

public interface IMainTop
{
    /// <summary>
    /// 启动游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    void Launch(GameItemModel obj);
    /// <summary>
    /// 启动游戏实例
    /// </summary>
    /// <param name="list">游戏实例列表</param>
    void Launch(ICollection<GameItemModel> list);
    /// <summary>
    /// 启动游戏实例
    /// </summary>
    /// <param name="list">游戏实例UUID列表</param>
    void Launch(ICollection<string> list);
    /// <summary>
    /// 选中一个游戏实例
    /// </summary>
    /// <param name="model">游戏实例</param>
    void Select(GameItemModel? model);
    /// <summary>
    /// 编辑一个游戏实例分组
    /// </summary>
    /// <param name="model">游戏实例</param>
    void EditGroup(GameItemModel model);
    /// <summary>
    /// 编辑一个游戏实例星标
    /// </summary>
    /// <param name="model">游戏实例</param>
    void DoStar(GameItemModel model);
    /// <summary>
    /// 关闭筛选
    /// </summary>
    void SearchClose();
    /// <summary>
    /// 获取游戏实例
    /// </summary>
    /// <param name="uuid">游戏实例UUID</param>
    /// <returns>游戏实例</returns>
    GameItemModel? GetGame(string uuid);
    /// <summary>
    /// 导出游戏实例启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    void ExportCmd(GameSettingObj obj);
}

public interface IMutTop : IMainTop
{
    /// <summary>
    /// 是否多选
    /// </summary>
    bool IsMut { get; }
    /// <summary>
    /// 开始多选
    /// </summary>
    void StartMut();
    /// <summary>
    /// 开始多选
    /// </summary>
    /// <param name="model">选中的游戏分组</param>
    void StartMut(GameGroupModel model);
    /// <summary>
    /// 结束多选
    /// </summary>
    /// <returns>选中的游戏实例</returns>
    List<GameItemModel> EndMut();
    /// <summary>
    /// 获取多选
    /// </summary>
    /// <returns>选中的游戏实例</returns>
    List<GameItemModel> GetMut();
    /// <summary>
    /// 多选启动
    /// </summary>
    void MutLaunch();
    /// <summary>
    /// 多选编辑
    /// </summary>
    void MutEdit();
    /// <summary>
    /// 多选编辑分组
    /// </summary>
    void MutEditGroup();
    /// <summary>
    /// 多选删除
    /// </summary>
    void MutDelete();
}