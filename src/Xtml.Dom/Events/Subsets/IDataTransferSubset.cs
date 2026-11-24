namespace Xtml.BrowserApis.Events.Subsets;

public interface IDataTransferSubset : ISubset, IViewSubset
{
    new const string TRIM = "dataTransfer";

    /// <summary>
    /// Returns a DataTransfer object containing contextual information.
    /// </summary>
    DataTransferContainer DataTransfer { get; }
}