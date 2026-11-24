namespace Xtml.BrowserApis.Events.Subsets;

public interface IPressuresSubset : ISubset, IViewSubset
{
    new const string TRIM = "pressure,tangentialPressure";

    /// <summary>
    /// The normalized pressure of the pointer input in the range 0 to 1, 
    /// where 0 and 1 represent the minimum and maximum pressure the 
    /// hardware is capable of detecting, respectively.
    /// </summary>
    double Pressure { get; }

    /// <summary>
    /// The normalized tangential pressure of the pointer input 
    /// (also known as barrel pressure or cylinder stress) in the 
    /// range -1 to 1, where 0 is the neutral position of the control.
    /// </summary>
    double TangentialPressure { get; }
}