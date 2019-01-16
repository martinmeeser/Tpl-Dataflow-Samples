using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public class SimpleBackPressure 
{
    
    public static async Task ExecuteSample() 
    {
        BroadcastBlock<double> bc = new BroadcastBlock<double>(d=>d);
        
        TransformBlock<double, double> tf = new TransformBlock<double, double>(d => d>0.5? d/2 : d);

        ActionBlock<double> ab1 = new ActionBlock<double>(d=>Console.WriteLine($"Final Value: {d}"));
        ActionBlock<double> ab2 = new ActionBlock<double>(d=>Console.WriteLine($"Proc. Value: {d}"));

        tf.LinkTo(ab1, d=> d<=0.5);   
        tf.LinkTo(bc, d=> d>0.5);

        bc.LinkTo(tf);
        bc.LinkTo(ab2);

        Random random = new Random(); 

        Parallel.For(0, 100, async (i, state) => await tf.SendAsync(random.NextDouble() * 100d));

        await ab1.Completion;
    }

}