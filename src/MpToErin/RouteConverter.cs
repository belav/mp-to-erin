namespace MpToErin;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

public static class RouteConverter
{
    public static List<MpRoute> MakeMpRoutes(string inputCsv)
    {
        using var reader = new StringReader(inputCsv);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        csv.Context.RegisterClassMap<MpRoute.MpRouteMap>();
        var climbingRoutes = csv.GetRecords<MpRoute>().ToList();

        return climbingRoutes;
    }

    public static string ConvertToNewCsv(string inputCsv)
    {
        var result = ConvertToErinRoutes(inputCsv);

        // Name/Description	Type of Climb	Gym/Outside	Grade	Send y/n	# of attempts	Date Sent	Notes

        using var stringWriter = new StringWriter();
        using var csvWriter = new CsvWriter(stringWriter, CultureInfo.InvariantCulture);

        foreach (var route in result.routes)
        {
            csvWriter.WriteField(route.Name);
            csvWriter.WriteField(route.TypeOfClimb);
            csvWriter.WriteField(route.InsideOrOutside);
            csvWriter.WriteField(route.Grade);
            csvWriter.WriteField(route.WasSent ? "Yes" : "Not Yet!");
            csvWriter.WriteField(route.Attempts);
            csvWriter.WriteField(route.DateSent);
            csvWriter.WriteField(route.Notes);
            csvWriter.NextRecord();
        }

        return stringWriter.ToString();
    }

    public static (List<ErinRoute> routes, List<string> errors) ConvertToErinRoutes(string inputCsv)
    {
        var climbingRoutes = MakeMpRoutes(inputCsv);

        var errors = new List<string>();
        var routes = new Dictionary<string, ErinRoute>();
        foreach (var mpRoute in climbingRoutes)
        {
            var erinRoute = ConvertToErin(mpRoute, errors);
            if (routes.TryGetValue(erinRoute.Url, out var existingRoute))
            {
                existingRoute.Attempts++;
                existingRoute.WasSent = erinRoute.WasSent || existingRoute.WasSent;
                existingRoute.DateSent = new[] { erinRoute.DateSent, existingRoute.DateSent }
                    .Where(date => date.HasValue)
                    .DefaultIfEmpty(null)
                    .Min();
                if (erinRoute.Notes.Length > 0)
                {
                    if (existingRoute.Notes.Length > 0)
                    {
                        existingRoute.Notes += "\n";
                    }

                    existingRoute.Notes += erinRoute.Notes;
                }
            }
            else
            {
                routes.Add(erinRoute.Url, erinRoute);
            }
        }

        return (routes.ToList().Select(o => o.Value).ToList(), errors);
    }

    private static ErinRoute ConvertToErin(MpRoute mpRoute, List<string> errors)
    {
        bool StoreLeadStyleError()
        {
            errors.Add(
                $"The tick for {mpRoute.Name} - {mpRoute.URL} has a Lead Style of '{mpRoute.LeadStyle}' which was unknown so assumed to not be a send."
            );

            return false;
        }

        var wasSent = mpRoute.LeadStyle switch
        {
            "Onsight" => true,
            "Flash" => true,
            "Pinkpoint" => true,
            "Redpoint" => true,
            "Lead/Hung" => false,
            _ => StoreLeadStyleError()
        };

        // TODO what about TR + trad + boulder?
        var typeOfClimb = mpRoute.Style switch
        {
            "Sport" => "Lead",
            _ => mpRoute.Style,
        };

        string StoreRatingError()
        {
            errors.Add(
                $"The tick for {mpRoute.Name} - {mpRoute.URL} has a Rating of '{mpRoute.Rating}' which was unknown."
            );

            return mpRoute.Rating;
        }

        var grade = mpRoute.Rating switch
        {
            "5.1" => "1",
            "5.1-" => "1",
            "5.1+" => "1",
            "5.2" => "2",
            "5.2-" => "2",
            "5.2+" => "2",
            "5.3" => "3",
            "5.3-" => "3",
            "5.3+" => "3",
            "5.4" => "4",
            "5.4-" => "4",
            "5.4+" => "4",
            "5.5" => "5",
            "5.5-" => "5",
            "5.5+" => "5",
            "5.6" => "6",
            "5.6-" => "6",
            "5.6+" => "6",
            "5.7" => "7",
            "5.7-" => "7",
            "5.7+" => "7",
            "5.8" => "8",
            "5.8-" => "8",
            "5.8+" => "8",
            "5.9" => "9",
            "5.9-" => "9",
            "5.9+" => "9",
            "5.10a" => "10.1",
            "5.10-" => "10.2",
            "5.10a/b" => "10.2",
            "5.10b" => "10.2",
            "5.10c" => "10.3",
            "5.10b/c" => "10.3",
            "5.10d" => "10.4",
            "5.10c/d" => "10.4",
            "5.10+" => "10.4",
            "5.11a" => "11.1",
            "5.11-" => "11.2",
            "5.11a/b" => "11.2",
            "5.11b" => "11.2",
            "5.11c" => "11.3",
            "5.11b/c" => "11.3",
            "5.11d" => "11.4",
            "5.11c/d" => "11.4",
            "5.11+" => "11.4",
            "5.12a" => "12.1",
            "5.12-" => "12.2",
            "5.12a/b" => "12.2",
            "5.12b" => "12.2",
            "5.12c" => "12.3",
            "5.12b/c" => "12.3",
            "5.12d" => "12.4",
            "5.12c/d" => "12.4",
            "5.12+" => "12.4",
            "5.13a" => "13.1",
            "5.13-" => "13.2",
            "5.13a/b" => "13.2",
            "5.13b" => "13.2",
            "5.13c" => "13.3",
            "5.13b/c" => "13.3",
            "5.13d" => "13.4",
            "5.13c/d" => "13.4",
            "5.13+" => "13.4",
            "5.14a" => "14.1",
            "5.14-" => "14.2",
            "5.14a/b" => "14.2",
            "5.14b" => "14.2",
            "5.14c" => "14.3",
            "5.14b/c" => "14.3",
            "5.14d" => "14.4",
            "5.14c/d" => "14.4",
            "5.14+" => "14.4",
            _ => StoreRatingError(),
        };

        if (!Regex.IsMatch(grade, "1?[0-9]([0-4]?)"))
        {
            errors.Add(
                $"The tick for {mpRoute.Name} - {mpRoute.URL} has a Rating of '{mpRoute.Rating}' which could not be properly converted to an Erin grade"
            );
        }

        return new ErinRoute
        {
            Name = mpRoute.Name,
            Url = mpRoute.URL,
            TypeOfClimb = typeOfClimb,
            InsideOrOutside = "Outside",
            Grade = grade,
            WasSent = wasSent,
            Attempts = 1,
            DateSent = wasSent ? mpRoute.Date : null,
            Notes = mpRoute.Notes
        };
    }
}
