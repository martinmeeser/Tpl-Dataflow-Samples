using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

class SimpleBlockSamples
{
    /// <summary>
    /// This is just a little helper method.
    /// Note that all ITargetBlock<T> are able to receive messages.
    /// </summary>
    private static async Task SendDelayedMessageAsync(string message, int delay_ms, ITargetBlock<string> targetBlock)
    {
        await Task.Delay(delay_ms);
        await targetBlock.SendAsync(message);
    }

    /// <summary>
    /// A first sample data block - an ActionBlock - performs an Action<T> for each message.
    /// </summary>
    ///
    public static async Task CreateBlockAndPostMessagesAsync()
    {
        // ************************************************************ 
        // performs an Action<T> for each message
        // ************************************************************
        ActionBlock<string> actionBlock = new ActionBlock<string>((string s) =>
        {
            Console.WriteLine(s);
        });

        // use this to add messages to the block's buffer
        actionBlock.Post("Hello"); // Post will block until it is determined wether the item item could be added or not

        // you can also asyncronously add items and do not block while waiting for a result
        await actionBlock.SendAsync("World");

        await SendDelayedMessageAsync("Hello", 2000, actionBlock);
        await SendDelayedMessageAsync("Data", 3000, actionBlock);
        await SendDelayedMessageAsync("Blocks", 1000, actionBlock);
        await SendDelayedMessageAsync("!", 100, actionBlock);

        actionBlock.Complete();
        await actionBlock.Completion;
    }

    /// <summary>
    /// A simple TransformBlock sample.
    /// </summary>
    public static async Task ExecuteTransformBlockSampleAsync()
    {
        TransformBlock<string, int?> transformBlock = new TransformBlock<string, int?>((string s) =>
        {
            if (Int32.TryParse(s, out int result))
            {
                return result;
            }
            return null; // I always use boxed types
        });

        ITargetBlock<string> targetBlock = transformBlock;
        targetBlock.Post("1");
        targetBlock.Post("2");

        ISourceBlock<int?> sourceBlock = transformBlock;
        Console.WriteLine(sourceBlock.Receive());
        Console.WriteLine(sourceBlock.Receive());

        transformBlock.Complete();
        await transformBlock.Completion;
    }

    public static async Task CreateBatchBlockAndSendAndReceiveMessagesAsync()
    {
        BatchBlock<string> batchBlock = new BatchBlock<string>(4);
        ISourceBlock<string[]> batchSourceBlock = batchBlock; // just to be clear ...

        await SendDelayedMessageAsync("Hello", 100, batchBlock);
        await SendDelayedMessageAsync("Data", 200, batchBlock);
        await SendDelayedMessageAsync("Blocks", 400, batchBlock);
        await SendDelayedMessageAsync("!", 100, batchBlock);

        Console.WriteLine(batchBlock.Receive()); // one message  gets read from the outout queue, one message remains
    }

}