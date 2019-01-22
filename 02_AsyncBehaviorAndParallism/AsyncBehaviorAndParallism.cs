using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public class AsyncBehaviorAndParallism
{

    public static async Task ExecuteSampleAsync()
    {
        // execute messages one by one
        ActionBlock<string> ab1 = new ActionBlock<string>(s => WriteLine("a1", s));
        // also executes messages one by one (even beeing an async Task)
        ActionBlock<string> ab2 = new ActionBlock<string>(async s => await WriteLineAsync("a2", s));

        // increase the parallelism by using Option "MaxDegreeOfParallelism"
        ActionBlock<string> ab3 = new ActionBlock<string>(async s =>
            await WriteLineAsync("a3", s),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });
        ActionBlock<string> ab4 = new ActionBlock<string>(async s => await WriteLineAsync("a4", s), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });


        Parallel.For(0, 40, (i, state) =>
        {
            ab1.Post(i.ToString());
            ab2.Post(i.ToString());
            ab3.Post(i.ToString());
            ab4.Post(i.ToString());
        });


        IDataflowBlock[] dfbs = new IDataflowBlock[] { ab1, ab2, ab3, ab4 };
        Array.ForEach(dfbs, dfb => dfb.Complete());
        await Task.WhenAll(dfbs.Select(dfb => dfb.Completion));
    }

    ///<summary>
    ///</summary>
    public static async Task ExecuteTaskParallismExample()
    {
        // ***********************************************
        // ** This sample is from https://github.com/svick
        // ***********************************************

        var exclusiveScheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;

        var options = new ExecutionDataflowBlockOptions
        {
            TaskScheduler = exclusiveScheduler,
            MaxMessagesPerTask = 1
        };

        var blockA = new ActionBlock<int>(i => Console.WriteLine($"A {i}"), options);
        var blockB = new ActionBlock<int>(i => Console.WriteLine($"B {i}"), options);

        for (int i = 0; i < 5; i++)
        {
            blockA.Post(i);
            blockB.Post(i);
        }

        blockA.Complete();
        blockB.Complete();

        await Task.WhenAll(blockA.Completion, blockB.Completion);
    }

    private static void WriteLine(string name, string s)
    {
        Task.WaitAll(Task.Delay(1000));
        Console.WriteLine($"{name}\t[ThreadId: {Thread.CurrentThread.ManagedThreadId}]\t{s}");
    }

    private static async Task WriteLineAsync(string name, string s)
    {
        await Task.Delay(1000);
        await Console.Out.WriteLineAsync($"{name}\t[ThreadId: {Thread.CurrentThread.ManagedThreadId}]\t{s}");
    }

}