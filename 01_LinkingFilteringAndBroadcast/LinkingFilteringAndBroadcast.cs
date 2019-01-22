using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public class LinkingFilteringAndBroadcast
{

    public static async Task ExecuteSimpleLinkWithFilterSampleAsync()
    {
        TransformBlock<string, int?> transformBlock = new TransformBlock<string, int?>(
                s =>
                {
                    if (Int32.TryParse(s, out int result))
                    {
                        return result;
                    }
                    else
                        return null;
                });


        ActionBlock<int?> actionBlock = new ActionBlock<int?>((int? i) =>
        {
            Console.WriteLine(i.HasValue ? i.Value.ToString() : "null");
        });

        // ******************************
        // link the blocks (with a filter)
        // ******************************
        transformBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true },
            i => i.HasValue
            );

        transformBlock.Post("-1");
        transformBlock.Post("-2");
        transformBlock.Post("three");

        transformBlock.Complete();
        await actionBlock.Completion;
    }

    public static async Task ExecuteMultipleSuccessorsSampleAsync()
    {
        TransformBlock<string, int?> transformBlock = new TransformBlock<string, int?>(
                    s =>
                    {
                        if (Int32.TryParse(s, out int result))
                        {
                            return result;
                        }
                        else
                            return null;
                    });

        ActionBlock<int?> ab1 = new ActionBlock<int?>(async (int? i) => await WriteIntDelayed(10, "ab1", i));
        ActionBlock<int?> ab2 = new ActionBlock<int?>(
            async (int? i)
                => await WriteIntDelayed(500, "ab2", i)
            , new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
        ActionBlock<int?> ab3 = new ActionBlock<int?>(
            async (int? i)
                => await WriteIntDelayed(200, "ab3", i),
            new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });

        transformBlock.LinkTo(ab1, new DataflowLinkOptions { MaxMessages = 10, PropagateCompletion = true });
        transformBlock.LinkTo(ab2, new DataflowLinkOptions { PropagateCompletion = true });
        transformBlock.LinkTo(ab3, new DataflowLinkOptions { PropagateCompletion = true });

        Parallel.For(0, 30, (i, state) =>
        {
            transformBlock.Post(i.ToString());
        });

        transformBlock.Complete();
        await Task.WhenAll(ab1.Completion, ab2.Completion, ab3.Completion);
    }

    public static async Task ExecuteBroadcastSampleAsync()
    {
        BroadcastBlock<int?> bc = new BroadcastBlock<int?>(i => i);

        ActionBlock<int?> ab1 = new ActionBlock<int?>(i => Console.WriteLine($"ab1: {i}"));
        ActionBlock<int?> ab2 = new ActionBlock<int?>(i => Console.WriteLine($"ab2: {i}"));


        bc.LinkTo(ab1, new DataflowLinkOptions { PropagateCompletion = true }, i => i.HasValue && i.Value > 10);
        bc.LinkTo(ab2, new DataflowLinkOptions { PropagateCompletion = true }, i => i.HasValue && i.Value > 15);

        bc.Post(5);         // this message will be "lost" "behind" the broadcast block, since there is no block accepting it
        bc.Post(11);        // this message will override the before message
        bc.Post(16);        // finally this message will cause two outputs at console

        bc.Complete();
        await Task.WhenAll(ab1.Completion, ab2.Completion);
    }

    ///<summary>
    /// A litte helper method
    ///</summary>
    private static async Task WriteIntDelayed(int delay_ms, string name, int? i)
    {
        await Task.Delay(delay_ms);
        await Console.Out.WriteLineAsync(name + "\t" + (i.HasValue ? i.ToString() : "null"));
    }
}