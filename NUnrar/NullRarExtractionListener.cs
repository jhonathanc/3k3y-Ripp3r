
using NUnrar.Common;

namespace NUnrar
{
    public class NullRarExtractionListener : IRarExtractionListener
    {
        public void OnFileEntryExtractionInitialized(string entryFileName, long? totalEntryCompressedBytes)
        {
        }

        public void OnFilePartExtractionInitialized(string partFileName, long totalPartCompressedBytes)
        {
        }

        public void OnCompressedBytesRead(long currentPartCompressedBytes, long currentEntryCompressedBytes)
        {
        }

        public void OnInformation(string message)
        {
        }
    }
}
