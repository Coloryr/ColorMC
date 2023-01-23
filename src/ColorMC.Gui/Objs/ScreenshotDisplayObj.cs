using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record ScreenshotDisplayObj
{
    public string Name { get; set; }
    public Bitmap Image { get; set; }

    public string Local { get; set; }
}
