using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public class EmptyBlock(bool last, BlockType type, int size) : FlacInfoBlock(last, type, size)
{

}
