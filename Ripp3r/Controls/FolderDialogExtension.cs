using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Ripp3r.Controls
{
    public static class FolderDialogExtension
    {
        /// <summary>
        /// Using title text to look for the top level dialog window is fragile.
        /// In particular, this will fail in non-English applications.
        /// </summary>
        private const string _topLevelSearchString = "Browse For Folder";

        /// <summary>
        /// These should be more robust.  We find the correct child controls in the dialog
        /// by using the GetDlgItem method, rather than the FindWindow(Ex) method,
        /// because the dialog item IDs should be constant.
        /// </summary>
        private const int _dlgItemBrowseControl = 0;

        private const int _dlgItemTreeView = 100;

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetDlgItem")]
        private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Some of the messages that the Tree View control will respond to
        /// </summary>
        private const int TV_FIRST = 0x1100;

        private const int TVM_GETNEXTITEM = (TV_FIRST + 10);
        private const int TVM_ENSUREVISIBLE = (TV_FIRST + 20);

        /// <summary>
        /// Constants used to identity specific items in the Tree View control
        /// </summary>
        private const int TVGN_CARET = 0x9;

        /// <summary>
        /// Calling this method is identical to calling the ShowDialog method of the provided
        /// FolderBrowserDialog, except that an attempt will be made to scroll the Tree View
        /// to make the currently selected folder visible in the dialog window.
        /// </summary>
        /// <param name="dlg"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static DialogResult ShowFocusedDialog(this FolderBrowserDialog dlg, IWin32Window parent = null)
        {
            using (Timer t = new Timer())
            {
                int retries = 10;
                t.Tick += (s, a) =>
                    {
                        if (retries > 0)
                        {
                            --retries;
                            IntPtr hwndDlg = FindWindow(null, _topLevelSearchString);
                            if (hwndDlg != IntPtr.Zero)
                            {
                                IntPtr hwndFolderCtrl = GetDlgItem(hwndDlg, _dlgItemBrowseControl);
                                if (hwndFolderCtrl != IntPtr.Zero)
                                {
                                    IntPtr hwndTV = GetDlgItem(hwndFolderCtrl, _dlgItemTreeView);

                                    if (hwndTV != IntPtr.Zero)
                                    {
                                        IntPtr item = SendMessage(hwndTV, TVM_GETNEXTITEM, new IntPtr(TVGN_CARET),
                                                                  IntPtr.Zero);
                                        if (item != IntPtr.Zero)
                                        {
                                            SendMessage(hwndTV, TVM_ENSUREVISIBLE, IntPtr.Zero, item);
                                            retries = 0;
                                            ((Timer) s).Stop();
                                        }
                                    }
                                }
                            }
                        }

                        else
                        {
                            //
                            //  We failed to find the Tree View control.
                            //
                            //  As a fall back (and this is an UberUgly hack), we will send
                            //  some fake keystrokes to the application in an attempt to force
                            //  the Tree View to scroll to the selected item.
                            //
                            ((Timer) s).Stop();
                            SendKeys.Send("{TAB}{TAB}{DOWN}{DOWN}{UP}{UP}");
                        }
                    };

                t.Interval = 10;
                t.Start();

                return dlg.ShowDialog(parent);
            }
        }
    }
}