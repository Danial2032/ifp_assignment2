using System;
using System.Linq;
using System.Threading;

public static class CdrProcessor
{
    public static decimal ProcessCallsSequential(CallRecord [] records)
    {
        if (records == null)
        {
            throw new ArgumentNullException(nameof(records));
        }

        decimal total = 0.00m;
        for (int i = 0; i < records.Length; i++)
        {
            total += CallPricing.CalculateCost(in records[i]);
        }

        return total;
    }

    public static decimal ProcessCallsParallel(CallRecord[] records)
    {
        if (records == null)
        {
            throw new ArgumentNullException(nameof(records));
        }

        if (records.Length == 0)
        {
            return 0.00m;
        }

        if (records.Length % 2 != 0)
        {
            throw new ArgumentException("Input record array must contain an even number of elements.", nameof(records));
        }

        int mid = records.Length / 2;

        CallRecord[] leftChunk = records[..mid];
        CallRecord[] rightChunk = records[mid..];

        decimal[] leftResults = new decimal[leftChunk.Length];
        decimal[] rightResults = new decimal[rightChunk.Length];

        Exception? leftException = null;
        Exception? rightException = null;

        Thread t1 = new(() =>
        {
            try
            {
                for (int i = 0; i< leftChunk.Length; i++)
                {
                    leftResults[i] = CallPricing.CalculateCost(in leftChunk[i]);
                }
            }
            catch (Exception ex)
            {
                leftException = ex;
            }
        });

        Thread t2 = new(()=>
        {
            try
            {
                for (int i = 0; i < rightChunk.Length; i++)
                {
                    rightResults[i] = CallPricing.CalculateCost(in rightChunk[i]);
                }
            }
                catch (Exception ex)
                {
                    rightException = ex;
                }
        });

        t1.Start();
        t2.Start();

        t1.Join();
        t2.Join();

        if (leftException != null)
        {
            throw new AggregateException("Worker thread 1 failed during call record processing.", leftException);
        }

        if (rightException != null)
        {
            throw new AggregateException("Worker thread 2 failed during call record processing.", rightException);
        }

        decimal total = 0.00m;
        for (int i = 0; i < leftResults.Length; i++)
        {
            total += leftResults[i];
        }

        for (int i = 0; i < rightResults.Length; i++)
        {
            total += rightResults[i];
        }

        return total;
    }
}