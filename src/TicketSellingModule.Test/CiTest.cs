using Xunit;

namespace Boards_WP.Tests;

public class SanityCheck
{
    [Fact]
    public void System_Should_Work()
    {
        //--this test always passes
        //--it is here just to check if the CI is working
        bool isSetupCorrect = true;
        Assert.True(isSetupCorrect);
    }
}
