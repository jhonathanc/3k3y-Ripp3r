using System.IO;
using System.Threading;
using NUnrar.Common;
#if THREEFIVE
using NUnrar.Headers;
#endif

namespace NUnrar.Archive
{

    public static class RarArchiveEntryExtensions
    {
#if !PORTABLE
        /// <summary>
        /// Extract to specific directory, retaining filename
        /// </summary>
        public static void WriteToDirectory(this RarArchiveEntry entry, string destinationDirectory, CancellationToken cancellation,
            IRarExtractionListener listener,
            ExtractOptions options = ExtractOptions.Overwrite)
        {
            string destinationFileName = string.Empty;
            string file = Path.GetFileName(entry.FilePath);


            if (options.HasFlag(ExtractOptions.ExtractFullPath))
            {

                string folder = Path.GetDirectoryName(entry.FilePath);
                destinationDirectory = Path.Combine(destinationDirectory, folder);
                destinationFileName = Path.Combine(destinationDirectory, file);
            }
            else
            {
                destinationFileName = Path.Combine(destinationDirectory, file);
            }
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            entry.WriteToFile(destinationFileName, cancellation, listener, options);
        }

        /// <summary>
        /// Extract to specific directory, retaining filename
        /// </summary>
        public static void WriteToDirectory(this RarArchiveEntry entry, string destinationPath, CancellationToken cancellation,
            ExtractOptions options = ExtractOptions.Overwrite)
        {
            entry.WriteToDirectory(destinationPath, cancellation, new NullRarExtractionListener(), options);
        }

        /// <summary>
        /// Extract to specific file
        /// </summary>
        public static void WriteToFile(this RarArchiveEntry entry, string destinationFileName, CancellationToken cancellation,
                        IRarExtractionListener listener,
            ExtractOptions options = ExtractOptions.Overwrite)
        {
            FileMode fm = FileMode.Create;

            if (!options.HasFlag(ExtractOptions.Overwrite))
            {
                fm = FileMode.CreateNew;
            }
            using (FileStream fs = File.Open(destinationFileName, fm))
            {
                entry.WriteTo(fs, cancellation, listener);
            }
        }

        /// <summary>
        /// Extract to specific file
        /// </summary>
        public static void WriteToFile(this RarArchiveEntry entry, string destinationFileName, CancellationToken cancellation,
           ExtractOptions options = ExtractOptions.Overwrite)
        {
            entry.WriteToFile(destinationFileName, cancellation, new NullRarExtractionListener(), options);
        }
#endif

        public static void WriteTo(this RarArchiveEntry entry, Stream stream, CancellationToken cancellation)
        {
            entry.WriteTo(stream, cancellation, new NullRarExtractionListener());
        }
    }
}
