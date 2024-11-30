namespace MpToErin.Tests;

using System.Xml;
using FluentAssertions;
using Xunit.Abstractions;

public class RouteConverterTests(ITestOutputHelper output)
{
    [Fact]
    public void CapturesBasicData()
    {
        var stuff = """
            Date,Route,Rating,Notes,URL,Pitches,Location,"Avg Stars","Your Stars",Style,"Lead Style","Route Type","Your Rating",Length,"Rating Code"
            2024-11-26,"TheName",5.11a,TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,Fell/Hung,Sport,,35,4600
            """;

        var routes = RouteConverter.ConvertToErinRoutes(stuff);
        var route = routes.routes.First();
        route.Name.Should().Be("TheName");
        route.TypeOfClimb.Should().Be("Lead");
        route.InsideOrOutside.Should().Be("Outside");
        route.Grade.Should().Be("11.1");
        route.WasSent.Should().Be(false);
        route.Attempts.Should().Be(1);
        route.DateSent.Should().BeNull();
        route.Notes.Should().Be("TheNotes");
        route.Url.Should().Be("TheUrl");
    }

    [Theory]
    [InlineData("Fell/Hung", false)]
    [InlineData("Onsight", true)]
    [InlineData("Flash", true)]
    [InlineData("Redpoint", true)]
    [InlineData("Pinkpoint", true)]
    [InlineData("weird", false, true)]
    public void CapturesSent(string leadStyle, bool wasSent, bool unknown = false)
    {
        var stuff = $"""
            Date,Route,Rating,Notes,URL,Pitches,Location,"Avg Stars","Your Stars",Style,"Lead Style","Route Type","Your Rating",Length,"Rating Code"
            2024-11-26,"TheName",5.11a,TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,{leadStyle},Sport,,35,4600
            """;

        var routes = RouteConverter.ConvertToErinRoutes(stuff);
        var route = routes.routes.First();
        route.WasSent.Should().Be(wasSent);
        if (wasSent)
        {
            route.DateSent.Should().Be(DateTime.Parse("2024-11-26"));
        }
        else
        {
            route.DateSent.Should().BeNull();
        }

        if (unknown)
        {
            routes.errors.Count.Should().Be(1);
            routes.errors.First().Should().Contain(leadStyle);
        }
    }

    // TODO what about trad/boulder?

    [Fact]
    public void CombinesAttempts()
    {
        var stuff = $"""
            Date,Route,Rating,Notes,URL,Pitches,Location,"Avg Stars","Your Stars",Style,"Lead Style","Route Type","Your Rating",Length,"Rating Code"
            2024-11-26,"TheName",5.11a,TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,Fell/Hung,Sport,,35,4600
            2024-11-26,"TheName",5.11a,TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,Redpoint,Sport,,35,4600
            """;
        var routes = RouteConverter.ConvertToErinRoutes(stuff);
        routes.routes.Count.Should().Be(1);
        var route = routes.routes.First();
        route.DateSent.Should().Be(DateTime.Parse("2024-11-26"));
        route.WasSent.Should().Be(true);
        route.Attempts.Should().Be(2);
    }

    [Fact]
    public void CombinesSentDate()
    {
        var stuff = $"""
            Date,Route,Rating,Notes,URL,Pitches,Location,"Avg Stars","Your Stars",Style,"Lead Style","Route Type","Your Rating",Length,"Rating Code"
            2024-11-10,"TheName",5.11a,TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,Redpoint,Sport,,35,4600
            2024-11-26,"TheName",5.11a,TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,Fell/Hung,Sport,,35,4600
            2024-11-26,"TheName",5.11a,TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,Redpoint,Sport,,35,4600
            """;
        var routes = RouteConverter.ConvertToErinRoutes(stuff);
        routes.routes.Count.Should().Be(1);
        var route = routes.routes.First();
        route.DateSent.Should().Be(DateTime.Parse("2024-11-10"));
        route.WasSent.Should().Be(true);
        route.Attempts.Should().Be(3);
    }

    [Fact]
    public void CombinesSentDate2()
    {
        var stuff = $"""
            Date,Route,Rating,Notes,URL,Pitches,Location,"Avg Stars","Your Stars",Style,"Lead Style","Route Type","Your Rating",Length,"Rating Code"
            2024-11-26,"TheName",5.11a,TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,Redpoint,Sport,,35,4600
            2024-11-10,"TheName",5.11a,TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,Redpoint,Sport,,35,4600
            2024-11-26,"TheName",5.11a,TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,Fell/Hung,Sport,,35,4600
            """;
        var routes = RouteConverter.ConvertToErinRoutes(stuff);
        routes.routes.Count.Should().Be(1);
        var route = routes.routes.First();
        route.DateSent.Should().Be(DateTime.Parse("2024-11-10"));
        route.WasSent.Should().Be(true);
        route.Attempts.Should().Be(3);
    }

    [Theory]
    [InlineData("5.8", "8")]
    [InlineData("5.8+", "8")]
    [InlineData("5.88+", "5.88+", true)]
    public void GetsGrade(string grade, string expected, bool hasError = false)
    {
        var stuff = $"""
            Date,Route,Rating,Notes,URL,Pitches,Location,"Avg Stars","Your Stars",Style,"Lead Style","Route Type","Your Rating",Length,"Rating Code"
            2024-11-26,"TheName",{grade},TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,Redpoint,Sport,,35,4600
            """;
        var routes = RouteConverter.ConvertToErinRoutes(stuff);
        routes.routes.Count.Should().Be(1);
        var route = routes.routes.First();
        route.Grade.Should().Be(expected);

        if (hasError)
        {
            routes.errors.First().Should().Contain(expected);
        }
    }

    [Fact]
    public void OutputsCsv()
    {
        var stuff = """
            Date,Route,Rating,Notes,URL,Pitches,Location,"Avg Stars","Your Stars",Style,"Lead Style","Route Type","Your Rating",Length,"Rating Code"
            2024-11-26,"TheName",5.11a,TheNotes,TheUrl,1,"Ash Branch",2.5,3,Lead,Fell/Hung,Sport,,35,4600
            """;

        var result = RouteConverter.ConvertToNewCsv(stuff);
        result.Should().Be("TheName,Lead,Outside,No,1,,TheNotes");
    }

    [Fact]
    public void DoStuff()
    {
        var stuff = File.ReadAllText("C:\\Users\\bela\\Downloads\\ticks2.csv");
        var data = RouteConverter.MakeMpRoutes(stuff);

        var results = data.Select(o =>
                o.Rating.Replace(" PG13", "").Replace(" WI3+ M3 Mod. Snow", "").Replace(" R", "")
            )
            .Distinct()
            .OrderBy(o => o)
            .ToList();

        foreach (var result in results)
        {
            output.WriteLine($"\"{result}\" => \"\",");
        }

        void AddThing(string gradeIn, string gradeOut)
        {
            output.WriteLine($"\"{gradeIn}\" => \"{gradeOut}\",");
        }

        for (var x = 1; x < 10; x++)
        {
            AddThing($"5.{x}", $"{x}");
            AddThing($"5.{x}-", $"{x}");
            AddThing($"5.{x}+", $"{x}");
        }
        for (var x = 0; x < 5; x++)
        {
            AddThing($"5.1{x}a", $"1{x}.1");
            AddThing($"5.1{x}-", $"1{x}.2");
            AddThing($"5.1{x}a/b", $"1{x}.2");
            AddThing($"5.1{x}b", $"1{x}.2");
            AddThing($"5.1{x}c", $"1{x}.3");
            AddThing($"5.1{x}b/c", $"1{x}.3");
            AddThing($"5.1{x}d", $"1{x}.4");
            AddThing($"5.1{x}c/d", $"1{x}.4");
            AddThing($"5.1{x}+", $"1{x}.4");
        }
    }

    [Fact]
    public void DoStuff2()
    {
        var stuff = File.ReadAllText("C:\\Users\\bela\\Downloads\\ticks2.csv");
        var data = RouteConverter.ConvertToNewCsv(stuff);
        File.WriteAllText("c:/Users/bela/Downloads/new.csv", data);
    }
}
