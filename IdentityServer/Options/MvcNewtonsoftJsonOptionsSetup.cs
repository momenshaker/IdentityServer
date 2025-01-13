using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IdenitityServer.Options;

public class MvcNewtonsoftJsonOptionsSetup()
: IConfigureOptions<MvcNewtonsoftJsonOptions>
{
    public void Configure(MvcNewtonsoftJsonOptions options)
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    }
}
