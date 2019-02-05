using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public class LinkingFilteringAndBroadcast
{

    public static async Task ExecuteSimpleLinkWithFilterSampleAsync()
    {
        TransformBlock<string, int?> tb = new TransformBlock<string, int?>(
                s =>
                {
                    if (Int32.TryParse(s, out int result))
                    {
                        return result;
                    }
                    return null;
                });


        ActionBlock<int?> ab1 = new ActionBlock<int?>(i => Console.WriteLine(i.ToString()));

        // ******************************
        // link the blocks (with a filter)
        // ******************************
        tb.LinkTo(
            ab1
            , new DataflowLinkOptions { PropagateCompletion = true, MaxMessages=10 }
            , i => i.HasValue);

        // if you remove this line, the block can not complete (because the null message will never be consumed)
        tb.LinkTo(DataflowBlock.NullTarget<int?>(), i => !i.HasValue);

        tb.Post("-1");
        tb.Post("-2");
        tb.Post("three");

        tb.Complete();
        //actionBlock.Complete(); // when you uncomment this line, you may see none, one, two or three lines of output, but most likely none
        await ab1.Completion;
    }

    ///<summary>
    /// A litte helper method
    ///</summary>
    private static async Task WriteIntDelayed(int delay_ms, string name, int? i)
    {
        await Task.Delay(delay_ms);
        await Console.Out.WriteLineAsync(name + "\t" + (i.HasValue ? i.ToString() : "null"));
    }
    private static int counter = 0;
    public static async Task ExecuteMultipleSuccessorsSampleAsync()
    {
        TransformBlock<string, int?> tb = new TransformBlock<string, int?>(
                    s =>
                    {
                        if (Int32.TryParse(s, out int result))
                        {
                            return result;
                        }
                        else
                            return null;
                    });

        ActionBlock<int?> ab1 = new ActionBlock<int?>(async (int? i) => await WriteIntDelayed(10, $"ab1[{counter++}]", i));
        ActionBlock<int?> ab2 = new ActionBlock<int?>(
            async (int? i)
                => await WriteIntDelayed(500, $"ab2[{counter++}]", i)
            , new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
        ActionBlock<int?> ab3 = new ActionBlock<int?>(
            async (int? i)
                => await WriteIntDelayed(200, $"ab3[{counter++}]", i),
            new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });

        tb.LinkTo(ab1, new DataflowLinkOptions { MaxMessages = 10, PropagateCompletion = true }); // after ten messages, the block gets unliked and no propagation will happen
        tb.LinkTo(ab2, new DataflowLinkOptions { PropagateCompletion = true });
        tb.LinkTo(ab3, new DataflowLinkOptions { PropagateCompletion = true });

        // Parallel.For(0, 30, (i, state) =>
        // {
        //     tb.Post(i.ToString());
        // });

        for (int i = 0; i < 30; i++)
        {
            tb.Post(i.ToString());
        }

        tb.Complete();
        await tb.Completion.ContinueWith(t => ab1.Complete()); // without this line, the following line can not complete
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
        bc.Post(11);        // this message will override the previous message
        bc.Post(16);        // finally this message will cause two outputs at console

        bc.Complete();
        await Task.WhenAll(ab1.Completion, ab2.Completion);
    }


}