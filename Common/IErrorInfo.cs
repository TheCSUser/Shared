namespace com.github.TheCSUser.Shared.Common
{
    public interface IErrorInfo
    {
        bool IsError { get; set; }
        int ErrorCount { get; }
    }
}