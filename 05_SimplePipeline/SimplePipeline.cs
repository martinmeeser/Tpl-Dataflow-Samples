using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public class SimplePipeline
{

    public static async Task CreateSampleCsvFile(string path, int totalCount, int countPerLine)
    {
        int lines = totalCount / countPerLine;
        Random random = new Random();

        StreamWriter streamWriter = null;
        try
        {
            streamWriter = new StreamWriter(path);
        }
        catch (UnauthorizedAccessException) { }
        catch (ArgumentException) { }
        catch (DirectoryNotFoundException) { }

        if (streamWriter != null)
        {
            for (int i = 0; i < lines; i++)
            {
                for (int j = 0; j < countPerLine; j++)
                {
                    double d = NextDoubleInMinMaxRange(random);

                    await streamWriter.WriteAsync(d.ToString());
                    await streamWriter.WriteAsync(";");
                }
                await streamWriter.WriteLineAsync();
            }
        }

        streamWriter.Close();
    }


    public static async Task ExecutePipelineSample(string path)
    {
        // blocks
        var splitStrings = new TransformManyBlock<string, string>(
            line => line.Split(';')
        , new ExecutionDataflowBlockOptions { });

        var parseStrings = new TransformBlock<string, double?>(s =>
        {
            if (!string.IsNullOrEmpty(s))
            {
                if (Double.TryParse(s, out double result))
                {
                    return result;
                }
                Console.WriteLine($"Parse Error: {s}");
            }
            return null;
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

        double? min = null;
        double? max = null;
        ulong count = 0;

        var minMax = new ActionBlock<double?>(d =>
        {
            if (d.HasValue)
            {
                if (!min.HasValue) { min = d.Value; }
                if (!max.HasValue) { max = d.Value; }

                if (min > d.Value) { min = d.Value; }
                if (max < d.Value) { max = d.Value; }

                count++;
            }
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

        // links
        splitStrings.LinkTo(parseStrings, new DataflowLinkOptions { PropagateCompletion = true });
        parseStrings.LinkTo(minMax, new DataflowLinkOptions { PropagateCompletion = true });

        try
        {
            StreamReader streamReader = new StreamReader(path);
            {
                string currLine;
                Console.WriteLine($"Starting: {DateTime.Now}");
                while ((currLine = streamReader.ReadLine()) != null)
                {
                    await splitStrings.SendAsync(currLine);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        splitStrings.Complete();
        await Task.WhenAll(splitStrings.Completion, parseStrings.Completion, minMax.Completion);

        Console.WriteLine($"Stopped: {DateTime.Now}");
        Console.WriteLine($"Computed {count} values, min is {min}, max is {max}");

    }


    private static double NextDoubleInMinMaxRange(Random random)
    {
        var bytes = new byte[sizeof(double)];
        var value = default(double);
        while (true)
        {
            random.NextBytes(bytes);
            value = BitConverter.ToDouble(bytes, 0);
            if (!double.IsNaN(value) && !double.IsInfinity(value))
                return value;
        }
    }

}