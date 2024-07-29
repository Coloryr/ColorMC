using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Helpers;

public static class UserHelper
{
    public static UserKeyObj GetKey(this LoginObj login)
    {
        return new() { UUID = login.UUID, Type = login.AuthType };
    }
}
