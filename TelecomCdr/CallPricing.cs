using System;

public static class CallPricing
{
    public static decimal CalculateCost(in CallRecord record)
    {

        if (string.IsNullOrWhiteSpace(record.RecordId) ||
        string.IsNullOrWhiteSpace(record.DestinationCountry) ||
        double.IsNaN(record.DurationMinutes) ||
        double.IsInfinity(record.DurationMinutes) ||
        record.DurationMinutes < 0.0 ||
        record.DurationMinutes > 10_000.0)
        {
            throw new ArgumentException("Invalid CallRecord instance.");
        }

        decimal cost = record switch
        {
            _ when double.IsNaN(record.DurationMinutes) => 
            throw new ArgumentException("Duration cannot be NaN."),

            { IsRoaming: true, DestinationCountry: "KZ", DurationMinutes: < 1.0 } =>
            50.00m,

            { IsRoaming: false, DestinationCountry: "KZ" } =>
            (decimal)record.DurationMinutes * 15.00m,

            { IsRoaming: true, DurationMinutes: >= 10.0 } =>
            (decimal)record.DurationMinutes * 120.00m,

            _ => (decimal)record.DurationMinutes * 45.00m
        };

        return Math.Round(cost, 2, MidpointRounding.AwayFromZero);
    }
}