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
            // ************************************************
            // ** you can follow the samples step by step in 
            // ** the given order
            // ************************************************



            // ************************************************
            // ** first set of samples is about the very basics,
            // ** creating blocks, and sending and receiving 
            // ** messages
            // ************************************************
            // await SimpleBlockSamples.CreateBlockAndPostMessagesAsync();
            // await SimpleBlockSamples.ExecuteTransformBlockSampleAsync();

            // ************************************************
            // ** now check out how to link blocks to pass
            // ** messages between them
            // ************************************************
            // await LinkingFilteringAndBroadcast.ExecuteSimpleLinkWithFilterSampleAsync();
            // await LinkingFilteringAndBroadcast.ExecuteMultipleSuccessorsSampleAsync();
            // await LinkingFilteringAndBroadcast.ExecuteBroadcastSampleAsync();

            // ************************************************
            // ** next set of samples is about async-ness and
            // ** parallism
            // ************************************************
            // await AsyncBehaviorAndParallism.ExecuteSampleAsync();
            // await AsyncBehaviorAndParallism.ExecuteTaskParallismExample();

            // ************************************************
            // ** now we take a look at data aggregation patterns with the batch block
            await DataAggregation.ExecuteBatchBlockSampleAsync(isGready: true, batchSize: 5);
            await DataAggregation.ExecuteBatchBlockSampleAsync(isGready: false, batchSize: 5);


            // ************************************************
            // ** the next sample is about the way datablocks works
            // ** in gernel, how to "think" when using datablocks
            // ************************************************
            await SimpleBackPressure.ExecuteSampleAsync();

            //await SimplePipeline.CreateSampleFile("/home/martin/sample.csv", 10000000, 10);

        }



    }
}
