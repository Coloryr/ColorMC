using ColorMC.Gui.Objs;

namespace ColorMC.Gui.Skin;

/// <summary>
/// 生成史蒂夫模型
/// </summary>
public class Steve3DModel
{
    private readonly CubeModel _cube = new();
    /// <summary>
    /// 生成一个模型
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public SteveModelObj GetSteve(SkinType type)
    {
        return new()
        {
            Head = new()
            {
                Model = _cube.GetSquare(),
                Point = _cube.GetSquareIndicies()
            },
            Body = new()
            {
                Model = _cube.GetSquare(multiplyZ: 0.5f, multiplyY: 1.5f),
                Point = _cube.GetSquareIndicies()
            },
            LeftArm = type == SkinType.NewSlim ? new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.375f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = _cube.GetSquareIndicies()
            } : new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = _cube.GetSquareIndicies()
            },
            RightArm = type == SkinType.NewSlim ? new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.375f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = _cube.GetSquareIndicies()
            } : new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = _cube.GetSquareIndicies()
            },
            LeftLeg = new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = _cube.GetSquareIndicies()
            },
            RightLeg = new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f
                ),
                Point = _cube.GetSquareIndicies()
            },
            Cape = new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 1.25f,
                    multiplyZ: 0.1f,
                    multiplyY: 2f
                ),
                Point = _cube.GetSquareIndicies()
            }
        };
    }

    /// <summary>
    /// 生成第二层模型
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns></returns>
    public SteveModelObj GetSteveTop(SkinType type)
    {
        var model = new SteveModelObj
        {
            Head = new()
            {
                Model = _cube.GetSquare(
                    enlarge: 1.125f
                ),
                Point = _cube.GetSquareIndicies()
            }
        };

        if (type != SkinType.Old)
        {
            model.Body = new()
            {
                Model = _cube.GetSquare(
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = _cube.GetSquareIndicies()
            };

            model.LeftArm = type == SkinType.NewSlim ? new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.375f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = _cube.GetSquareIndicies()
            } : new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = _cube.GetSquareIndicies()
            };

            model.RightArm = type == SkinType.NewSlim ? new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.375f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = _cube.GetSquareIndicies()
            } : new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = _cube.GetSquareIndicies()
            };

            model.LeftLeg = new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = _cube.GetSquareIndicies()
            };
            model.RightLeg = new()
            {
                Model = _cube.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    enlarge: 1.125f
                ),
                Point = _cube.GetSquareIndicies()
            };
        }

        return model;
    }
}
