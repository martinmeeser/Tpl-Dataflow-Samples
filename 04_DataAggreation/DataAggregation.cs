using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public class DataAggregation
{

    public static TransformBlock<int, string> CreateTransformBlock(int i)
    {
        return new TransformBlock<int, string>(i2 => String.Format("{0}-{1}", i, i2));
    }

    public static async Task ExecuteBatchBlockSampleAsync(bool isGready, int batchSize)
    {
        Console.WriteLine($"BatchBlock isGready: {isGready}, batchSize: {batchSize}");

        TransformBlock<int, string>[] tbs = new TransformBlock<int, string>[batchSize];
        for (int i = 0; i < batchSize; i++)
        {
            tbs[i] = CreateTransformBlock(i);
        }


        BatchBlock<string> batchBlock = new BatchBlock<string>(
                    batchSize, new GroupingDataflowBlockOptions() { Greedy = isGready }
                );

        ActionBlock<string[]> writer = new ActionBlock<string[]>(
            strings =>
            {
                strings.ToList().ForEach(str => Console.Write(str + ";"));
                Console.WriteLine();
            });

        Array.ForEach(tbs, tb => tb.LinkTo(batchBlock));
        batchBlock.LinkTo(writer, new DataflowLinkOptions { PropagateCompletion = true });

        for (int i = 0; i < batchSize; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                tbs[i].Post(j);
            }
        }


        Array.ForEach(tbs, tb => tb.Complete());
        await Task.WhenAll(tbs.Select(tb => tb.Completion));
        batchBlock.Complete();
        await writer.Completion;
    }







}