using dev.kaldiroglu.bootcamp.cleancode;
using Xunit;

namespace Bootcamp.Examples.Tests;

public class CleanCodeTests
{
    private static readonly Dictionary<string, string> Users = new() { ["u1"] = "Ada" };

    [Fact]
    public void HonestErrorsThrowInsteadOfReturningNull()
    {
        Assert.Throws<UserNotFoundException>(() => new UserFinder(Users).Find("missing"));
        Assert.Equal("Ada", new UserFinder(Users).Find("u1"));
        Assert.Null(new UserFinderSmell(Users).Find("missing"));
    }

    [Fact]
    public void GuardClausesPreserveBehaviour()
    {
        var smell = new EligibilitySmell();
        var clean = new Eligibility();
        int[][] cases = { new[] { 25, 1, 0 }, new[] { 16, 1, 0 }, new[] { 30, 0, 0 }, new[] { 40, 1, 1 } };
        foreach (var c in cases)
        {
            Assert.Equal(
                smell.Describe(c[0], c[1] == 1, c[2] == 1),
                clean.Describe(c[0], c[1] == 1, c[2] == 1));
        }
        Assert.Equal("eligible", clean.Describe(25, true, false));
        Assert.Equal("banned", clean.Describe(25, true, true));
    }
}
