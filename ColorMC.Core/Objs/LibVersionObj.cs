namespace ColorMC.Core.Objs;

public class LibVersionObj
{
    public string Pack { get; set; }
    public string Name { get; set; }
    public string Verison { get; set; }
    public string Extr { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not LibVersionObj)
            return false;

        var obj1 = obj as LibVersionObj;

        return Pack == obj1.Pack && Name == obj1.Name && Extr == obj1.Extr;
    }
}
