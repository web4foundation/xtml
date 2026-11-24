namespace Xtml.BrowserApis.Events.Subsets;

public interface ILengthSubset : ISubset, IViewSubset
{
    new const string TRIM = "length";

    /// <summary>
    /// Returns an integer representing the number of data items stored in the Storage object.
    /// </summary>
    int Length { get; }
}