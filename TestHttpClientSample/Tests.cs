using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Xunit;

namespace TestHttpClientSample;

public class Tests
{
    [Fact]
    public async Task CatFact_ReturnsNotNullAndNotEmpty()
    {
        await using var clientMock = new HttpClientMock();
        var expectedFact = new CatFact { Fact = "Cats are funny", Length = 14 };
        clientMock.Application.MapGet("/fact", () => expectedFact ).RequireHost("catfact.ninja").AllowAnonymous();
        
        using var httpClient = clientMock.CreateHttpClient();
        var response = await httpClient.GetAsync("https://catfact.ninja/fact");
        var fact = await response.Content.ReadFromJsonAsync<CatFact>();
    
        Assert.NotNull(fact);
        Assert.Equal(expectedFact.Length, fact.Length);
        Assert.Equal(expectedFact.Fact, fact.Fact);
    }
    
    [Theory]
    [InlineData(14)]
    [InlineData(13)]
    [InlineData(12)]
    public async Task CatFact_ReturnsLimitedByLengthResult(int maxLimit)
    {
        await using var clientMock = new HttpClientMock();
        var expectedFact = new CatFact { Fact = "Cats are funny", Length = 14 };
        clientMock.Application.MapGet("/fact",
                async context => {
                    if (int.TryParse(context.Request.Query["max_length"], out int limit))
                    {
                        if (expectedFact.Length == limit)
                        {
                            var json = JsonConvert.SerializeObject(expectedFact);
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            await context.Response.WriteAsync(json);
                        }
                        else
                        {
                            var json = JsonConvert.SerializeObject(default(CatFact));
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            await context.Response.WriteAsync(json);
                        }
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(expectedFact);
                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        await context.Response.WriteAsync(json);
                    }
                })
            .RequireHost("catfact.ninja");
        
        using var httpClient = clientMock.CreateHttpClient();
        HttpResponseMessage? response;
        if (maxLimit == 12)
        {
            var notValidQueryParam = "string";
            response = await httpClient.GetAsync($"https://catfact.ninja/fact?max_length={notValidQueryParam}");
        }
        else
        {
            response = await httpClient.GetAsync($"https://catfact.ninja/fact?max_length={maxLimit}");
        }
        var fact = await response.Content.ReadFromJsonAsync<CatFact>();
        if (maxLimit >= expectedFact.Length)
        {
            Assert.NotEqual(default, fact);
            Assert.Equal(expectedFact.Length, fact.Length);
            Assert.Equal(expectedFact.Fact, fact.Fact);
        }
        else if (maxLimit < expectedFact.Length && maxLimit != 12)
        {
            Assert.Equal(default, fact);
        }
        else if (maxLimit == 12)
        {
            Assert.NotEqual(default, fact);
            Assert.Equal(expectedFact.Length, fact.Length);
            Assert.Equal(expectedFact.Fact, fact.Fact);
        }
    }
}