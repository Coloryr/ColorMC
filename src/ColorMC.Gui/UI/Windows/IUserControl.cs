namespace ColorMC.Gui.UI.Windows;

public interface IUserControl
{
    public IBaseWindow Window { get; }
    virtual public void Opened() { }
    virtual public void Closed() { }
    virtual public void Update() { }
    virtual public void Closing() { }
}
