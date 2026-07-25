using FluentAssertions;
using SP.Domain.Abstractions;
using Xunit;

namespace SP.UnitTests.Domain;

public sealed class ResultTests
{
    [Fact]
    public void Success_IsSuccess_IsTrue()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_IsFailure_IsTrue()
    {
        var error = new Error("Test.Error", "Something went wrong");
        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void SuccessWithValue_ReturnsValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void FailureWithValue_AccessingValue_ThrowsInvalidOperation()
    {
        var result = Result.Failure<int>(new Error("E", "msg"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_WithNonNullValue_ReturnsSuccess()
    {
        var result = Result.Create("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Create_WithNullValue_ReturnsFailure()
    {
        var result = Result.Create<string>(null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NullValue);
    }

    [Fact]
    public void Success_WithError_ThrowsInvalidOperation()
    {
        var act = () => new Result_Accessor(true, new Error("E", "msg"));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Failure_WithNoError_ThrowsInvalidOperation()
    {
        var act = () => new Result_Accessor(false, Error.None);

        act.Should().Throw<InvalidOperationException>();
    }

    // Expose protected constructor for testing
    private sealed class Result_Accessor : Result
    {
        public Result_Accessor(bool isSuccess, Error error) : base(isSuccess, error) { }
    }
}
