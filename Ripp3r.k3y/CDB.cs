using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Ripp3r
{
    class CDB
    {
        private struct SenseEntry
        {
            public byte SK;
            public byte ASC;
            public byte ASCQ;
            public Native.SenseCodes Code;

            public SenseEntry(byte sk, byte asc, byte ascq, Native.SenseCodes code)
            {
                SK = sk;
                ASC = asc;
                ASCQ = ascq;
                Code = code;
            }

            public bool IsSense(byte sk, byte asc, byte ascq)
            {
                return (SK == sk) && (ASC == asc) && (ASCQ == ascq);
            }

            public Native.SenseCodes Sense
            {
                get { return Code; }
            }
        }

        private static readonly IList<SenseEntry> senseEntries = new ReadOnlyCollection<SenseEntry>
            (new[]
                {
                    new SenseEntry(0x00, 0x00, 0x00, Native.SenseCodes.SENSE_OK),
                    new SenseEntry(0x02, 0x04, 0x01, Native.SenseCodes.SENSE_BECOMING_READY),
                    new SenseEntry(0x02, 0x3A, 0x00, Native.SenseCodes.SENSE_MEDIUM_NOT_PRESENT)
                });
        /*****************************************************************************
        *****************************************************************************/

        public bool Open(UsbDevice device)
        {
            slimDriveHandle = Native.CreateFile(device.Device, FileAccess.ReadWrite, FileShare.ReadWrite, 0, FileMode.Open, (int)FileAttributes.Device | 0x40000000,
                              IntPtr.Zero);

            if (slimDriveHandle.IsInvalid)
            {
                Win32Exception ex = new Win32Exception();
                Trace.WriteLine("Open Pointer failed: " + ex.Message);
            }
            return !slimDriveHandle.IsInvalid;
        }

        private void OpenHandle(string driveLetter, CancellationToken cts)
        {
            TaskEx.Run(() => slimDriveHandle =
                             Native.CreateFile(driveLetter,
                                               FileAccess.ReadWrite,
                                               FileShare.None,
                                               0,
                                               FileMode.Open,
                                               FILE_FLAG_NO_BUFFERING,
                                               IntPtr.Zero), cts).Wait(); // Max one second
        }

        public bool Open(string driveLetter)
        {
            return Open(driveLetter, false);
        }

        private bool Open(string driveLetter, bool retry)
        {
            if (driveLetter[0] != Path.DirectorySeparatorChar)
            {
                driveLetter = @"\\.\" + driveLetter; 
            }

            if (retry)
            {
                // Eject CD?
                Native.mciSendString(string.Format("open {0} type CDAudio alias drivePS3", driveLetter), null, 0, IntPtr.Zero);
                Native.mciSendString("set drivePS3 door open", null, 0, IntPtr.Zero);
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(500));

            OpenHandle(driveLetter, cts.Token);

            if (retry)
            {
                Native.mciSendString("set drivePS3 door closed", null, 0, IntPtr.Zero);
            }

            if (slimDriveHandle.IsInvalid)
            {
                if (!cts.IsCancellationRequested)
                {
                    Win32Exception ex = new Win32Exception();
                    Trace.WriteLine("Open Pointer failed: " + ex.Message);
                }
                else
                {
                    if (!retry) return Open(driveLetter, true);

                    Interaction.Instance.ReportMessage(
                        "Drive detection timeout, did you remove the disc from the drive?", ReportType.Fail);
                }
            }
            return !slimDriveHandle.IsInvalid;
        }
        /*****************************************************************************
        *****************************************************************************/
        public void Close()
        {
            if(!slimDriveHandle.IsInvalid)
                slimDriveHandle.Close();
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoInquiry(byte[] theResponse)
        {
            byte[] theCDB  = { 0x12, 0x00, 0x00, 0x00, 0x3C, 0x00 };
            theCDB[4] = (byte)theResponse.Length;
            return DoCDB(theCDB, (uint)theResponse.Length, theResponse, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoStartStop()
        {
            byte[] theCDB = { 0x1B, 0x00, 0x00, 0x00, 0x02, 0x00 };
            return DoCDB(theCDB, 0, null, SCSI_IOCTL_DATA_NONE);
        }
        /*****************************************************************************
        *****************************************************************************/
        public bool DoTestUnitReady()
        {
            bool result = false;
            byte[] theCDB = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            if (DoCDB(theCDB, 0, new byte[0], SCSI_IOCTL_DATA_NONE) == IoResult.OK)
            {
                result = true;
            }
            return result;
        }
        /*****************************************************************************
        *****************************************************************************/
        public Native.SenseCodes GetLastSense()
        {
            foreach (SenseEntry entry in senseEntries)
            {
                if (entry.IsSense (senseData[0x02], senseData[0x0C], senseData[0x0D]))
                {
                    return entry.Sense;
                }
            }

            return Native.SenseCodes.SENSE_UNKNOWN;
        }
        /*****************************************************************************
        *****************************************************************************/
        public void DoRequestSense()
        {
            byte[] data = new byte[0x0E];
            byte[] theCDB = { 0x03, 0x00, 0x00, 0x00, 0x0E, 0x00 };
            DoCDB(theCDB, 0x0E, data, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoModeSelect(byte[] payload)
        {
            byte[] theCDB = { 0x55, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00 };
            theCDB[8] = (byte)payload.Length;
            return DoCDB(theCDB, (uint)payload.Length, payload, SCSI_IOCTL_DATA_OUT);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoWriteBuffer(byte mode, byte buffer, byte[] payload)
        {
            byte[] theCDB = { 0x3B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            theCDB[1] = mode;
            theCDB[2] = buffer;
            theCDB[6] = (byte)(payload.Length >> 16);
            theCDB[7] = (byte)(payload.Length >> 8);
            theCDB[8] = (byte)(payload.Length);
            return DoCDB(theCDB, (uint)payload.Length, payload, SCSI_IOCTL_DATA_OUT);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoReadBuffer(byte buffer, byte[] payload)
        {
            byte[] theCDB = { 0x3C, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            theCDB[2] = buffer;
            theCDB[6] = (byte)(payload.Length >> 16);
            theCDB[7] = (byte)(payload.Length >> 8);
            theCDB[8] = (byte)(payload.Length);
            return DoCDB(theCDB, (uint)payload.Length, payload, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoModeSense(byte pageCode, byte[] payload)
        {
            byte[] theCDB = { 0x5A, 0x00, 0x3B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3A, 0x00 };
            theCDB[2] = pageCode;
            theCDB[8] = (byte)payload.Length;
            return DoCDB(theCDB, (uint)payload.Length, payload, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoReadCapacity(byte[] payload)
        {
            byte[] theCDB = { 0x25, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            return DoCDB(theCDB, 0x08, payload, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************
        *****************************************************/
        public IoResult DoSendKey(byte KeyFormat, byte[] payload)
        {
            byte[] theCDB = { 0xA3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x00, 0x00, 0x00};
            theCDB[8]  = (byte)(payload.Length >> 8);
            theCDB[9]  = (byte)(payload.Length);
            theCDB[10] = KeyFormat;
            return DoCDB(theCDB, (uint)payload.Length, payload, SCSI_IOCTL_DATA_OUT);
        }
        /*****************************************************
        *****************************************************/
        public IoResult DoReportKey(byte KeyFormat, byte[] payload)
        {
            byte[] theCDB = { 0xA4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x00, 0x00, 0x00};
            theCDB[8] = (byte)(payload.Length >> 8);
            theCDB[9] = (byte)(payload.Length);
            theCDB[10] = KeyFormat;
            return DoCDB(theCDB, (uint)payload.Length, payload, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************
        *****************************************************/
        public IoResult DoReadPICZone(byte[] payload)
        {
            byte[] theCDB = {0xAD, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x73, 0x00, 0x00};
            return DoCDB(theCDB, 0x73, payload, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoUnknownE1(byte[] header, byte[] payload)
        {
            byte[] theCDB = { 0xE1, 0x00, 0x54, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            Array.Copy(header, 0x00, theCDB, 0x04, 0x08);
            return DoCDB(theCDB, 0x54, payload, SCSI_IOCTL_DATA_OUT);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoUnknownE0(byte[] header, byte[] payload)
        {
            byte[] theCDB = { 0xE0, 0x00, 0x34, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            Array.Copy(header, 0x00, theCDB, 0x04, 0x08);
            return DoCDB(theCDB, 0x34, payload, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoGetEventStatusNotification(byte[] payload)
        {
            byte[] theCDB = { 0x4A, 0x01, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x08, 0x00 };
            return DoCDB(theCDB, 8, payload, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoGetConfiguration(byte[] payload)
        {
            byte[] theCDB = { 0x46, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00 };
            return DoCDB(theCDB, 8, payload, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoReadDVDStruct(UInt32 address, byte layer, byte format, UInt16 allocLength, byte control, byte[] payload)
        {
            byte[] theCDB = { 0xAD, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC0 };
            theCDB[2] = (byte)(address >> 24);
            theCDB[3] = (byte)(address >> 16);
            theCDB[4] = (byte)(address >> 8);
            theCDB[5] = (byte)(address);
            theCDB[6] = layer;
            theCDB[7] = format;
            theCDB[8] = (byte)(allocLength >> 8);
            theCDB[9] = (byte)(allocLength);
            theCDB[11] = control;
            return DoCDB(theCDB, (uint)payload.Length, payload, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoRead(byte[] payload, UInt32 lba, UInt32 numSectors)
        {
            byte[] theCDB = { 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC0 };
            theCDB[2] = (byte)(lba >> 24);
            theCDB[3] = (byte)(lba >> 16);
            theCDB[4] = (byte)(lba >> 8);
            theCDB[5] = (byte)(lba);

            theCDB[7] = (byte)(numSectors >> 8);
            theCDB[8] = (byte)(numSectors);

            return DoCDB(theCDB, numSectors * Utilities.SectorSize, payload, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoSetCDROMSpeed(UInt16 speed)
        {
            byte[] theCDB = { 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            theCDB[2] = (byte)(speed >> 8);
            theCDB[3] = (byte)(speed);
            return DoCDB(theCDB, 0, new byte[0], SCSI_IOCTL_DATA_NONE);
        }
        /*****************************************************************************
        *****************************************************************************/
        public IoResult DoRead12(byte[] payload, UInt32 lba, UInt32 numSectors)
        {
            byte[] theCDB = {0xA8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

            theCDB[2] = (byte)(lba >> 24);
            theCDB[3] = (byte)(lba >> 16);
            theCDB[4] = (byte)(lba >>  8);
            theCDB[5] = (byte)(lba);

            theCDB[6] = (byte)(numSectors >> 24);
            theCDB[7] = (byte)(numSectors >> 16);
            theCDB[8] = (byte)(numSectors >> 8);
            theCDB[9] = (byte)(numSectors);

            return DoCDB(theCDB, numSectors * Utilities.SectorSize, payload, SCSI_IOCTL_DATA_IN);
        }
        /*****************************************************************************
        *****************************************************************************/
        private IoResult DoCDB(byte[] cdb, long transferLength, byte[] transferBuffer, byte direction)
        {
            byte cdbLen = (byte)cdb.Length;
            IntPtr transbuff = IntPtr.Zero;
            uint translenAligned = 0; //better set some value in case of direction 2 ?!!?!?
            if (direction != SCSI_IOCTL_DATA_NONE)
            {
                translenAligned = (uint)((transferBuffer.Length + 3) & 0xFFFFFFFC); //roundup for both directions
            }
            transbuff = Marshal.AllocHGlobal((int)translenAligned);
            if (direction == SCSI_IOCTL_DATA_OUT) Marshal.Copy(transferBuffer, 0, transbuff, (int)transferLength);
            IoResult result;

            Native.SCSI_PASS_THROUGH_DIRECT_WITH_BUFFERS sptdwb = new Native.SCSI_PASS_THROUGH_DIRECT_WITH_BUFFERS
                {
                    sptd =
                        {
                            Length = (ushort) Marshal.SizeOf(typeof (Native.SCSI_PASS_THROUGH_DIRECT)),
                            ScsiStatus = 0,
                            PathId = 0,
                            TargetId = 0,
                            Lun = 0,
                            CdbLength = cdbLen,
                            SenseInfoLength = 32,
                            DataIn = direction,
                            DataTransferLength = translenAligned,
                            TimeOutValue = 20,
                            DataBuffer = transbuff,
                            SenseInfoOffset =
                                (uint) Marshal.OffsetOf(typeof (Native.SCSI_PASS_THROUGH_DIRECT_WITH_BUFFERS), "sense"),
                            Cdb = new byte[16] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
                        }
                };

            Array.Copy(cdb, sptdwb.sptd.Cdb, cdbLen);
            IntPtr inoutbuf = Marshal.AllocHGlobal(Marshal.SizeOf(sptdwb));
            Marshal.StructureToPtr(sptdwb, inoutbuf, true);
            byte iook = 0;
            for (byte i = 0; i < 0x10; i++) //sometimes the ioctrl fails randomly?!?! just retry few times and who cares
            {
                uint returned;
                if (!Native.DeviceIoControl(slimDriveHandle,
                                            IOCTL_SCSI_PASS_THROUGH_DIRECT,
                                            inoutbuf,
                                            (uint) Marshal.SizeOf(sptdwb),
                                            inoutbuf,
                                            (uint) Marshal.SizeOf(sptdwb),
                                            out returned,
                                            IntPtr.Zero)) continue;

                iook = 1;
                break;
            }
            if (iook != 0)
            {
                Marshal.PtrToStructure(inoutbuf, sptdwb);
                Array.Copy(sptdwb.sense, senseData, 0x20);//get sense data
                /* Check for sense data */
                if (sptdwb.sense[2] != 0 || sptdwb.sense[12] != 0 || sptdwb.sense[13] != 0)
                {
                    result = IoResult.SENSE;

                }
                else
                {
                    if (direction == SCSI_IOCTL_DATA_IN) Marshal.Copy(transbuff, transferBuffer, 0, (int)transferLength);
                    result = IoResult.OK;
                }
            }

            else
            {
                Win32Exception ex = new Win32Exception();
                string lastErr = ex.ToString();
                Debug.WriteLine(lastErr);
                result = IoResult.FAIL;
            }

            Marshal.FreeHGlobal(transbuff);
            Marshal.FreeHGlobal(inoutbuf);
            return result;
        }
        /*****************************************************************************
        *****************************************************************************/
        public void GetSense(byte[] sense)
        {
            Array.Copy(senseData, sense, 0x20);
        }
        /*****************************************************************************
        *****************************************************************************/
        private const int FILE_FLAG_NO_BUFFERING = 0x20000000;
        /*****************************************************************************
        *****************************************************************************/
        private SafeFileHandle slimDriveHandle;
        private const byte SCSI_IOCTL_DATA_OUT              = 0;
        private const byte SCSI_IOCTL_DATA_IN               = 1;
        private const byte SCSI_IOCTL_DATA_NONE             = 2;
        private const int IOCTL_SCSI_PASS_THROUGH           = 0x4D004;
        private const int IOCTL_SCSI_PASS_THROUGH_DIRECT    = 0x4D014;

        private const byte ASMEDIA_ATA_MODE                 = 0x01;
        private const byte ASMEDIA_ATAPI_MODE               = 0x02;

        private readonly byte[] senseData = new byte[0x20];

        public enum IoResult { FAIL, OK, SENSE};

        private const UInt32 bulkReadSectors = 0x20;
        private const UInt32 bulkReadSectorMask = bulkReadSectors - 1;
        private const UInt32 bulkReadMask = ~(bulkReadSectors - 1);
        private const UInt32 bulkReadLength = (UInt32)(Utilities.SectorSize * bulkReadSectors);
    }
}
