using System.Collections.Generic;

namespace ColorMC.Gui.Skin.Model;

public class SteveC
{
    public static (float[], ushort[]) GetSteve(SkinType modelType)
    {
        var steveCoords = new List<float>();
        var steveIndicies = CubeC.GetCubeIndicies(6);

        // Head
        steveCoords.AddRange(CubeC.GetSquare(addY: CubeC.Value * 2.5f));

        // Torso
        steveCoords.AddRange(CubeC.GetSquare(multiplyZ: 0.5f, multiplyY: 1.5f));

        if (modelType == SkinType.New_Slim)
        {
            // Left Arm
            steveCoords.AddRange(CubeC.GetSquare(
                multiplyX: 0.375f,
                multiplyZ: 0.5f,
                multiplyY: 1.5f,
                addX: 1.375f * CubeC.Value
            ));

            // Right Arm
            steveCoords.AddRange(CubeC.GetSquare(
                multiplyX: 0.375f,
                multiplyZ: 0.5f,
                multiplyY: 1.5f,
                addX: -1.375f * CubeC.Value
            ));
        }
        else
        {
            // Left Arm
            steveCoords.AddRange(CubeC.GetSquare(
                multiplyX: 0.5f,
                multiplyZ: 0.5f,
                multiplyY: 1.5f,
                addX: 1.5f * CubeC.Value
            ));

            // Right Arm
            steveCoords.AddRange(CubeC.GetSquare(
                multiplyX: 0.5f,
                multiplyZ: 0.5f,
                multiplyY: 1.5f,
                addX: -1.5f * CubeC.Value
            ));
        }

        // Left Leg
        steveCoords.AddRange(CubeC.GetSquare(
            multiplyX: 0.5f,
            multiplyZ: 0.5f,
            multiplyY: 1.5f,
            addX: CubeC.Value * 0.5f,
            addY: -CubeC.Value * 3f
        ));

        // Right Leg
        steveCoords.AddRange(CubeC.GetSquare(
            multiplyX: 0.5f,
            multiplyZ: 0.5f,
            multiplyY: 1.5f,
            addX: -CubeC.Value * 0.5f,
            addY: -CubeC.Value * 3f
        ));

        return (steveCoords.ToArray(), steveIndicies);
    }

    public static (float[], ushort[]) GetSteveTop(SkinType modelType)
    {
        var steveCoords = new List<float>();
        var steveIndicies =
            CubeC.GetCubeIndicies(modelType == SkinType.Old ? 1 : 6);

        // Hat
        steveCoords.AddRange(CubeC.GetSquare(
            addY: CubeC.Value * 2.5f,
            enlarge: 1.125f
        ));

        if (modelType != SkinType.Old)
        {
            // Torso 2nd layer
            steveCoords.AddRange(CubeC.GetSquare(
                multiplyZ: 0.5f,
                multiplyY: 1.5f,
                enlarge: 1.125f
            ));

            if (modelType == SkinType.New_Slim)
            {
                // Left Arm 2nd Layer
                steveCoords.AddRange(CubeC.GetSquare(
                    multiplyX: 0.375f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    addX: 1.375f * CubeC.Value,
                    enlarge: 1.125f
                ));

                // Right Arm 2nd Layer
                steveCoords.AddRange(CubeC.GetSquare(
                    multiplyX: 0.375f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    addX: -1.375f * CubeC.Value,
                    enlarge: 1.125f
                ));
            }
            else
            {
                // Left Arm 2nd Layer
                steveCoords.AddRange(CubeC.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    addX: 1.5f * CubeC.Value,
                    enlarge: 1.125f
                ));

                // Right Arm 2nd Layer
                steveCoords.AddRange(CubeC.GetSquare(
                    multiplyX: 0.5f,
                    multiplyZ: 0.5f,
                    multiplyY: 1.5f,
                    addX: -1.5f * CubeC.Value,
                    enlarge: 1.125f
                ));
            }

            // Left Leg 2nd Layer
            steveCoords.AddRange(CubeC.GetSquare(
                multiplyX: 0.5f,
                multiplyZ: 0.5f,
                multiplyY: 1.5f,
                addX: CubeC.Value * 0.5f,
                addY: -CubeC.Value * 3f,
                enlarge: 1.125f
            ));

            // Right Leg 2nd Layer
            steveCoords.AddRange(CubeC.GetSquare(
                multiplyX: 0.5f,
                multiplyZ: 0.5f,
                multiplyY: 1.5f,
                addX: -CubeC.Value * 0.5f,
                addY: -CubeC.Value * 3f,
                enlarge: 1.125f
            ));
        }

        return (steveCoords.ToArray(), steveIndicies);
    }
}
