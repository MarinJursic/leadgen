using Xunit;

namespace LeadGen.Tests;

public sealed class RealProviderFactAttribute : FactAttribute
{
    public RealProviderFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY"))
            || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TAVILY_API_KEY")))
        {
            Skip = "Real provider keys are required for this test.";
        }
    }
}
