using System;
using System.Diagnostics;

namespace Ripp3r.Controls
{
    /// <summary>
    /// Represents an instance of the Windows taskbar
    /// </summary>
    public class TaskbarManager
    {
        // Hide the default constructor
        private TaskbarManager()
        {
        }

        // Best practice recommends defining a private object to lock on
        private static readonly Object syncLock = new Object();

        private static volatile TaskbarManager instance;

        /// <summary>
        /// Represents an instance of the Windows Taskbar
        /// </summary>
        public static TaskbarManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                            instance = new TaskbarManager();
                    }
                }

                return instance;
            }
        }

        // Internal implemenation of ITaskbarList4 interface
        private volatile ITaskbarList4 taskbarList;

        private ITaskbarList4 TaskbarList
        {
            get
            {
                // Check if running on Win7
                if (!IsWin7()) return null;

                if (taskbarList == null)
                {
                    // Create a new instance of ITaskbarList3
                    lock (syncLock)
                    {
                        if (taskbarList == null)
                        {
                            ITaskbarList4 t = (ITaskbarList4) new CTaskbarList();
                            t.HrInit();

                            taskbarList = t;
                        }
                    }
                }

                return taskbarList;
            }
        }



        /// <summary>
        /// Displays or updates a progress bar hosted in a taskbar button of the main application window 
        /// to show the specific percentage completed of the full operation.
        /// </summary>
        /// <param name="currentValue">An application-defined value that indicates the proportion of the operation that has been completed at the time the method is called.</param>
        /// <param name="maximumValue">An application-defined value that specifies the value currentValue will have when the operation is complete.</param>
        public void SetProgressValue(int currentValue, int maximumValue)
        {
            if (!IsWin7()) return;

            TaskbarList.SetProgressValue(OwnerHandle, Convert.ToUInt32(currentValue), Convert.ToUInt32(maximumValue));
        }

        private static bool IsWin7()
        {
            return (Environment.OSVersion.Version.Major > 6) ||
                   (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1);
        }


        /// <summary>
        /// Sets the type and state of the progress indicator displayed on a taskbar button of the main application window.
        /// </summary>
        /// <param name="state">Progress state of the progress button</param>
        public void SetProgressState(TaskbarProgressBarState state)
        {
            if (!IsWin7()) return;
            TaskbarList.SetProgressState(OwnerHandle, (TBPFLAG) state);
        }


        private IntPtr ownerHandle;

        /// <summary>
        /// Sets the handle of the window whose taskbar button will be used
        /// to display progress.
        /// </summary>
        private IntPtr OwnerHandle
        {
            get
            {
                // Check if running on Win7
                if (!IsWin7()) return IntPtr.Zero;

                if (ownerHandle == IntPtr.Zero)
                {
                    Process currentProcess = Process.GetCurrentProcess();

                    if (currentProcess.MainWindowHandle != IntPtr.Zero)
                        ownerHandle = currentProcess.MainWindowHandle;
                    else
                        throw new InvalidOperationException("A valid active Window is needed to update the Taskbar");
                }

                return ownerHandle;
            }
        }
    }

    /// <summary>
    /// Represents the thumbnail progress bar state.
    /// </summary>
    public enum TaskbarProgressBarState
    {
        /// <summary>
        /// No progress is displayed.
        /// </summary>
        NoProgress = 0,

        /// <summary>
        /// The progress is indeterminate (marquee).
        /// </summary>
        Indeterminate = 0x1,

        /// <summary>
        /// Normal progress is displayed.
        /// </summary>
        Normal = 0x2,

        /// <summary>
        /// An error occurred (red).
        /// </summary>
        Error = 0x4,

        /// <summary>
        /// The operation is paused (yellow).
        /// </summary>
        Paused = 0x8
    }
}