using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model;

public interface ILoadFuntion<T>
{
    void SetSelect(T item);
    Task Load();
}
