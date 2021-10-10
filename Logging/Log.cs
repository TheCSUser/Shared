using ColossalFramework.IO;
using com.github.TheCSUser.Shared.Imports;
using com.github.TheCSUser.Shared.Properties;
using System;
using System.IO;

namespace com.github.TheCSUser.Shared.Logging
{
    public sealed class Log : ILogger
    {
        public static readonly ILogger None = new DummyLogger();
        private static readonly Lazy<ILogger> _sharedLog = new Lazy<ILogger>(() =>
        {
            var modsConfigPath = Path.Combine(DataLocation.localApplicationData, "ModConfig");
            if (!Directory.Exists(modsConfigPath)) Directory.CreateDirectory(modsConfigPath);
            var configDir = Path.Combine(modsConfigPath, "TheCSUser_Shared");
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);

            var logDir = Path.Combine(configDir, "Logs");
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

            return new Log(logDir, LibProperties.Name);
        });
        public static readonly ILogger Shared = _sharedLog.Value;

        public ILogger Unity { get; }
        public ILogger File { get; }

        public Log(string path, string name)
        {
            Unity = new UnityDebugLogger(name);
            File = new SyncFileLogger(path);
        }

        public void Info(string text)
        {
#if PREVIEW || DEV
            Unity.Info(text);
#endif
            File.Info(text);
        }
        public void Warning(string text)
        {
#if PREVIEW || DEV
            Unity.Warning(text);
#endif
            File.Warning(text);
        }
        public void Error(string text)
        {
#if PREVIEW || DEV
            Unity.Error(text);
#endif
            File.Error(text);
        }
        public void Error(string text, Exception e)
        {
#if PREVIEW || DEV
            Unity.Error(text, e);
#endif
            File.Error(text, e);
        }

        private sealed class DummyLogger : ILogger
        {
            public void Info(string text) { }
            public void Warning(string text) { }
            public void Error(string text) { }
            public void Error(string text, Exception e) { }
        }
    }
}