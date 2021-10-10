using System;
using System.Globalization;
using System.IO;

namespace com.github.TheCSUser.Shared.Logging
{
    using static DateTime;
    using static File;
    using static Path;

    internal sealed class SyncFileLogger : ILogger
    {
#if DEV
        private const int LOG_RETENTION_DAYS = 1;
#elif PREVIEW
        private const int LOG_RETENTION_DAYS = 30;
#else
        private const int LOG_RETENTION_DAYS = 7;
#endif
        public string Path { get; }
        public string FileName { get; }

        public SyncFileLogger(string path)
        {
            Path = path;
            FileName = Combine(Path, $"{Now.ToString("yyyy-MM-dd_HHmm", CultureInfo.InvariantCulture)}.log");
            RemoveOldFiles();
        }

        private void RemoveOldFiles()
        {
            if (string.IsNullOrEmpty(Path)) return;
            try
            {
                var daysToKeep = TimeSpan.FromDays(LOG_RETENTION_DAYS);
                foreach (var file in Directory.GetFiles(Path, "*.log", SearchOption.TopDirectoryOnly))
                {
                    var createDate = GetCreationTime(file);
                    if (Now - createDate > daysToKeep) Delete(file);
                }
            }
            catch { /*ignore*/ }
        }

        public void Info(string text)
        {
            if (string.IsNullOrEmpty(Path)) return;
            try
            {
                AppendAllText(FileName, $"[INFO] {text}\n");
            }
            catch { /*ignore*/ }
        }

        public void Warning(string text)
        {
            if (string.IsNullOrEmpty(Path)) return;
            try
            {
                AppendAllText(FileName, $"[WARNING] {text}\n");
            }
            catch { /*ignore*/ }
        }

        public void Error(string text)
        {
            if (string.IsNullOrEmpty(Path)) return;
            try
            {
                AppendAllText(FileName, $"[ERROR] {text}\n");
            }
            catch { /*ignore*/ }
        }
        public void Error(string text, Exception e)
        {
            if (string.IsNullOrEmpty(Path)) return;
            try
            {
                AppendAllText(FileName, $"[ERROR] {text}\n");
                AppendAllText(FileName, $"{e}\n");
            }
            catch { /*ignore*/ }
        }
    }
}