using System;
using System.Collections.Generic;
using Xunit;

public class CdrProcessorTests
{

    [Theory]
    [InlineData("KZ", false, 4.0, 60.00)]
    [InlineData("KZ", true, 0.5, 50.00)]
    [InlineData("US", true, 10.0, 1200.00)]
    [InlineData("DE", false, 3.0, 135.00)]
    [InlineData("XX", false, 2.0, 90.00)]
    [InlineData("KZ", true, 1.0, 45.00)]
    public void CalculateCost_ValidTariffCases_ReturnsExpectedCost(string country, bool isRoaming, double duration, decimal expectedCost)
    {
        var record = new CallRecord("RECE_TEST", country, duration, isRoaming);
        decimal actualCost = CallPricing.CalculateCost(in record);
        Assert.Equal(expectedCost, actualCost);
    }

    [Theory]
    [InlineData("","KZ", 5.0)]
    [InlineData("   ","KZ", 5.0)]
    [InlineData("REC1","", 5.0)]
    [InlineData("REC1","   ", 5.0)]
    [InlineData("REC1","KZ", -0.1)]
    [InlineData("REC1","KZ", 10000.1)]
    [InlineData("REC1","KZ", double.NaN)]
    [InlineData("REC1","KZ", double.PositiveInfinity)]
    [InlineData("REC1","KZ", double.NegativeInfinity)]
    public void CallRecord_Constructor_RejectsInvalidInputs(string recordId, string country, double duration)
    {
        Assert.Throws<ArgumentException>(() => new CallRecord(recordId, country, duration, false));
    }

    [Fact]
    public void CalculateCost_DefaultCallRecord_ThrowsArgumentException()
    {
        CallRecord defaultRecord = default;
        Assert.Throws<ArgumentException>(() => CallPricing.CalculateCost(in defaultRecord));
    }

    [Fact]
    public void CalculateCost_ZeroMinuteCalls_CalculatesCorrectly()
    {
        var zeroRoamingKz = new CallRecord("REC_Z1", "KZ", 0.0, true);
        var zeroNonRoamingKz = new CallRecord("REC_Z2", "KZ", 0.0, false);

        Assert.Equal(50.00m, CallPricing.CalculateCost(in zeroRoamingKz));
        Assert.Equal(0.00m, CallPricing.CalculateCost(in zeroNonRoamingKz));
    }

    [Theory]
    [InlineData(0.999999, 50.00)] 
    [InlineData(1.000000, 45.00)] 
    [InlineData(9.999999, 450.00)] 
    [InlineData(10.000000, 1200.00)]
    public void CalculateCost_TariffBoundaries_EvaluateCorrectly(double duration, decimal expectedCost)
    {
        bool isKz = duration < 5.0;
        string country = isKz? "KZ" : "US";
        var record =  new CallRecord("REC_BND", country, duration, true);

        Assert.Equal(expectedCost, CallPricing.CalculateCost(in record));
    }

    [Fact]
    public void ProcessCallsParallel_MatchesSequential_Across100Runs()
    {
        const int recordCount = 1000;
        var records = new CallRecord[recordCount];
        var originalRecords = new CallRecord[recordCount];

        string[] countries = ["KZ", "US", "DE", "XX", "FR"];
        Random rand = new(42);

        for (int i = 0; i < recordCount; i++)
        {
            string country = countries[rand.Next(countries.Length)];
            double duration = Math.Round(rand.NextDouble() * 100.0, 3);
            bool isRoaming = rand.Next(2) == 0;

            var rec = new CallRecord($"REC_{i}", country, duration, isRoaming);
            records[i] = rec;
            originalRecords[i] = rec;
        }

        decimal expectedTotal = CdrProcessor.ProcessCallsSequential(records);

        for (int run = 0; run < 100; run++)
        {
            decimal parallelTotal = CdrProcessor.ProcessCallsParallel(records);
            Assert.Equal(expectedTotal, parallelTotal);
        }

        for (int i =0; i< recordCount; i++)
        {
            Assert.Equal(originalRecords[i], records[i]);
        }
    }

    [Fact]
    public void ProcessCallsParallel_EmptyArray_ReturnsZero()
    {
        CallRecord[] empty = [];
        Assert.Equal(0.00m, CdrProcessor.ProcessCallsParallel(empty));
    }

    [Fact]
    public void ProcessCallsParallel_OddLengthArray_ThrowsArgumentException()
    {
        CallRecord[] oddRecords = [new CallRecord("REC1", "KZ", 2.0, false)];
        Assert.Throws<ArgumentException>(() => CdrProcessor.ProcessCallsParallel(oddRecords));
    }
}