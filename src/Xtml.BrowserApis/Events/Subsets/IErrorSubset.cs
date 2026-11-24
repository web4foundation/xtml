namespace Xtml.BrowserApis.Events.Subsets;

public interface IErrorSubset : ISubset, IViewSubset
{
    new const string TRIM = "error";

    string Message { get; }
    string FileName { get; }
    int LineNo { get; }
    int ColNo { get; }
    DOMException Error { get; }
}