namespace MpToErin;

using System;
using System.Globalization;
using CsvHelper.Configuration;

public class MpRoute
{
    public DateTime Date { get; set; }
    public string Name { get; set; }
    public string Rating { get; set; }
    public string Notes { get; set; }
    public string URL { get; set; }
    public int Pitches { get; set; }
    public string Location { get; set; }
    public string AvgStars { get; set; }
    public string YourStars { get; set; }
    public string Style { get; set; }
    public string LeadStyle { get; set; }
    public string RouteType { get; set; }
    public string YourRating { get; set; }
    public string Length { get; set; }
    public string RatingCode { get; set; }
    
    public sealed class MpRouteMap : ClassMap<MpRoute>
    {
        public MpRouteMap()
        {
            this.AutoMap(CultureInfo.InvariantCulture);

            // Custom header mappings
            this.Map(m => m.Date).Name("Date");
            this.Map(m => m.Name).Name("Route");
            this.Map(m => m.Rating).Name("Rating");
            this.Map(m => m.Notes).Name("Notes");
            this.Map(m => m.URL).Name("URL");
            this.Map(m => m.Pitches).Name("Pitches");
            this.Map(m => m.Location).Name("Location");
            this.Map(m => m.AvgStars).Name("Avg Stars");
            this.Map(m => m.YourStars).Name("Your Stars");
            this.Map(m => m.Style).Name("Style");
            this.Map(m => m.LeadStyle).Name("Lead Style");
            this.Map(m => m.RouteType).Name("Route Type");
            this.Map(m => m.YourRating).Name("Your Rating");
            this.Map(m => m.Length).Name("Length");
            this.Map(m => m.RatingCode).Name("Rating Code");
        }
    }
}