namespace MpToErin;

using System;

public class ErinRoute
{
    public string Name { get; set; }
    public string TypeOfClimb { get; set; }
    public string InsideOrOutside { get; set; }
    public string Grade { get; set; }
    public bool WasSent { get; set; }
    public int Attempts { get; set; }
    public DateTime? DateSent { get; set; }
    public string Notes { get; set; }
    public string Url { get; set; }
}