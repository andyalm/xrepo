using FluentAssertions;
using XRepo.Core;
using Xunit;

namespace XRepo.Tests
{
    public class NuGetVersionSpecs
    {
        [Theory]
        [InlineData("1", "1.0.0")]
        [InlineData("1.0", "1.0.0")]
        [InlineData("1.0.0-beta", "1.0.0-beta")]
        [InlineData("2.0-beta", "2.0.0-beta")]
        [InlineData("2.0.0-beta+12345", "2.0.0-beta+12345")]
        [InlineData("2.0.0+12345", "2.0.0+12345")]
        public void VersionsAreNormalized(string input, string expected)
        {
            var actual = NuGetVersion.Normalize(input);
            actual.Should().Be(expected);
        }
    }
}