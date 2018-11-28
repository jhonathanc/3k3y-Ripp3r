using System.IO;
using System.Threading;
using NUnrar.Common;
#if THREEFIVE
using NUnrar.Headers;
#endif

namespace NUnrar.Reader
{
    public static class RarReaderExtensions
    {
#if !PORTABLE
        public static void WriteEntryTo(this RarReader reader, string filePath, CancellationToken cancellation)
        {
            using (Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write))
            {
                reader.WriteEntryTo(stream, cancellation);
            }
        }
        public static void WriteEntryTo(this RarReader reader, FileInfo filePath, CancellationToken cancellation)
        {
            using (Stream stream = filePath.Open(FileMode.Create))
            {
                reader.WriteEntryTo(stream, cancellation);
            }
        }

        /// <summary>
        /// Extract to specific directory, retaining filename
        /// </summary>
        public static void WriteEntryToDirectory(this RarReader reader, string destinationDirectory, CancellationToken cancellation,
            ExtractOptions options = ExtractOptions.Overwrite)
        {
            string destinationFileName = string.Empty;
            string file = Path.GetFileName(reader.Entry.FilePath);


            if (options.HasFlag(ExtractOptions.ExtractFullPath))
            {
                string folder = Path.GetDirectoryName(reader.Entry.FilePath);
                string destdir = Path.Combine(destinationDirectory, folder);
                if (!Directory.Exists(destdir))
                {
                    Directory.CreateDirectory(destdir);
                }
                destinationFileName = Path.Combine(destdir, file);
            }
            else
            {
                destinationFileName = Path.Combine(destinationDirectory, file);
            }

            reader.WriteEntryToFile(destinationFileName, cancellation, options);
        }

        /// <summary>
        /// Extract to specific file
        /// </summary>
        public static void WriteEntryToFile(this RarReader reader, string destinationFileName, CancellationToken cancellation,
            ExtractOptions options = ExtractOptions.Overwrite)
        {
            FileMode fm = FileMode.Create;

            if (!options.HasFlag(ExtractOptions.Overwrite))
            {
                fm = FileMode.CreateNew;
            }
            using (FileStream fs = File.Open(destinationFileName, fm))
            {
                reader.WriteEntryTo(fs, cancellation);
            }
        }
#endif
    }
}
