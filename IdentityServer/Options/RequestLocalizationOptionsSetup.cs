using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace IdenitityServer.Options;

public class RequestLocalizationOptionsSetup()
: IConfigureOptions<RequestLocalizationOptions>
{
    public void Configure(RequestLocalizationOptions options)
    {

        options.SupportedCultures = [
            new("en") { DateTimeFormat = { Calendar = new GregorianCalendar() } },
            new("ar") { DateTimeFormat = { Calendar = new GregorianCalendar() } }
            ];
        
        options.DefaultRequestCulture = new RequestCulture("en");
    }
}
