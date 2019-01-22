using System;
using System.IO;
using System.Threading.Tasks;

public class SimplePipeline
{

    public static async Task CreateSampleFile(string path, int totalCount, int countPerLine)
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