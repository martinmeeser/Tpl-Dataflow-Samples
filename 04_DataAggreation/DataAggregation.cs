using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public class DataAggregation
{

    public static async Task ExecuteGreedyBatchBlockSample()
    {
        BatchBlock<string> batchBlock = new BatchBlock<string>(5, new GroupingDataflowBlockOptions() { Greedy = true });

        var writer = new ActionBlock<string[]>(strings => strings.ToList().ForEach(str => Console.WriteLine(str)));
        batchBlock.LinkTo(writer, new DataflowLinkOptions() { PropagateCompletion = true });

        batchBlock.Post("1-1");
        batchBlock.Post("1-2");
        batchBlock.Post("1-3");
        batchBlock.Post("2-1");
        batchBlock.Post("3-1");
        batchBlock.Post("4-1");
        batchBlock.Post("5-1");

        batchBlock.Complete();
        await batchBlock.Completion;
    }

    public void ExecuteNonGreedyBatchBlockSample()
    {

    }

    public void ExecuteNonGreedyBatchBlockSampleWithBufferBlocks()
    {

    }



}