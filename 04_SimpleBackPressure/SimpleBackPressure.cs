using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public class SimpleBackPressure
{

    public static async Task ExecuteSampleAsync()
    {
        // blocks
        TransformBlock<double, double> tf = new TransformBlock<double, double>(d =>
            {
                Console.WriteLine($"Proc. Value: {d}");
                return d > 0.5 ? d / 2 : d;
            }
        );
        ActionBlock<double> ab1 = new ActionBlock<double>(d => Console.WriteLine($"Final Value: {d}"));

        // links    
        tf.LinkTo(ab1, new DataflowLinkOptions { PropagateCompletion = true }, d => d <= 0.5);
        tf.LinkTo(tf, d => d > 0.5);

        // logic
        Random random = new Random();
        var result = Parallel.For(0, 100, async (i, state) =>
        {
            await tf.SendAsync(random.NextDouble() * 100d);
        });

        // completing
        while (!result.IsCompleted)
        {
            await Task.Delay(10);
        }

        while (tf.InputCount > 0 || tf.OutputCount > 0)
        {
            await Task.Delay(100);
        }

        tf.Complete();
        await Task.WhenAll(tf.Completion, ab1.Completion);
        Console.WriteLine("Finished");
    }

}