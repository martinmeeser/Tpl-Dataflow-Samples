using System;
using System.IO;
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
            await SimpleBlockSamples.CreateBlockAndPostMessagesAsync();
            await SimpleBlockSamples.ExecuteTransformBlockSampleAsync();

            // ************************************************
            // ** now check out how to link blocks to pass
            // ** messages between them
            // ************************************************
            await LinkingFilteringAndBroadcast.ExecuteSimpleLinkWithFilterSampleAsync();
            await LinkingFilteringAndBroadcast.ExecuteMultipleSuccessorsSampleAsync();
            await LinkingFilteringAndBroadcast.ExecuteBroadcastSampleAsync();

            // ************************************************
            // ** now we take a look at data aggregation patterns with the batch block
            // ************************************************
            await DataAggregation.ExecuteBatchBlockSampleAsync(isGready: true, batchSize: 5);
            await DataAggregation.ExecuteBatchBlockSampleAsync(isGready: false, batchSize: 5);

            // ************************************************
            // ** next set of samples is about async-ness and
            // ** parallism
            // ************************************************
            await AsyncBehaviorAndParallism.ExecuteSampleAsync();
            await AsyncBehaviorAndParallism.ExecuteTaskParallismExample();

            // ************************************************
            // ** the next sample is about back pressure
            // ************************************************
            await SimpleBackPressure.ExecuteSampleAsync();

            // ************************************************
            // ** the final sample is an examplary pipe line
            // ************************************************
            string path = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                "sample.csv");
            // await SimplePipeline.CreateSampleCsvFile(path, 100000000, 10);
            await SimplePipeline.ExecutePipelineSample(path);

        }



    }
}
