using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public class SimpleBackPressure 
{
    
    public static async Task ExecuteSampleAsync() 
    {
        BufferBlock<double> source = new BufferBlock<double>();
        
        BroadcastBlock<double> bc = new BroadcastBlock<double>(d=>d);
        
        TransformBlock<double, double> tf = new TransformBlock<double, double>(d => d>0.5? d/2 : d);

        ActionBlock<double> ab1 = new ActionBlock<double>(d=>Console.WriteLine($"Final Value: {d}"));
        ActionBlock<double> ab2 = new ActionBlock<double>(d=>Console.WriteLine($"Proc. Value: {d}"));

        source.LinkTo(tf, new DataflowLinkOptions{ PropagateCompletion=true });

        tf.LinkTo(ab1, new DataflowLinkOptions{ PropagateCompletion=true }, d=> d<=0.5);
        tf.LinkTo(bc, new DataflowLinkOptions{ PropagateCompletion=true }, d=> d>0.5);

        bc.LinkTo(tf);
        bc.LinkTo(ab2, new DataflowLinkOptions{ PropagateCompletion=true });

        Random random = new Random();

        var result = Parallel.For(0, 100, async (i, state) => 
        {
            await source.SendAsync(random.NextDouble() * 100d);
        });

        while (!result.IsCompleted) 
        {
            await Task.Delay(100);
        }
        
        source.Complete();
        await Task.WhenAll(bc.Completion, tf.Completion, ab1.Completion, ab2.Completion);

        await ab1.Completion;
    }

}