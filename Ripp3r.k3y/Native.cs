using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
namespace Ripp3r
{
    public static class Native
    {
        #region Constants

        public const int WM_DEVICECHANGE = 0x0219;

        public static Guid MediaArrival = Guid.Parse("A5DCBF10-6530-11D2-901F-00C04FB951ED");
        public static Guid WCE_USB_SH_GUID = new Guid(0x25dbce51, 0x6c8f, 0x4a72,
                                                      0x8a, 0x6d, 0xb5, 0x4c, 0x2b, 0x4f, 0xc8, 0x35);

        #endregion

        #region Enums

        [Flags]
        public enum DeviceNotifyHandle
        {
            Window = 0,
            Service = 1,
            AllInterfaceClasses = 4
        }

        public enum DeviceBroadcastType
        {
            DeviceChanged = 7, // DBT_DEVNODES_CHANGED

            Arrival = 0x8000,
            QueryRemove,
            QueryRemoveFailed,
            RemovePending,
            RemoveComplete,
            DeviceTypeSpecific,
        }

        public enum DeviceType
        {
            Oem = 0,
            Devnode,
            Volume,
            Port,
            Net,
            Interface
        }

        [Flags]
        public enum BroadcastFlags
        {
            Hdd = 0,
            Media,
            Net
        }

        public enum SenseCodes
        {
            SENSE_OK,                   /* 0,00,0 */
            SENSE_BECOMING_READY,       /* 2,04,1 */
            SENSE_MEDIUM_NOT_PRESENT,   /* 2,3A,1 */

            SENSE_UNKNOWN               /* Not real */
        };

        #endregion

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_HDR
        {
            public Int32 Size;
            public DeviceType DeviceType;
            public Int32 Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_VOLUME
        {
            public DEV_BROADCAST_HDR Header;
            public Int32 Unitmask;
            public BroadcastFlags Flags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public DEV_BROADCAST_HDR Header;
            public Guid dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SCSI_PASS_THROUGH_DIRECT
        {
            public UInt16 Length;
            public byte ScsiStatus;
            public byte PathId;
            public byte TargetId;
            public byte Lun;
            public byte CdbLength;
            public byte SenseInfoLength;
            public byte DataIn;
            public UInt32 DataTransferLength;
            public UInt32 TimeOutValue;
            public IntPtr DataBuffer;
            public UInt32 SenseInfoOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Cdb;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SCSI_PASS_THROUGH_DIRECT_WITH_BUFFERS
        {
            internal SCSI_PASS_THROUGH_DIRECT sptd;
            internal ulong filler; //just to reallign to double word
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            internal byte[] sense;
        }

        #endregion

        #region Operations

        [DllImport("winmm.dll")]
        internal static extern Int32 mciSendString(string command, StringBuilder buffer, Int32 bufferSize, IntPtr hwndCallback);

        [DllImport("Kernel32.dll")]
        public static extern SafeFileHandle CreateFile(string filename,
                                               uint fileaccess,
                                               uint fileshare,
                                               int securityattributes,
                                               [MarshalAs(UnmanagedType.U4)] FileMode creationdisposition,
                                               int flags,
                                               IntPtr template);

        [DllImport("Kernel32.dll")]
        public static extern SafeFileHandle CreateFile(string filename,
                                               [MarshalAs(UnmanagedType.U4)] FileAccess fileaccess,
                                               [MarshalAs(UnmanagedType.U4)] FileShare fileshare,
                                               int securityattributes,
                                               [MarshalAs(UnmanagedType.U4)] FileMode creationdisposition,
                                               int flags,
                                               IntPtr template);

        [DllImport("Kernel32.dll")]
        public static extern IntPtr CloseHandle(SafeFileHandle hObject);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hDevice, uint dwIoControlCode,
            IntPtr lpInBuffer, uint nInBufferSize,
            IntPtr lpOutBuffer, uint nOutBufferSize,
            out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice, uint dwIoControlCode,
            IntPtr lpInBuffer, uint nInBufferSize,
            IntPtr lpOutBuffer, uint nOutBufferSize,
            out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr windowHandle, IntPtr lpBuf, DeviceNotifyHandle handle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterDeviceNotification(IntPtr handle);

        public static IEnumerable<string> MaskToDrives(Int32 mask)
        {
            for (int i = 0; i < 26; i++)
            {
                if ((mask & 1) == 1) 
                    yield return String.Concat((char) (i + 'A'), ':');
                mask >>= 1;
            }
        }

        #endregion
    }
}
// ReSharper restore FieldCanBeMadeReadOnly.Global
// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore UnusedMember.Global
