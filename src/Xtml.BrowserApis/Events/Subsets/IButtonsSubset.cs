namespace Xtml.BrowserApis.Events.Subsets;

public interface IButtonsSubset : ISubset, IViewSubset
{
    new const string TRIM = "button,buttons";

    /// <summary>
    /// The button number that was pressed (if applicable) when the mouse event was fired.
    /// </summary>
    Button Button { get; }

    /// <summary>
    /// The buttons being pressed (if any) when the mouse event was fired.
    /// </summary>
    ButtonFlag Buttons { get; }
}