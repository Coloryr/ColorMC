//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ColorMC.Core.Objs.MinecraftAPI;

//namespace ColorMC.Gui.Utils;

//public static class NewsUtils
//{
//    public static bool IsNews(this MinecraftNewObj.ResultObj.ResultsObj obj)
//    {
//        if (obj.Type == "Game")
//        {
//            return false;
//        }
//        if (obj.Url is "https://www.minecraft.net/zh-hans/download"
//            or "https://www.minecraft.net/zh-hans/store/legends-deluxe-edition"
//            or "https://www.minecraft.net/zh-hans/store/legends-standard-edition"
//            or "https://www.minecraft.net/zh-hans/store/legends-deluxe-skin-pack"
//            or "https://www.minecraft.net/zh-hans/catalog"
//            or "https://www.minecraft.net/zh-hans/marketplace")
//        {
//            return false;
//        }

//        return true;
//    }
//}
