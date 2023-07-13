namespace ColorMC.Core.Objs;

public class LibVersionObj
{
    public string Pack { get; set; }
    public string Name { get; set; }
    public string Verison { get; set; }
    public string Extr { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not LibVersionObj obj1)
            return false;

        return Pack == obj1.Pack && Name == obj1.Name && Verison == obj1.Verison && Extr == obj1.Extr;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
