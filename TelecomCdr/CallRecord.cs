using System;

public readonly record struct CallRecord
{
    public string RecordId { get; }
    public string DestinationCountry { get; }
    public double DurationMinutes { get; }
    public bool IsRoaming { get; }

    public CallRecord(string recordId, string destinationCountry, double durationMinutes, bool isRoaming)
    {
        ValidateInputs(recordId, destinationCountry, durationMinutes);

        RecordId = recordId;
        DestinationCountry = destinationCountry;
        DurationMinutes = durationMinutes;
        IsRoaming = isRoaming;
    }

    internal static void ValidateInputs(string recordId, string destinationCountry, double durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(recordId))
        {
            throw new ArgumentException("Record ID cannot be null, empty or whitespace.", nameof(recordId));
        }

        if (string.IsNullOrWhiteSpace(destinationCountry))
        {
            throw new ArgumentException("Destination country code cannot be null, empty or whitespace.", nameof(destinationCountry));
        }

        if (double.IsNaN(durationMinutes))
        {
            throw new ArgumentException("Duration cannot be NaN.", nameof(durationMinutes));
        }

        if (double.IsInfinity(durationMinutes))
        {
            throw new ArgumentException("Duration cannot be infinite.", nameof(durationMinutes));
        }

        if (durationMinutes < 0.0 || durationMinutes > 10_000.0)
        {
            throw new ArgumentException("Duration must be finite, non-negative and not greater than 10000 minutes.", nameof(durationMinutes));
        }
    }
}