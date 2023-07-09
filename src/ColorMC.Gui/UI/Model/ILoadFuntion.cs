using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model;

public interface ILoadFuntion<T>
{
    void SetSelect(T item);
    Task Load();
}
