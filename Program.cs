using System;
using System.Threading.Tasks;

namespace tpldf_samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.WaitAll(MainAsync());
        }

        static async Task MainAsync()
        {
            await SimpleBlockSamples.CreateBlockAndPostMessagesAsync();
            SimpleBlockSamples.CreateTransformBlockAsync();
            await SimpleBlockSamples.CreateBatchBlockAndSendAndReceiveMessages();
        }



    }
}
