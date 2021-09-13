using com.github.TheCSUser.Shared.Imports;
using System;

namespace com.github.TheCSUser.Shared.Logging
{
    public static class Log
    {
        private static readonly Lazy<UnityDebugLogger> _unityDebugLogger = new Lazy<UnityDebugLogger>(() => new UnityDebugLogger());
        public static ILogger Unity => _unityDebugLogger.Value;

        private static readonly Lazy<SyncFileLogger> _fileLogger = new Lazy<SyncFileLogger>(() => new SyncFileLogger());
        public static ILogger File => _fileLogger.Value;

        public static void Info(string text)
        {
#if PREVIEW || DEV
            Unity.Info(text);
#endif
            File.Info(text);
        }
        public static void Warning(string text)
        {
#if PREVIEW || DEV
            Unity.Warning(text);
#endif
            File.Warning(text);
        }
        public static void Error(string text)
        {
#if PREVIEW || DEV
            Unity.Error(text);
#endif
            File.Error(text);
        }
        public static void Error(string text, Exception e)
        {
#if PREVIEW || DEV
            Unity.Error(text, e);
#endif
            File.Error(text, e);
        }
    }
}