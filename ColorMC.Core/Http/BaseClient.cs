using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Http;

public class BaseClient
{
    public HttpClient Client { get; init; }

    public BaseClient() 
    {
        Client = new();
    }
}
