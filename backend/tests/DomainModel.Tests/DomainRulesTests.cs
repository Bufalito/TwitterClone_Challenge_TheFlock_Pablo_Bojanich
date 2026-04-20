using Domain.Entities;
using Xunit;

namespace DomainModel.Tests;

public class DomainRulesTests
{
    [Fact]
    public void Tweet_ShouldRejectContentLongerThan280Characters()
    {
        var content = new string('a', Tweet.MaxContentLength + 1);

        var exception = Assert.Throws<ArgumentException>(() => new Tweet(Guid.NewGuid(), content));

        Assert.Equal("content", exception.ParamName);
    }

    [Fact]
    public void Follow_ShouldRejectSelfFollow()
    {
        var userId = Guid.NewGuid();

        var exception = Assert.Throws<ArgumentException>(() => new Follow(userId, userId));

        Assert.Equal("followedId", exception.ParamName);
    }
}