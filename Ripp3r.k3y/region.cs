using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Ripp3r
{
    public abstract class Region
    {
        public UInt32 Start { get; protected internal set; }
        public UInt32 Length { get; protected internal set; }
        protected UInt32 RegionIdx { get; private set; }
        public UInt32 End { get { return Start + Length - 1; } }

        public byte[] SourceHash { get; protected set; }
        public byte[] DestinationHash { get; protected set; }

        protected Region(UInt32 region)
        {
            RegionIdx = region;
        }

        public virtual Task CopyRegion(CancellationToken cancellation, Stream input, Stream output)
        {
            return null;
        }

        public abstract string Type { get; }
    }

    class PlainRegion : Region
    {
        private readonly bool DoDecrypt;
        private readonly bool doBuild;
        private readonly bool IsValidHash;

        public PlainRegion(uint region, bool dodecrypt, bool dobuild, bool isValidHash, uint StartSector, uint Next)  
            : base(region)
        {
            DoDecrypt = dodecrypt;
            doBuild = dobuild;
            IsValidHash = isValidHash;
            Start = StartSector;
            Length = Next - StartSector + 1;
        }

        public async override Task CopyRegion(CancellationToken cancellation, Stream input, Stream output)
        {
            using (MD5 md5 = MD5.Create())
            {
                int i;
                if (RegionIdx == 0 && Start == 0)
                {
                    byte[] fts = new byte[0x10*Utilities.SectorSize];
                    await input.ReadAsync(fts, 0, fts.Length);

                    if (doBuild && IsValidHash)
                        Array.Copy(DoDecrypt ? Utilities.Decrypted3KBuild : Utilities.Encrypted3KBuild, 0, fts, 0xf70, 0x10);
                    else if (doBuild)
                        Array.Copy(DoDecrypt ? Utilities.Decrypted3KFailedBuild : Utilities.Encrypted3KFailedBuild, 0, fts, 0xf70, 0x10);
                    else
                        Array.Copy(DoDecrypt ? Utilities.Decrypted3KISO : Utilities.Encrypted3KISO, 0, fts, 0xf70, 0x10);

                    await output.WriteAsync(fts, 0, fts.Length);
                    md5.TransformBlock(fts, 0, fts.Length, null, 0);
                    if (cancellation.IsCancellationRequested) return;
                    Interaction.Instance.ReportProgress((int)(fts.Length / Utilities.SectorSize));
                }
                byte[] buffer = new byte[Utilities.SectorSize];
                for (i = (int) (RegionIdx == 0 && Start == 0 ? 0x10 : Start); i <= End; i++)
                {
                    await input.ReadAsync(buffer, 0, (int)Utilities.SectorSize);
                    await output.WriteAsync(buffer, 0, (int)Utilities.SectorSize);
                    md5.TransformBlock(buffer, 0, (int)Utilities.SectorSize, null, 0);
                    if (cancellation.IsCancellationRequested) return;
                    Interaction.Instance.ReportProgress(i);
                }
                md5.TransformFinalBlock(new byte[0], 0, 0);
                SourceHash = DestinationHash = md5.Hash;
            }
            Interaction.Instance.ReportProgress((int)(Start + Length));
        }

        public override string Type
        {
            get { return "plain"; }
        }
    }

    abstract class CryptoRegion : Region
    {
        protected readonly byte[] DiskKey;

        protected CryptoRegion(UInt32 region, byte[] d1, byte[] d2, UInt32 StartSector, UInt32 Next)
            : base(region)
        {
            byte[] user_key_erk  = { 0x38, 0x0B, 0xCF, 0x0B, 0x53, 0x45, 0x5B, 0x3C, 0x78, 0x17, 0xAB, 0x4F, 0xA3, 0xBA, 0x90, 0xED };
            byte[] user_key_riv  = { 0x69, 0x47, 0x47, 0x72, 0xAF, 0x6F, 0xDA, 0xB3, 0x42, 0x74, 0x3A, 0xEF, 0xAA, 0x18, 0x62, 0x87 };

            Start = StartSector + 1;
            Length = Next - Start;
            DiskKey = new byte[0x10];
            ODD.AESEncrypt(user_key_erk, user_key_riv, d1, 0, 0x10, DiskKey, 0);
        }
    }

    internal class QueuedItem
    {
        public long Offset;
        public int Sectors;
        public byte[] Buffer;
    }

    internal class CryptedRegion : BaseCryptedRegion
    {
        public CryptedRegion(uint region, byte[] d1, byte[] d2, uint StartSector, uint Next, bool isCompressed)
            : base(region, d1, d2, StartSector, Next, isCompressed)
        {
        }

        protected override void Crypt(Aes aes, byte[] key, byte[] iv, byte[] source, int sourceOffset, int sourceLength, byte[] dest, int destOffset)
        {
            ODD.AESDecrypt(aes, key, iv, source, sourceOffset, sourceLength, dest, destOffset);
        }

        public override string Type
        {
            get { return "crypted"; }
        }
    }

    internal class DecryptedRegion : BaseCryptedRegion
    {
        public DecryptedRegion(uint region, byte[] d1, byte[] d2, uint StartSector, uint Next, bool isCompressed)
            : base(region, d1, d2, StartSector, Next, isCompressed)
        {
        }

        protected override void Crypt(Aes aes, byte[] key, byte[] iv, byte[] source, int sourceOffset, int sourceLength, byte[] dest, int destOffset)
        {
            ODD.AESEncrypt(key, iv, source, sourceOffset, sourceLength, dest, destOffset);
        }

        public override string Type
        {
            get { return "decrypted"; }
        }
    }

    abstract class BaseCryptedRegion : CryptoRegion
    {
        private readonly int amountOfCores;
        private const int amountOfSectorsToRead = 0x10;
        private const int buffersPerCore = 16;

        protected BaseCryptedRegion(UInt32 region, byte[] d1, byte[] d2, UInt32 StartSector, UInt32 Next, bool isCompressed)
            : base(region, d1, d2, StartSector, Next)
        {
            int maxCores = Interaction.Instance.AmountOfCores != 0
                               ? Math.Min(Interaction.Instance.AmountOfCores, Environment.ProcessorCount)
                               : Environment.ProcessorCount;
            amountOfCores = isCompressed && maxCores > 1 ? maxCores - 1 : maxCores;

            // Create the maximum amount of buffers to use
            for (int i = 0; i < amountOfCores*buffersPerCore; i++)
                availableBuffers.Add(new byte[amountOfSectorsToRead * Utilities.SectorSize]);
        }

        private readonly BlockingCollection<byte[]> availableBuffers = new BlockingCollection<byte[]>(); 

        private readonly BlockingCollection<QueuedItem> readingCollection =
            new BlockingCollection<QueuedItem>();

        private bool readingComplete;

        private readonly ConcurrentDictionary<long, QueuedItem> writingCollection = new ConcurrentDictionary<long, QueuedItem>();

        private MD5 source;
        private MD5 dest;

        public async override Task CopyRegion(CancellationToken cancellationToken, Stream input, Stream output)
        {
            source = MD5.Create();
            dest = MD5.Create();

            try
            {
                Task writeTask = TaskEx.Run(() => Write(cancellationToken, output));
                Task readTask = TaskEx.Run(() => Read(cancellationToken, input));

                // Only one crypt action if there are not that many sectors
                Action[] actions =
                    Enumerable.Repeat<Action>(() => CryptRegion(cancellationToken), Length < 24 ? 1 : amountOfCores)
                              .ToArray();
                Task[] tasks = new Task[actions.Length];
                for (int i = 0; i < tasks.Length; i++)
                    tasks[i] = TaskEx.Run(actions[i], cancellationToken);

                await readTask;
                await TaskEx.WhenAll(tasks); // Wait for all decrypting tasks to be finished

                readingComplete = true;
                await writeTask; // Wait for the write task to be finished

                source.TransformFinalBlock(new byte[0], 0, 0);
                dest.TransformFinalBlock(new byte[0], 0, 0);
                SourceHash = source.Hash;
                DestinationHash = dest.Hash;
            }
            finally
            {
                availableBuffers.Dispose();
                readingCollection.Dispose();

                source.Dispose();
                dest.Dispose();
            }
        }

        private async Task Read(CancellationToken cancellation, Stream stream)
        {
            try
            {
                for (int i = 0; i < Length; i += amountOfSectorsToRead)
                {
                    if (cancellation.IsCancellationRequested) break;

                    // Read blocks and add them to the queue
                    byte[] buffer = availableBuffers.Take(cancellation);

                    long offset = stream.Position;
                    int amount = await stream.ReadAsync(buffer, 0, buffer.Length);
                    source.TransformBlock(buffer, 0, buffer.Length, null, 0);
                    readingCollection.Add(new QueuedItem {Offset = offset, Buffer = buffer, Sectors = (int) (amount/Utilities.SectorSize)});
                }
                readingCollection.CompleteAdding();
            }
            catch (InvalidOperationException)
            {
                // Collection marked as completed, can't fetch anymore (ugly!)
            }
            catch (OperationCanceledException)
            {
                // User pressed cancel
            }
        }

        private async Task Write(CancellationToken cancellation, Stream output)
        {
            try
            {
                int done = (int) Start;
                while (!cancellation.IsCancellationRequested)
                {
                    if (readingComplete && writingCollection.Count == 0) break;
                    long offset = output.Position;

                    // Find item at current offset
                    if (!writingCollection.ContainsKey(offset))
                    {
                        await TaskEx.Delay(100);
                        continue;
                    }

                    QueuedItem item;
                    if (!writingCollection.TryRemove(offset, out item)) continue;

                    await output.WriteAsync(item.Buffer, 0, (int) (item.Sectors * Utilities.SectorSize));
                    dest.TransformBlock(item.Buffer, 0, (int)(item.Sectors * Utilities.SectorSize), null, 0);

                    Array.Clear(item.Buffer, 0, item.Buffer.Length);
                    availableBuffers.Add(item.Buffer);

                    done += item.Sectors;
                    Interaction.Instance.ReportProgress(done);
                }
            }
            catch (InvalidOperationException)
            {
                // Collection marked as completed, can't fetch anymore (ugly!)
            }
            catch (OperationCanceledException)
            {
                // User pressed cancel
            }
        }

        private void CryptRegion(CancellationToken cancellation)
        {
            using (Aes aes = ODD.CreateAes())
            {
                try
                {
                    while (!cancellation.IsCancellationRequested)
                    {
                        QueuedItem item = readingCollection.Take(cancellation);

                        for (int i = 0; i < item.Sectors; i++)
                        {
                            // Decrypt the buffer
                            int sector_index = (int) (item.Offset/Utilities.SectorSize) + i;
                            byte[] sector_iv = new byte[0x10];
                            for (int j = 0; j < 0x10; j++)
                            {
                                sector_iv[16 - j - 1] = (byte) (sector_index & 0xFF);
                                sector_index = sector_index >> 8;
                            }
                            Crypt(aes, DiskKey, sector_iv, item.Buffer, (int) (i*Utilities.SectorSize),
                                  (int) Utilities.SectorSize, item.Buffer, (int) (i*Utilities.SectorSize));
                        }
                        writingCollection[item.Offset] = item;
                    }
                }
                catch (InvalidOperationException)
                {
                    // Collection marked as completed, can't fetch anymore (ugly!)
                }
                catch (OperationCanceledException)
                {
                    // User pressed cancel
                }
            }
        }

        protected abstract void Crypt(Aes aes, byte[] key, byte[] iv, byte[] source, int sourceOffset, int sourceLength, byte[] dest, int destOffset);
    }
}
