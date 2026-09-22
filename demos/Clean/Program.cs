using System.Drawing;
using Xtml.Templating;
using Xtml.WebSocket;
using Xtml.Dom;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string name = "World";
int c = 16;
double d = 3.14;
List<string> names = ["one", "two", "three", "four", "five", "six", "seven"];
Color color = Color.FromArgb(0xf2f2f7);
bool b = true;

app.MapWindow("/list", () => $"""
    <!doctype html>
    <html>
        <head>
            {$"<title>Clean {name} {c}!</title>"}
        </head>
        <body>
            {$"<h1>Hello {c:c} {name}!</h1>"}

            {GetList()}
        </body>
    </html>
    """);

Html GetList() => $"""
    <list>

        {names.Select(n => $"""
            <lr>
                <ld>{ Icons.Globe }</ld>
                <ld>Hello, { n }!</ld>
            </lr>
        """):zoom-fade}

        <button onpointerdown={ () => c++ }>Clicks: { c }</button>
        <button onpointerdown={ DoThingAsync }>Async</button>
        <button onpointerdown={ EditRandom }>Edit Random</button>
        <button onpointerdown={ Move1 }>Move 1</button>
        <button onpointerdown={ Swap2 }>Swap 2</button>
        <button onpointerdown={ Shuffle }>Shuffle</button>
        <button onpointerdown={ AddOne }>Insert One</button>
        <button onpointerdown={ RemoveOne }>Remove One</button>
        
    </list>
    """;

async Task DoThingAsync(Event.Subsets.XY e)
{
    var window = e.View;
    var console = window.Console;
    console.Log("one");
    console.Log("two");
    await Task.Delay(2000);
    console.Log("three");
    console.Log("four");
    await Task.Delay(2000);
    console.Log("five");
    console.Log("six");

    int i = 1;
    i--;
    i /= i;
    
    window.Alert("Neato");
    console.Log("seven");

    // Console.WriteLine($"one... x:{e.X}, y:{e.Y} thread:{Thread.CurrentThread.ManagedThreadId} {SynchronizationContext.Current?.ToString() ?? "null"}");
    // await Task.Delay(2000);
    // Console.WriteLine($"two... x:{e.X}, y:{e.Y} thread:{Thread.CurrentThread.ManagedThreadId} {SynchronizationContext.Current?.ToString() ?? "null"}");
    // await Task.Delay(2000);
    // Console.WriteLine($"three... x:{e.X}, y:{e.Y} thread:{Thread.CurrentThread.ManagedThreadId} {SynchronizationContext.Current?.ToString() ?? "null"}");
    // await Task.Delay(2000);
    // Console.WriteLine($"four... x:{e.X}, y:{e.Y} thread:{Thread.CurrentThread.ManagedThreadId} {SynchronizationContext.Current?.ToString() ?? "null"}");
}

void EditRandom()
{
    var index = Random.Shared.Next(names.Count);
    names[index] = names[index] + " 😀";
}

void Move1()
{
    int i1 = Random.Shared.Next(names.Count);
    int i2 = Random.Shared.Next(names.Count);
    var item = names[i1];
    names.RemoveAt(i1);
    names.Insert(i2, item);
}

void Swap2()
{
    var i1 = Random.Shared.Next(names.Count);
    var i2 = Random.Shared.Next(names.Count);
    var tmp = names[i1];
    names[i1] = names[i2];
    names[i2] = tmp;
}

void Shuffle()
{
    names = names.OrderBy(n => Random.Shared.Next()).ToList();
}

void AddOne()
{
    // names.Add(names.Count.ToString());
    var i = Random.Shared.Next(names.Count);
    names.Insert(i, names.Count.ToString());
}

void RemoveOne()
{
    // State.names.RemoveAt(State.names.Count - 1);
    var i = Random.Shared.Next(names.Count);
    names.RemoveAt(i);
}

app.MapWindow("/swiftui", () => $"""
    <col>
        { Icons.Globe }
        Hello, World!
    </col>
    """);

var window = app.MapWindow("/app", () => $"""
    <!doctype html>
    <html>
        <head>
            <title>Neato 123!</title>
        </head>
        <body style={$"background-color: {color}"}>
            <button onmousedown={() => c++}>
                Clicks: {c}
            </button>
            {c:c} and {d:c}

            {$"<div>one {name} {c} four</div>"}
            {(Html)$"<div>one {name} {c} four</div>"}
            {MyButton(text: "Nice to repeat you")}
            {MyButton(text: "Nice to repeat you")}

            <br />
            <br />
            {MyButton(text: "111")}{MyButton(text: "222")}{MyButton(text: "333")}{MyButton(text: "444")}
            <br />
            <br />
 
            <row> 
                { names.Select(n => NoCButton(text: n)) }
            </row>

            <section>
                <h2>Attribute stuffs</h2>
                <input type="number" value={c} oninput={e => c = e.Target.Value} /> {c}
                <br/>
                <input type="text" value={name} oninput={e => name = e.Target.Value} /> {name}
                <br/>
                <input type="checkbox" checked={b} oninput={e => b = e.Target.Value} /> {b}
                <br/>
                <input type="checkbox" checked={b} oninput={e => b = e.Target.Value} /> {b}
                <br/>
            </section>
            
            {(b
                ? (Html)$"<row>one {name}<br>{c} four</row>" 
                : (Html)$"<row>{color}</row>"
            ):zoom-fade}

            <row>
                {true switch {
                    _ when c < 5 => MyButton(text: "Neato"),
                    _ when c == 15 => NoCButton(name),
                    _ when c == 17 => $"",
                    _ when c > 50 => YourButton(),
                    _ => NoneButton()
                }}
            </row>


            <br/><br/>

            <span style={$"color: {color.Invert()}; font-size: {c}pt;"}>{color:RGB}</span>
            <input type="color" value={color} oninput={e => color = e.Target.Value} />
            <br/>

            <h2>Click on the buttons</h2>
            <div id="outer-div" onclick={OnDivClicked}>
                outer div<br />
                <div id="middle-div" onclick={OnDivClicked}>
                    middle div<br />
                    <div id="inner-div" onclick={OnDivClicked}>
                        inner div<br />
                        <button id="allowPropagation" onclick={OnAllowPropagation}>allow propagation</button><br />
                        <button id="stopPropagation" onclick={OnStopPropagation}>stop propagation</button><br />
                        <button id="stopImmediatePropagation" onclick={OnStopImmediatePropagation}>immediate stop propagation</button>
                    </div>
                </div>
            </div>
            
            {ComponentWithComponentInside()}

            and one more for that big time key-rewind
            {MyButton(text: "Nice to repeat you")}

        </body>
    </html>
    """);

void OnDivClicked(Event.Subsets.CurrentTarget e)
{
    Console.WriteLine($"Click event process on {e.CurrentTarget.ID}");
}

void OnAllowPropagation(Event.Subsets.Target e)
{
    Console.WriteLine($"Click event processed on '{e?.Target?.ID ?? "null"}' button");
}

void OnStopPropagation(Event e)
{
    e.StopPropagation();
    Console.WriteLine($"Click event processed on '{e?.Target?.ID ?? "null"}' button");
}

void OnStopImmediatePropagation(Event.Subsets.Target e)
{
    e.StopImmediatePropagation();
    Console.WriteLine($"Click event processed on '{e?.Target?.ID ?? "null"}' button");
}

// window.Document.AddEventListener("click", () => Console.WriteLine($"Click event 1 process on document"));
// window.Document.AddEventListener("click", (Event.Subsets.XY e) => { Console.WriteLine($"Click event 2 process on document"); /*e.StopImmediatePropagation();*/ });
// window.Document.AddEventListener("click", () => Console.WriteLine($"Click event 3 process on document"));

// window.AddEventListener("click", () => Console.WriteLine($"Click event 1 process on window"));
// window.AddEventListener("click", () => Console.WriteLine($"Click event 2 process on window"));
// window.AddEventListener("click", () => Console.WriteLine($"Click event 3 process on window"));

Html MyButton(string text, Action<Event>? onClick = null) => $"""
    <button onpointerdown={onClick ?? OnClick}>
        Hello {text}! Clicks: {c}
    </button>
    """;

Html YourButton(string? text = null) => $"""
    <button>
        Clicks: {c} - {text ?? "whatevs"}
    </button>
    """;

Html NoCButton(string text) => $"""
    
    <span>{text}, </span>
    """;

Html NoneButton() => $"""
    <span>None</span>
    """;

Html ComponentWithComponentInside() => $"""
    <div>
        {MyButton(text: "3rd level?")}
    </div>
""";

static void OnClick(Event.Mouse e)
{
    // c++;

    var x = e.X;
}

// window.OnClick = e => Console.WriteLine($"win0: window.OnClick 0: {"e"}");
// window.AddEventListener("afterprint", (Event e) => Console.WriteLine($"win1: afterprint: {"e"}"));
// window.AddEventListener("dblclick", () => Console.WriteLine("win2: dblclick:"));
// window.AddEventListener("click", e => d = e.X);
// window.OnClick = e => Console.WriteLine($"win4: window.OnClick 1: {"e"}");
// window.Document.OnSelectionChange = e => Console.WriteLine($"doc5: {e.Type}");
// window.Document.AddEventListener("click", e => Console.WriteLine($"doc6: document.onclick: {e.X}"));
// window.AddEventListener("mousemove", e => c = (int)e.X);

window.MapGet("/about", window =>
{
    name = "Reset";
    c = 0;
    window.Console.Log($"Navigated to /about.  name:{name}, c:{c}");
});

app.Run();








static class Icons
{
    public static Html Globe => $"""
        <svg style="width: 24px; height: 24px;" viewBox="0 0 63.9375 63.7812">
        <g>
        <rect height="63.7812" opacity="0" width="63.9375" x="0" y="0"/>
        <path fill="#0033ff" d="M31.875 62C40.5625 62 47.6562 49.1562 47.6562 31.9375C47.6562 14.625 40.5938 1.78125 31.875 1.78125C23.1562 1.78125 16.0938 14.625 16.0938 31.9375C16.0938 49.1562 23.1875 62 31.875 62ZM31.875 5.5625C38 5.5625 43.4688 17.75 43.4688 31.9375C43.4688 45.875 38 58.1875 31.875 58.1875C25.75 58.1875 20.2812 45.875 20.2812 31.9375C20.2812 17.75 25.75 5.5625 31.875 5.5625ZM29.8438 2.21875L29.8438 61.4062L33.9375 61.4062L33.9375 2.21875ZM31.875 43.625C22.25 43.625 13.7188 46.1562 9.34375 50.1875L12.5 52.7812C16.625 49.5 23.3438 47.7188 31.875 47.7188C40.4062 47.7188 47.125 49.5 51.25 52.7812L54.4062 50.1875C50.0312 46.1562 41.5 43.625 31.875 43.625ZM60.7188 29.8125L3.03125 29.8125L3.03125 33.9062L60.7188 33.9062ZM31.875 20.2812C41.5 20.2812 50.0312 17.75 54.4062 13.7188L51.25 11.125C47.125 14.375 40.4062 16.1875 31.875 16.1875C23.3438 16.1875 16.625 14.375 12.5 11.125L9.34375 13.7188C13.7188 17.75 22.25 20.2812 31.875 20.2812ZM31.875 63.75C49.4688 63.75 63.75 49.4688 63.75 31.875C63.75 14.2812 49.4688 0 31.875 0C14.2812 0 0 14.2812 0 31.875C0 49.4688 14.2812 63.75 31.875 63.75ZM31.875 59.5C16.625 59.5 4.25 47.125 4.25 31.875C4.25 16.625 16.625 4.25 31.875 4.25C47.125 4.25 59.5 16.625 59.5 31.875C59.5 47.125 47.125 59.5 31.875 59.5Z" fill="white" fill-opacity="0.85"/>
        </g>
        </svg>
        """;

    public static Html Check => $"""
        <svg xmlns="http://www.w3.org/2000/svg" enable-background="new 0 0 24 24" height="24px" viewBox="0 0 24 24" width="24px" fill="#0033ff"><rect fill="none" height="24" width="24"/><path d="M22,5.18L10.59,16.6l-4.24-4.24l1.41-1.41l2.83,2.83l10-10L22,5.18z M19.79,10.22C19.92,10.79,20,11.39,20,12 c0,4.42-3.58,8-8,8s-8-3.58-8-8c0-4.42,3.58-8,8-8c1.58,0,3.04,0.46,4.28,1.25l1.44-1.44C16.1,2.67,14.13,2,12,2C6.48,2,2,6.48,2,12 c0,5.52,4.48,10,10,10s10-4.48,10-10c0-1.19-0.22-2.33-0.6-3.39L19.79,10.22z"/></svg>
        """;
}