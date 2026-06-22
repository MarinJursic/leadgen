using System.Text.Json;
using LeadGen.Core.Providers;
using LeadGen.Infrastructure.Providers;

namespace LeadGen.Tests;

public sealed class DeepSeekJsonParsingTests
{
    [Fact]
    public void ExtractJsonPayload_RemovesFencesAndTrailingText()
    {
        var payload = DeepSeekAiClient.ExtractJsonPayload("""
        ```json
        {"summary":"ok","targetIndustries":[]}
        ```
        extra text
        """);

        Assert.Equal("""{"summary":"ok","targetIndustries":[]}""", payload);
    }

    [Fact]
    public void DeserializePayload_SearchPlan_AcceptsTopLevelArray()
    {
        var plan = DeepSeekAiClient.DeserializePayload<SearchPlan>("""
        [
          {"query":"dental clinic Croatia contact","purpose":"Find private clinics"}
        ]
        """);

        var query = Assert.Single(plan.Queries);
        Assert.Equal("dental clinic Croatia contact", query.Query);
        Assert.Equal("Find private clinics", query.Purpose);
    }

    [Fact]
    public void DeserializePayload_IcpProfile_AcceptsFencedJson()
    {
        var profile = DeepSeekAiClient.DeserializePayload<IcpProfile>("""
        ```json
        {
          "summary": "Private clinics in Croatia",
          "targetIndustries": ["Dental clinics"],
          "targetLocations": ["Croatia"],
          "buyerTypes": ["Practice owners"],
          "painPoints": ["Low website conversion"],
          "positiveSignals": ["Appointment request page"],
          "negativeSignals": ["Login-gated only"],
          "searchKeywords": ["dental clinic"],
          "exampleQueries": ["dental clinic Croatia contact"]
        }
        ```
        """);

        Assert.Equal("Private clinics in Croatia", profile.Summary);
        Assert.Contains("Dental clinics", profile.TargetIndustries);
    }

    [Fact]
    public void DeserializePayload_IcpProfile_ThrowsOnUnclosedJson()
    {
        Assert.Throws<JsonException>(() => DeepSeekAiClient.DeserializePayload<IcpProfile>("""
        {
          "summary": "Private clinics",
          "targetIndustries": ["Dental clinics"],
          "segments": [
        """));
    }
}
