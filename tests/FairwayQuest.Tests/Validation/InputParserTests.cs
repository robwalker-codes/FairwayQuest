using FairwayQuest.Core.Models;
using FairwayQuest.Core.Validation;

namespace FairwayQuest.Tests.Validation;

public class InputParserTests
{
    [Theory]
    [InlineData("1", 1)]
    [InlineData("4", 4)]
    public void PlayerCount_AllowsValuesBetweenOneAndFour(string input, int expected)
    {
        InputParsers.TryParsePlayerCount(input, out var result).Should().BeTrue();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("5")]
    [InlineData("abc")]
    public void PlayerCount_RejectsInvalidEntries(string input)
    {
        InputParsers.TryParsePlayerCount(input, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("9", 9)]
    [InlineData("18", 18)]
    public void HoleCount_AcceptsNineOrEighteen(string input, int expected)
    {
        InputParsers.TryParseHoleCount(input, out var result).Should().BeTrue();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("8")]
    [InlineData("20")]
    [InlineData("" )]
    public void HoleCount_RejectsOtherValues(string input)
    {
        InputParsers.TryParseHoleCount(input, out _).Should().BeFalse();
    }

    [Fact]
    public void ClubParsing_IsCaseInsensitive()
    {
        InputParsers.TryParseClub("7I", false, out var club).Should().BeTrue();
        club.Should().Be("7i");
    }

    [Fact]
    public void ClubParsing_RejectsPutterOffGreen()
    {
        InputParsers.TryParseClub("p", false, out _).Should().BeFalse();
    }

    [Fact]
    public void ClubParsing_AllowsPutterOnGreen()
    {
        InputParsers.TryParseClub("P", true, out var club).Should().BeTrue();
        club.Should().Be("p");
    }

    [Fact]
    public void ClubParsing_RejectsUnknownCode()
    {
        InputParsers.TryParseClub("xx", false, out _).Should().BeFalse();
    }
}
