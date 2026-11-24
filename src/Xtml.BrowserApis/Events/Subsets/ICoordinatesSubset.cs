using static Xtml.BrowserApis.Events.Aliases.Subsets;

namespace Xtml.BrowserApis.Events.Subsets;

public interface ICoordinatesSubset : ISubset, XY, ClientXY, MovementXY, OffsetXY, PageXY, ScreenXY, IViewSubset
{
    new const string TRIM = $"{XY.TRIM},{ClientXY.TRIM},{MovementXY.TRIM},{OffsetXY.TRIM},{PageXY.TRIM},{ScreenXY.TRIM}";
}