using System;

namespace com.github.TheCSUser.Shared.Logging
{
    public interface ILogger
    {
        void Info(string text);
        void Warning(string text);
        void Error(string text);
        void Error(string text, Exception e);
    }
}