using System.Net;
using Microsoft.Extensions.DependencyInjection;
using ProductMonitoringService.Policies;

namespace ProductMonitoringService.Extensions;

public static class AlbertHeijnStoreServices
{
    public static IServiceCollection AddAlbertHeijnStoreServices(this IServiceCollection services)
    {
        services.AddSingleton<AlbertHeijnRateLimitPolicy>();

        services.AddHttpClient("AlbertHeijnClient", c =>
            {
                c.BaseAddress = new Uri("https://ah.nl");

                // Add default headers to HttpClient
                // c.DefaultRequestHeaders.Add("Cookie","Ahonlnl-Prd-01-DigitalDev-B2=!OjoYw6DPD/8CYSiD0aCMIMm0RfdmFOuSYqNbpSwS7QZgxF4wlN2c3wuerO8o9+t8HeyewWMDMw/yNg==; Ahonlnl-Prd-01-DigitalDev-F1=!pBBnFNUK4lh8zzybSBJyxgtWB4QD3HDl6/sGPOm/F/aJ1CBcNDGjwCT5aB0X3CpWohziS7tuB8eghE0=; _abck=AB55E02D8EC438B7EAD71A3B9DB2D549~-1~YAAQnv1IF2AD0o6KAQAAB5BkmQrrKxbxF7Vq7Zr3L3YSST+ckO+7mZwwLwL44+CgVBTzrZKgj5le25WyWTfQAExzMHLxM5YqFpAzVrFtZVbgl6mVm/9d8DCP3R6VEK2669iRxgoGQ80CjIjCwAYZ855nwSHFg7fV7KFOdpxr9geHwGgzY7S6X+MYZ7oWalht20ZLvJDLu/N9Us2jEVSfMrvF1FxCXe/gU/jqXgl2Kcj6vZ6aZ7wE6WMUa9L7eC0FEu0R3DC5MlyP6vrP3HgIw8zVAhA9OlMWUVQOOaMbF9QQaajIheVGqDqk1xk8hYSbM57KoJFOVoLDwC/osIZdogDTRO9gMNFLLkydJlsmeZ3p+4RRXxurRw==~-1~-1~1694794145; bm_sz=57D1F5E69425DBD9DC648FFB9335A023~YAAQnv1IF2ED0o6KAQAAB5BkmRU1sWhz71q98A3e++qHUXqpdmEfCXkfeJBhccqA3veAWXHu1FY9PakSqta/4bGlUmcurI3FoEpgw8qyy8GxQuzNrad63eN6GaMtFUFhq/fdog8TzOUInCJyxecKkotmkLXcHy/xTB78EEEOZtoIfIORhs80TxS2tJNEwT1rQZs4GL/UZb623eid14MDntT0wPn7cNylDlo0GZx5d5xPkEX9vLW1ZaYgxvudwxZ5bCFfpIHq8EeANEjKKvnvVHk6bO7tfixEMstrNTzq~3294773~3360306; SSLB=1; SSID=CQAVtx1wAAAAAADbcwRlNvOAAdtzBGUBAAAAAAAH3MZo23MEZQBUmkYJAAODugAA23MEZQEAqwoAAefQAADbcwRlAQCyCgABN9EAANtzBGUBAKcKAAPO0AAA23MEZQEAuQoAAVTRAADbcwRlAQCQCgAB488AANtzBGUBALwKAAFn0QAA23MEZQEABAcAAx2VAADbcwRlAQA; SSSC=4.G7279070282197824310.1|1796.38173:2374.47747:2704.53219:2727.53454:2731.53479:2738.53559:2745.53588:2748.53607; SSRT=23MEZQABAA; SSPV=CngAAAAAAAgABgAAAAAAAAAAAAIAAAAAAAAAAAAA; _csrf=MUzo5CzlDgh7Z_OkEARTN9O0; i18next=nl-nl; ak_bmsc=0FA36E94BF1A298DA5E88437EEC7FAD3~000000000000000000000000000000~YAAQnv1IF50D0o6KAQAAHJFkmRWs5tG8I+B4Am3Qt8HnRBTduwF5dwVugwHVEkpgvy2J0JZRLZBzqnvRfsn3s6UAfVMeHeF6o7e8kV34sGoxA8yqe2woos5RD2noal5vo3c9YJnS7O+u2lu2wFzpdGQnwR7nskLdDasKBqRFAHdTcMwtTzuHfOdtQtWi5gREnaFyXlC4/rP4Ri16fqnC24i7wkKCSgh2otWDMW+VNCRAtHS8fcY7pM6MJS0rH0CEITw5Qfzyc9+jXBrONJ3zY2XpssoUOfZ01GnD2tauNUkM0SmFbYnnELLaz2e+KnHsEfDtlrvbSt39Bt98pYUD3iZIJ5vzXjUlouMtpXEnJhUSlvtDvEMaJADuHB9RkAjQM3oule+E9TY=; _abck=AB55E02D8EC438B7EAD71A3B9DB2D549~0~YAAQnv1IF7cE0o6KAQAAyJZkmQqhNO/bp93zAPOO+dZSuQYXwL6wD52PF8hPZUTkkQGDwBa60y9T2SUNvFooCHxFe4Pjc5letZHMwrGR66//CP4ouBjxgGezbyphqO4nPgtmNaU/bJ0gV6Fc1St3xk63ODs6SAaDIk3AaXgD9Rci1unl/xtfQX+9SwzqxJiTrltQ9QEn12XNaGBtS4yT0uHgBDOez0CEOBgFbPfkRr6vsG+hdc4rXE4+quSfdTF5NBq7Yo25A1TT1g7i85kJ1Rz0IK3y7U1/Bvs6lIeXYLiRl+Vt91FZGojw/mM6dm2ns9lcX1H5fZuBMzHlerSi0FJ4DNezE91tlKDdPT3CMrC5jaVbQBGGDhGZQ9VeWQYyCVO7UL+Dl1iazvAsd/pyOpYd0YCbjeDy~-1~||1-kNnSTDBRzX-1-10-1000-2||~1694794114; bm_sv=5EE156AA4F73F14D6284A5FBEE14F416~YAAQnv1IFwgG0o6KAQAAXJ5kmRV3tbaLt2LlM4tsaxAoFhqG9p8u7LmxyJdKjGSrObi6wmptTnXpzu2gcV0IljIMB4oNrHdhEk4F9QWvNk5FZxScmQIXLvIa8+r33Ea7wAqVLw/eyIR496DJ0J8tIf5zB2mf9jG0MV/ESQJCGPvcrMxor4N1ihNI97txKTuFYCCCnb7ygRzRZb2b1eJ3RS8MqBZqq4vSQxgjlxxz+Ef6inI2Zp7mEs5kMD/ffB4=~1");
                c.DefaultRequestHeaders.Add("Host", "www.ah.nl");
                c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:109.0) Gecko/20100101 Firefox/117.0");
                c.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
                c.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                c.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                c.DefaultRequestHeaders.Add("Connection", "keep-alive");
                c.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                c.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                c.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                c.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
                c.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                c.DefaultRequestHeaders.Add("DNT", "1");
                c.DefaultRequestHeaders.Add("Sec-GPC", "1");
                c.DefaultRequestHeaders.Add("TE", "trailers");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            .AddHttpMessageHandler<AlbertHeijnRateLimitPolicy>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        return services;
    }
}