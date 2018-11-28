using System;
using System.Threading.Tasks;

namespace Ripp3r
{
    public static class Interaction
    {
        public static void SetInteraction(IInteraction interaction)
        {
            Instance = interaction;
        }

        public static IInteraction Instance { get; private set; }
    }

    public interface IInteraction
    {
        void TaskBegin(bool inBytes = false);
        void TaskComplete();

        void UpdateFound(Version version, string releaseNotes);
        void Terminate();

        #region Global

        bool Compress { get; }
        long PartSize { get; }
        bool MultiPart { get; }
        int AmountOfCores { get; }

        #endregion

        #region Crypt

        Task<string> GetCryptFilename();
        Task<string> GetCryptOutputFilename(string inputfilename, bool isdecrypting);

        #endregion

        #region Create ISO

        Task<bool> DownloadUpdate(string message);
        Task<string> GetJBDirectory();
        Task<string> GetSaveIsoPath();
        Task<string> GetIrdFile();

        #endregion

        #region Create IRD

        Task<string[]> GetIsoFile();
        Task<string> GetIrdOutputFile(string inputFile);
        Task<string> GetIsoPath(string irdFile);

        #endregion

        #region Upload IRD

        Task<string[]> GetIrdFiles();

        #endregion

        #region GameTDB

        bool GameTDB { get; }
        string GameTDBLanguage { get; }

        #endregion

        void ShowMessageDialog(string message);
        void SetProgressError();
        void ReportProgress(int progress);
        void ReportMessage(string message, ReportType reportType = ReportType.Normal);
        void SetProgressMaximum(int max);
    }

    public enum ReportType
    {
        Normal,
        Success,
        Fail,
        Warning,
        Url
    }
}
