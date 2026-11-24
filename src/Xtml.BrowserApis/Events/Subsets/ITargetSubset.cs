using System.Drawing;
using static Xtml.BrowserApis.Events.Aliases.Subsets;

namespace Xtml.BrowserApis.Events.Subsets;

public interface ITargetSubset : 
    ISubset,
    Target<string>,
    Target<bool>,
    Target<int>,
    Target<long>,
    Target<float>,
    Target<double>,
    Target<decimal>,
    Target<DateTime>,
    Target<DateOnly>,
    Target<TimeOnly>,
    Target<Color>,
    Target<Uri>,
    IViewSubset
{
    new const string TRIM = "target";

    /// <summary>
    /// A reference to the object to which the event was originally dispatched.
    /// </summary>
    new EventTarget Target { get; }
}