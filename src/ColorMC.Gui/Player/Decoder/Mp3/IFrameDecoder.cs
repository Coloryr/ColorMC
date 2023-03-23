using Avalonia.Controls.Models.TreeDataGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public interface IFrameDecoder
{
    /**
     * Decodes one frame of MPEG audio.
     */
    void DecodeFrame();
}
