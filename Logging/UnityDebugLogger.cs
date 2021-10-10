using System;
using UnityEngine;

namespace com.github.TheCSUser.Shared.Logging
{
    internal sealed class UnityDebugLogger : ILogger
    {
        public string LoggerName { get; }

        public UnityDebugLogger(string name)
        {
            LoggerName = name ?? string.Empty;
        }

        public void Info(string text)
        {
            try
            {
                Debug.Log($"[{LoggerName}] {text}");
            }
            catch { /*ignore*/ }
        }

        public void Warning(string text)
        {
            try
            {
                Debug.LogWarning($"[{LoggerName}] {text}");
            }
            catch { /*ignore*/ }
        }

        public void Error(string text)
        {
            try
            {
                Debug.LogError($"[{LoggerName}] {text}");
            }
            catch { /*ignore*/ }
        }
        public void Error(string text, Exception e)
        {
            try
            {
                Debug.LogError($"[{LoggerName}] {text}");
                Debug.LogError(e);
            }
            catch { /*ignore*/ }
        }
    }
}