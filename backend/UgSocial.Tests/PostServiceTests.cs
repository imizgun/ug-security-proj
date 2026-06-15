using backend.Services;

namespace UgSocial.Tests;

public class PostServiceTests
{
    private readonly PostService _sut = new();

    [Fact]
    public void ValidateContent_EmptyString_ReturnsFailure()
    {
        var result = _sut.ValidateContent(string.Empty);

        Assert.False(result.IsValid);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void ValidateContent_WhitespaceOnly_ReturnsFailure()
    {
        var result = _sut.ValidateContent("   ");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateContent_NullContent_ReturnsFailure()
    {
        var result = _sut.ValidateContent(null);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateContent_ValidText_ReturnsSuccess()
    {
        var result = _sut.ValidateContent("Hello, world!");

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ValidateContent_ExactlyMaxLength_ReturnsSuccess()
    {
        var content = new string('a', PostService.MaxContentLength);

        var result = _sut.ValidateContent(content);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateContent_ExceedsMaxLength_ReturnsFailure()
    {
        var content = new string('a', PostService.MaxContentLength + 1);

        var result = _sut.ValidateContent(content);

        Assert.False(result.IsValid);
        Assert.Contains("500", result.Error);
    }

    [Fact]
    public void CanDelete_Moderator_ReturnsTrue()
    {
        var result = _sut.CanDelete(isRequestingUserModerator: true);

        Assert.True(result);
    }

    [Fact]
    public void CanDelete_RegularUser_ReturnsFalse()
    {
        var result = _sut.CanDelete(isRequestingUserModerator: false);

        Assert.False(result);
    }
}
