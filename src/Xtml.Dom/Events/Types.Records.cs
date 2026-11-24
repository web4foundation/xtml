using System.Drawing;

namespace Xtml.BrowserApis.Events;

/// <summary>
/// The acceleration property is an object providing information about acceleration on three axis. Each axis is represented with its own property:
/// </summary>
/// <param name="X">Represents the acceleration upon the x axis which is the west to east axis</param>
/// <param name="Y">Represents the acceleration upon the y axis which is the south to north axis</param>
/// <param name="Z">Represents the acceleration upon the z axis which is the down to up axis</param>
public record struct XYZ(double X = 0, double Y = 0, double Z = 0)
{
    public static readonly XYZ Empty = new();
}

/// <summary>
/// The rotationRate property is a read only object describing the rotation rates of the device around each of its axes:
/// </summary>
/// <param name="Alpha">The rate at which the device is rotating about its Z axis; that is, being twisted about a line perpendicular to the screen.</param>
/// <param name="Beta">The rate at which the device is rotating about its X axis; that is, front to back.</param>
/// <param name="Gamma">The rate at which the device is rotating about its Y axis; that is, side to side.</param>
public record struct ABG(double Alpha = 0, double Beta = 0, double Gamma = 0)
{
    public static readonly ABG Empty = new();
}

/// <summary>
/// The DataTransfer object is used to hold any data transferred between contexts, such as a drag and drop operation, or clipboard read/write. It may hold one or more data items, each of one or more data types.  DataTransfer was primarily designed for the HTML Drag and Drop API, as the DragEvent.dataTransfer property, and is still specified in the HTML drag-and-drop section, but it is now also used by other APIs, such as ClipboardEvent.clipboardData and InputEvent.dataTransfer. However, other APIs only use certain parts of its interface, ignoring properties such as dropEffect. Documentation of DataTransfer will primarily discuss its usage in drag-and-drop operations, and you should refer to the other APIs' documentation for usage of DataTransfer in those contexts.
/// </summary>
/// <param name="DropEffect">Gets the type of drag-and-drop operation currently selected or sets the operation to a new type. The value must be none, copy, link or move.</param>
/// <param name="EffectAllowed">Provides all of the types of operations that are possible. Must be one of none, copy, copyLink, copyMove, link, linkMove, move, all or uninitialized.</param>
/// <param name="Files">Contains a list of all the local files available on the data transfer. If the drag operation doesn't involve dragging files, this property is an empty list.</param>
/// <param name="Items">Gives a DataTransferItemList object which is a list of all of the drag data.</param>
/// <param name="Types">An array of strings giving the formats that were set in the dragstart event.</param>
public record struct DataTransferContainer(
    string DropEffect,
    string EffectAllowed,
    string[] Files,
    DataTransferItem[] Items,
    string[] Types
) {
    public DataTransferContainer() : this("", "", [], [], []) {}

    public static readonly DataTransferContainer Empty = new();
}

/// <summary>
/// The DataTransferItem object represents one drag data item. During a drag operation, each DragEvent has a dataTransfer property which contains a list of drag data items. Each item in the list is a DataTransferItem object.
/// </summary>
/// <param name="Kind">The kind of drag data item, string or file.</param>
/// <param name="Type">The drag data item's type, typically a MIME type.</param>
public record struct DataTransferItem(string Kind = "", string Type = "")
{
    public static readonly DataTransferItem Empty = new();
}

public record struct DOMException(string Name = "", string Message = "")
{
    public static readonly DOMException Empty = new();
}

public interface EventTarget<T>
{
    public string ID { get; }
    public string Name { get; }
    public string Type { get; }
    public bool? Checked { get; }
    public T Value { get; }
}

public record class EventTarget(
    string ID = "",
    string Name = "",
    string Type = "",
    bool? Checked = false,
    string Value = "") :
        EventTarget<string>,
        EventTarget<bool>,
        EventTarget<int>,
        EventTarget<long>,
        EventTarget<float>,
        EventTarget<double>,
        EventTarget<decimal>,
        EventTarget<DateTime>,
        EventTarget<DateOnly>,
        EventTarget<TimeOnly>,
        EventTarget<Color>,
        EventTarget<Uri>
{
    public static readonly EventTarget Empty = new();

    public bool? ValueAsBool => Checked;
    public int? ValueAsInt => int.TryParse(Value, out var i) ? i : null;
    public long? ValueAsLong => long.TryParse(Value, out var i) ? i : null;
    public float? ValueAsFloat => float.TryParse(Value, out var f) ? f : null;
    public double? ValueAsDouble => double.TryParse(Value, out var d) ? d : null;
    public decimal? ValueAsDecimal => decimal.TryParse(Value, out var m) ? m : null;
    public DateTime? ValueAsDateTime => DateTime.TryParse(Value, out var d) ? d : null;
    public DateOnly? ValueAsDateOnly => DateOnly.TryParse(Value, out var d) ? d : null;
    public TimeOnly? ValueAsTimeOnly => TimeOnly.TryParse(Value, out var t) ? t : null;
    public Color? ValueAsColor => Value != null ? ColorTranslator.FromHtml(Value) : null; // TODO: Should "TryParse" not throw.
    public Uri? ValueAsUri => Uri.TryCreate(Value, UriKind.RelativeOrAbsolute, out var u) ? u : null;

    string EventTarget<string>.Value => Value ?? string.Empty;
    bool EventTarget<bool>.Value => ValueAsBool ?? default;
    int EventTarget<int>.Value => ValueAsInt ?? default;
    long EventTarget<long>.Value => ValueAsLong ?? default;
    float EventTarget<float>.Value => ValueAsFloat ?? default;
    double EventTarget<double>.Value => ValueAsDouble ?? default;
    decimal EventTarget<decimal>.Value => ValueAsDecimal ?? default;
    DateTime EventTarget<DateTime>.Value => ValueAsDateTime ?? default;
    DateOnly EventTarget<DateOnly>.Value => ValueAsDateOnly ?? default;
    TimeOnly EventTarget<TimeOnly>.Value => ValueAsTimeOnly ?? default;
    Color EventTarget<Color>.Value => ValueAsColor ?? default;
    Uri EventTarget<Uri>.Value => ValueAsUri ?? new Uri("about:blank");
}

/// <summary>
/// The Touch interface represents a single contact point on a touch-sensitive device. The contact point is commonly a finger or stylus and the device may be a touchscreen or trackpad.
/// </summary>
/// <param name="Identifier">Returns a unique identifier for this Touch object. A given touch point (say, by a finger) will have the same identifier for the duration of its movement around the surface. This lets you ensure that you're tracking the same touch all the time.</param>
/// <param name="ScreenX">Returns the X coordinate of the touch point relative to the left edge of the screen.</param>
/// <param name="ScreenY">Returns the Y coordinate of the touch point relative to the top edge of the screen.</param>
/// <param name="ClientX">Returns the X coordinate of the touch point relative to the left edge of the browser viewport, not including any scroll offset.</param>
/// <param name="ClientY">Returns the Y coordinate of the touch point relative to the top edge of the browser viewport, not including any scroll offset.</param>
/// <param name="PageX">Returns the X coordinate of the touch point relative to the left edge of the document. Unlike clientX, this value includes the horizontal scroll offset, if any.</param>
/// <param name="PageY">Returns the Y coordinate of the touch point relative to the top of the document. Unlike clientY, this value includes the vertical scroll offset, if any.</param>
/// <param name="RadiusX">Returns the X radius of the ellipse that most closely circumscribes the area of contact with the screen. The value is in pixels of the same scale as screenX.</param>
/// <param name="RadiusY">Returns the Y radius of the ellipse that most closely circumscribes the area of contact with the screen. The value is in pixels of the same scale as screenY.</param>
/// <param name="RotationAngle">Returns the angle (in degrees) that the ellipse described by radiusX and radiusY must be rotated, clockwise, to most accurately cover the area of contact between the user and the surface.</param>
/// <param name="Force">Returns the amount of pressure being applied to the surface by the user, as a float between 0.0 (no pressure) and 1.0 (maximum pressure).</param>
public record struct TouchPoint(
    long Identifier = 0,
    double ScreenX = 0,
    double ScreenY = 0,
    double ClientX = 0,
    double ClientY = 0,
    double PageX = 0,
    double PageY = 0,
    double RadiusX = 0,
    double RadiusY = 0,
    double RotationAngle = 0,
    double Force = 0
) {
    public static readonly TouchPoint Empty = new();
}