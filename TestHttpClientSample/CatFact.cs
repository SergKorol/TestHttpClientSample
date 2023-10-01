using Newtonsoft.Json;

namespace TestHttpClientSample;

public record CatFact
{
    [JsonProperty("fact")]
    public string Fact { get; set; }

    [JsonProperty("length")]
    public int Length { get; set; }
}