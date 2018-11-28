using System;
using NUnrar.Common;

namespace NUnrar
{
    public class ConsoleRarExtractionListener : IRarExtractionListener
    {
        private long? entryTotal;
        private long partTotal;

        private int CreatePercentage(long n, long d)
        {
            return (int)(((double)n / (double)d) * 100);
        }

        public void OnFileEntryExtractionInitialized(string entryFileName, long? totalEntryCompressedBytes)
        {
            this.entryTotal = totalEntryCompressedBytes;
            Console.WriteLine("Initializing File Entry Extraction: " + entryFileName);
        }

        public void OnFilePartExtractionInitialized(string partFileName, long totalPartCompressedBytes)
        {
            this.partTotal = totalPartCompressedBytes;
            Console.WriteLine("Initializing File Part Extraction: " + partFileName);
        }

        public void OnCompressedBytesRead(long currentPartCompressedBytes, long currentEntryCompressedBytes)
        {
            Console.WriteLine("Read Compressed File Part Bytes: {0} Percentage: {1}%",
                currentPartCompressedBytes, CreatePercentage(currentPartCompressedBytes, partTotal));

            string percentage = entryTotal.HasValue ? CreatePercentage(currentEntryCompressedBytes,
                entryTotal.Value).ToString() : "Unknown";
            Console.WriteLine("Read Compressed File Entry Bytes: {0} Percentage: {1}%",
                currentEntryCompressedBytes, percentage);

        }

        public void OnInformation(string message)
        {
            Console.WriteLine(message);
        }
    }
}
