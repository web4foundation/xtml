global using Xtml.Templating;
global using Xtml;
global using Xtml.WebSocket;
global using Xtml.Dom;
global using Xtml;
global using System.Buffers;
using System.Drawing;
using System.Text;
Action<string> Log = Console.WriteLine;

var builder = WebApplication.CreateBuilder(args);

#if !DEBUG
builder.Services.AddLogging(c => c.ClearProviders());
#endif

var app = builder.Build();






var c = 4;//.AsState();
// var d = 3.14;//.AsState();
var name = "Rylan";//.AsState();
// var product = State.FromShared<Product>($"products/{c}");

var bg="grey";
var fg="red";
var isSelected=true;
var max=3.14;

app.MapGet("/hi1", () => "Hello world");
app.MapGet("/hi2", () => $"Hello world");
// app.MapGet("/hi3", ctx => $"Hello world");

app.MapGet("/hi", () => $"""
    <html>
        <body>
            <button onclick="">
                Clicks: {c}
            </button>
        </body>
    </html>
    """);

async Task DoItAsync(string source)
{
    Console.WriteLine($"{source}:  start: {Thread.CurrentThread.Name} {Thread.CurrentThread.ManagedThreadId}");
    await Task.Delay(1000);
    Console.WriteLine($"{source}:  end:   {Thread.CurrentThread.Name} {Thread.CurrentThread.ManagedThreadId}");
}

async void DoIt(string source)
{
    Console.Write($"{source}:  start: {Thread.CurrentThread.Name} {Thread.CurrentThread.ManagedThreadId}");
    for (int i = 0; i < 10; i++)
    {
        Thread.Sleep(100);
        Console.Write(".");
    }
    Console.WriteLine();
    await Task.Delay(1);
    Console.WriteLine($"{source}:  end:   {Thread.CurrentThread.Name} {Thread.CurrentThread.ManagedThreadId}");
}

Action a = new(SomeInstanceMethod);
void SomeInstanceMethod()
{
    c++;
    Console.WriteLine("I am a method");
}

var window = app.MapUI("/x02", () => $"""
    <!doctype html>
    <html>
        <body>
            <button onclick={a ?? SomeInstanceMethod}>
                Clicks: {c}
            </button>
        </body>
    </html>
    """);

window.MapGet("/about", ctx => 
{
    c = 0;
    name = "about";
});

window.AddEventListener("click", e => 
{
    Console.WriteLine(e.AltKey);
});



app.MapUI("/x01", () => $"""
    <!doctype html>
    <html>
        <body>
            <h1>Hello world {c}</h1>
            <h1>Hello world {c:c}{c}</h1>

            <h1>
                <button style="font-size: 36pt;" onclick={ async (Event e) => await DoItAsync("btn1") }>
                    async
                </button>
                <button style="font-size: 36pt;" onclick={() => DoIt("btn2") } onmousemove={e => { Console.WriteLine($"x:{e.X} y:{e.Y}"); }}>
                    sync
                </button>
                <button style="font-size: 36pt;" onclick={DoMouseStuff:x,y}>
                    Mouse 1
                </button>
                <button style="font-size: 36pt;" onclick={() => OnClickNoEvent()}>
                    Mouse 2
                </button>
                <button style="font-size: 36pt;" onclick={(Event e) => Console.WriteLine(e):xy}>
                    Mouse 3
                </button>
            </h1>

            { JustAMethod(name: name) }

            <button onclick={() => Console.WriteLine("hi") }>
                onclick => Console.WriteLine("hi")
            </button>

            { JustAMethod(name: name) }

            <button onclick={ () => c++ }>
                onclick => c++
            </button>

            <button onclick={ () => c-- }>
                onclick => c--
            </button>

            <button onclick={ () => isSelected = !isSelected }>
                onclick => isSelected = !isSelected
            </button>

            <button onclick={ (Event e) => { isSelected = !isSelected; Console.WriteLine(e.DefaultPrevented); } } ondblclick={ (Event e) => Console.WriteLine(e.Type) }>
                () => isSelected = !isSelected
            </button>

            <button onclick={ () => c++ }>
                () => c++
            </button>

            <button onclick={ () => c++ }>
                () => c++
            </button>

            <button onclick={ OnClickNoEvent }>
                OnClickNoEvent: { c }
            </button>

            <button onclick={ OnClick }>
                OnClick: { c }
            </button>

            <button onclick={ OnClickNoEventAsync }>
                OnClickNoEventAsync: { c }
            </button>

            <button onclick={ OnClickAsync }>
                OnClickAsync: { c }
            </button>

            <button style={$"background-color: {bg}; color: {fg};"} size={max}>
                Clicks: {c}
            </button>

            <input type="checkbox" checked={isSelected} />

            <input type="number" value={c} onchange={e => c = e.Target.Value} />
            <input type="text" value={name} onchange={e => name = e.Target?.Value ?? ""} onkeyup={e => name = e.Target?.Value ?? ""} />

        </body>
    </html>
    """
);
// .MapGet("/", state => 
// {
//     name = "/Rylan/";
// })
// .MapGet("/actives", state => 
// {
//     c = 14;
// })
// .MapGet("/deleted", state => 
// {
//     c++;
// });

Html JustAMethod(string name) => $"""
    <div>
        <p>I am just a method, {name}</p>
        { TempFooter() }
    </div>
    """;

Html TempFooter() => $"""
    <div>And a happy {c} {c} New Year: {DateTime.Now:O}</div>
    """;

void DoMouseStuff(Event e)
{
    // double x = e.x;
    // double y = e.y;
    // Console.WriteLine($"DoMouseStuff: x:{x} y:{y} eventPhase:{e.eventPhase}");
    Console.WriteLine(e);
}

void OnClickNoEvent()
{
    c++;
    isSelected = !isSelected;
    Console.WriteLine("I'm a handler (no event)!!!");
}

void OnClick(Event e)
{
    c++;
    isSelected = !isSelected;
    Console.WriteLine($"I'm a handler with an event!!! x:{e.X} y:{e.Y}");
}

async Task OnClickNoEventAsync()
{
    c++;
    isSelected = !isSelected;
    Console.WriteLine("I'm a handler (no event)...");
    await Task.Delay(500);
    Console.WriteLine("...now I'm done");
}

async Task OnClickAsync(Event e)
{
    c++;
    isSelected = !isSelected;
    Console.WriteLine("I'm a handler...");
    await Task.Delay(500);
    Console.WriteLine("...now I'm done");
}







string s = "Rylan";
bool b = true;
int i = 3;
long l = 5_000_000_000;
float f = 3.1f;
double d = 3.14;
decimal m = 3.1415m;
DateTime dt = DateTime.Now;
DateOnly dO = DateOnly.FromDateTime(DateTime.Now);
TimeOnly tO = TimeOnly.FromDateTime(DateTime.Now);
Color color = Color.Red;
Uri u = new("http://Itcanbeanything");
app.MapUI("/signatures", () => $"""
    <!doctype html>
    <html>
        <body>
            <h2>Mutable Values</h2>
            <p>string: {s}</p>
            <p>bool: {b}</p>
            <p>int: {i} {i:c}</p>
            <p>double: {d} {d:#000.000}</p>
            <p>DateTime: {dt} {dt:hh:mm:ss}</p>
            <p>html: {TempFooter()}

            <h2>Mutable Attributes</h2>
            <p><input name={s} value="attribute string" /></p>
            <p><input checked={b} type="checkbox" />attribute bool</p>
            <p><input max={i} type="number" />attribute int</p>
            <p><input value={0.5} min="0" max="1" step="0.5" type="range" />attribute double</p>
            <p><input style={$"background-color: {bg}; color: {fg};"} type="submit" value="attribute html" /></p>

            <h2>Event Handlers</h2>

            <h3>Event</h3>
            <p>
                <button onclick={WithoutEvent}>WithoutEvent</button>
                <button onclick={WithoutEventAsync}>WithoutEventAsync</button>
                <button onclick={WithEvent}>WithEvent</button>
                <button onclick={WithEventAsync}>WithEventAsync</button>
                <button onclick={() => WithoutEvent()}>() => WithoutEvent()</button>
                <button onclick={() => WithoutEventAsync()}>() => WithoutEventAsync()</button>
                <button onclick={e => WithEvent(e)}>e => WithEvent(e)</button>
                <button onclick={e => WithEventAsync(e)}>e => WithEventAsync(e)</button>
                <button onclick={e => WithEvent(e):x,y}>e => WithEvent(e):x,y</button>
                <button onclick={e => WithEventAsync(e):x,y}>e => WithEventAsync(e):x,y</button>
                <button onclick={() => Console.WriteLine("hi")}>Anonymous, returns void</button>
                <button onclick={() => i++}>Anonymous, has return value</button>
                <button {onclick => i++}>Anonymous, arg-as-name, has return value</button>
            </p>

            <h3>Event.Mouse</h3>
            <p>
                <button onclick={WithMouseEvent}>WithMouseEvent</button>
                <button onclick={WithMouseEventAsync}>WithMouseEventAsync</button>
                <button onclick={e => WithMouseEvent(e)}>e => WithMouseEvent(e)</button>
                <button onclick={e => WithMouseEventAsync(e)}>e => WithMouseEventAsync(e)</button>
                <button onclick={e => WithMouseEvent(e):x,y}>e => WithMouseEvent(e):x,y</button>
                <button onclick={e => WithMouseEventAsync(e):x,y}>e => WithMouseEventAsync(e):x,y</button>
                <button onclick={e => Console.WriteLine($"{e.X},{e.Y}")}>e => Console.WriteLine($"e.X,e.Y")</button>
            </p>

            <h3>Event.Touch</h3>
            <p>
                <button onclick={WithTouchEvent}>WithMouseEvent</button>
                <button onclick={WithTouchEventAsync}>WithTouchEventAsync</button>
                <button onclick={e => WithTouchEvent(e)}>e => WithTouchEvent(e)</button>
                <button onclick={e => WithTouchEventAsync(e)}>e => WithTouchEventAsync(e)</button>
                <button onclick={e => WithTouchEvent(e):x,y}>e => WithTouchEvent(e):x,y</button>
                <button onclick={e => WithTouchEventAsync(e):x,y}>e => WithTouchEventAsync(e):x,y</button>
            </p>

            <h3>Input Values</h3>
            <p>
                <input type="text" value={s} oninput={e => s = e.Target?.Value ?? "error"} /> {s} <br/>
                <input type="text" value={s} oninput={e => s = e.Target.Value} /> {s} <br/>
                <input type="checkbox" checked={b} oninput={e => b = e.Target.Value} /> {b}
                <input type="checkbox" checked={b} oninput={e => b = e.Target.Value} /> {b} <br/>
                <input type="number" value={i} oninput={e => i = e.Target.Value} step="1" /> {i} <br/>
                <input type="number" value={l} oninput={e => l = e.Target.Value} step="1000000" /> {l} <br/>
                <input type="number" value={f} oninput={e => f = e.Target.Value} step="0.1" /> {f} <br/>
                <input type="number" value={d} oninput={e => d = e.Target.Value} step="0.1" /> {d} <br/>
                <input type="number" value={m} oninput={e => m = e.Target.Value} step="0.1" /> {m} <br/>
                <input type="datetime-local" value={dt} oninput={e => dt = e.Target.Value} /> {dt} <br/>
                <input type="date" value={dO} oninput={e => dO = e.Target.Value} /> {dO} <br/>
                <input type="time" value={tO} oninput={e => tO = e.Target.Value} /> {tO} <br/>
                <input type="color" value={color} oninput={e => color = e.Target.Value} /> {color} <br/>
                <input type="url" value={u} oninput={e => u = e.Target.Value} /> {u} <br/>
                <input type="file" value={u} oninput={e => u = e.Target.Value} /> {u} <br/>
            </p>

            <button id="itID" name="itName" onclick={DoIt2}>DoIt2</button>
            <input type="number" oninput={e => Console.WriteLine(e.DataTransfer.DropEffect)}>lambda</input>
            <input type="number" { value => c } onchange={ e => Console.WriteLine($"{e.RelatedTarget.ValueAsInt}") } />
            <button onclick={e => d = e.X}>lambda</button>
            <input type="number" { value => i } oninput={DoIt3} step="1" /> {i} <br/>
            <input type="text" oninput={DoIt4}>DoIt2</input>
            <button onclick={DoIt4}>DoIt4</button>
        </body>
    </html>
    """);

void WithoutEvent()                             { Console.WriteLine($"WithoutEvent()"); }
void WithEvent(Event e)                         { Console.WriteLine($"WithEvent(Event e): {e}"); }
void WithMouseEvent(Event.Mouse e)              { Console.WriteLine($"WithMouseEvent(Event.Mouse e): {e}"); }
void WithTouchEvent(Event.Touch e)              { Console.WriteLine($"WithTouchEvent(Event.Touch e): {e}"); }
async Task WithoutEventAsync()                  { Console.WriteLine($"WithoutEventAsync()"); await Task.Delay(1); }
async Task WithEventAsync(Event e)              { Console.WriteLine($"WithEventAsync(Event e): {e}"); await Task.Delay(1); }
async Task WithMouseEventAsync(Event.Mouse e)   { Console.WriteLine($"WithMouseEventAsync(Event.Mouse e): {e}"); await Task.Delay(1); }
async Task WithTouchEventAsync(Event.Touch e)   { Console.WriteLine($"WithTouchEventAsync(Event.Touch e): {e}"); await Task.Delay(1); }

void DoIt2(Event.Input<double> e)
{
    var a = e.Target.Value;
    Console.WriteLine($"a:{a}");
    Console.WriteLine($"d:{d}");
    Console.WriteLine($"m:{m}");
    Console.WriteLine($"{e.Target.ID} {e.Target.Name} {e.EventPhase} {e}\n\n{e}");
}

async Task DoIt3(Event.Subsets.Target<Color> e)
{
    await Task.Delay(1);
    Console.WriteLine($"{e.Target.Value}");
    color = e.Target.Value;
}

void DoIt4(Event.Subsets.ModifierShift e)
{
    Console.WriteLine($"{e.ShiftKey}");
}






app.MapGet("/pip", async context =>
{
    var pipeWriter = context.Response.BodyWriter;

    pipeWriter.Write($"""
        <!doctype html>
        <html>
            <body>
                Hello world {c}
                { JustAMethod(name: name) }
                <button onclick={e => c++}>
                    Clicks: {c}
                </button>
            </body>
        </html>
        """);

    await pipeWriter.FlushAsync();
});










app.MapGet("/ex1", async context =>
{
    context.Response.ContentLength = 11;

    var pipeWriter = context.Response.BodyWriter;

    pipeWriter.Write($"""
        Hello world
        """);

    await pipeWriter.FlushAsync();
});

var ui = new UI();
var vm = new ViewModel();
app.MapGet("/main", async context =>
{
    var pipeWriter = context.Response.BodyWriter;

    pipeWriter.Write($"{ui.Wat(vm)}");

    await pipeWriter.FlushAsync();
});





const int contentLength = 11;
const int writeCount = 1;

string strN = new('a', contentLength / writeCount);
ReadOnlyMemory<byte> memN = new(System.Text.Encoding.UTF8.GetBytes(strN));
ReadOnlySpan<byte> spnN = memN.Span;
byte[] bufN = System.Text.Encoding.UTF8.GetBytes(strN);


// 5.8M
app.MapGet("/min", () => "Hello world");

// 7.0M
app.MapGet("/str", context =>
{
    context.Response.ContentLength = contentLength;
    for (int i = 0; i < writeCount; i++)
    {
        context.Response.WriteAsync(strN);
    }
    return Task.CompletedTask;
});

// 7.5M
app.MapGet("/asy", async context =>
{
    context.Response.ContentLength = contentLength;
    for (int i = 0; i < writeCount; i++)
    {
        await context.Response.WriteAsync(strN);
    }
});

// 4.5M
app.MapGet("/syn", context =>
{
    context.Response.ContentLength = contentLength;

    var pipeWriter = context.Response.BodyWriter;
    for (int i = 0; i < writeCount; i++)
    {
        var w = pipeWriter.GetMemory(strN.Length);
        int b = System.Text.Encoding.UTF8.GetBytes(strN, w.Span);
        pipeWriter.Advance(b);
    }
    return Task.CompletedTask;
});

// 7.8M
app.MapGet("/mem", async context =>
{
    context.Response.ContentLength = contentLength;

    var pipeWriter = context.Response.BodyWriter;
    for (int i = 0; i < writeCount; i++)
    {
        await pipeWriter.WriteAsync(memN);
    }
});

// 4.2M
app.MapGet("/ff0", () => $"Hello world");

// 7.4M
app.MapGet("/ff1", () => $"Hello world");

// 7.8M
byte[] hw = Encoding.UTF8.GetBytes("Hello world");
ReadOnlyMemory<byte> hw2 = new(hw);
app.MapGet("/co0", async context =>
{
    context.Response.ContentLength = 11;

    var pipeWriter = context.Response.BodyWriter;
    await pipeWriter.WriteAsync(hw2);
});

// 7.8M
app.MapGet("/co1", async context =>
{
    context.Response.ContentLength = 11;
    var pipeWriter = context.Response.BodyWriter;
    await pipeWriter.WriteAsync(hw);
});

// 4.7M
app.MapGet("/spn", async context =>
{
    context.Response.ContentLength = contentLength;

    var pipeWriter = context.Response.BodyWriter;
    for (int i = 0; i < writeCount; i++)
    {
        pipeWriter.Write(memN.Span);
    }
    await pipeWriter.FlushAsync();
    // return Task.CompletedTask;
});

// 4.7M
app.MapGet("/sp2", context =>
{
    context.Response.ContentLength = contentLength;

    var pipeWriter = context.Response.BodyWriter;
    for (int i = 0; i < writeCount; i++)
    {
        pipeWriter.Write(bufN.AsSpan());
    }
    return Task.CompletedTask;
});

app.MapGet("/ros", context =>
{
    ReadOnlySpan<byte> rosN = new byte[] {
        (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4',
        (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9',
    };

    context.Response.ContentLength = contentLength;

    var pipeWriter = context.Response.BodyWriter;
    for (int i = 0; i < writeCount; i++)
    {
        context.Response.Body.Write(rosN);
    }
    return Task.CompletedTask;
});

// 7.7M
app.MapGet("/wat", async context =>
{
    context.Response.ContentLength = contentLength;

    var pipeWriter = context.Response.BodyWriter;
    for (int i = 0; i < writeCount; i++)
    {
        if (i < 10)
        {
            await pipeWriter.WriteAsync(memN);
        }
        else
        {
            var memory = pipeWriter.GetMemory(memN.Length);
            memN.CopyTo(memory);
            pipeWriter.Advance(memN.Length);
        }
    }
    // return Task.CompletedTask;
});



// new Thread(async () =>
// {
//     while (true)
//     {
//         long gc1 = GC.GetTotalAllocatedBytes();
//         await Task.Delay(1000);
//         long gc2 = GC.GetTotalAllocatedBytes();

//         Console.WriteLine("{0:n0} bytes allocated", gc2 - gc1);
//     }
// }).Start();

app.Run();//"http://+:5001");





// readonly partial struct MyComponent(string productId, State<int> Third) : IView
// {
//     [State]
//     partial int Count { get; set; }

//     [State(shared: $"ram://products/{nameof(productId)}")]
//     partial Product Product { get; set; }

//     private readonly State<int> _third = Third;
//     public int Third
//     {
//         get => _third.Value;
//         set => _third.Value = value;
//     }

//     void OnClick(Event e)
//     {
//         Third = 5;

//         Count++;
//         Product = Product with { Title = $"wat {Count}?" };
//     }

//     public Html Render() => $"""
//         <button>
//             { Product.Title }
//         </button>
//     """;
// }



// readonly partial struct MyComponent
// {
//     private readonly State<Product> productState = State<Product>.Shared(productId);
//     partial Product Product
//     {
//         get
//         {
//             productState.Observe(renderContext);
//             return productState.Value;
//         }
//         set
//         {
//             Invalidating();
//             productState.Value = value;
//             Invalidate();
//         }
//     }

//     private HttpContent renderContext;
//     internal Html Render(HttpContext httpContext)
//     {
//         renderContext = httpContext;
//         return Render();
//         renderContext = null;
//     }

//     private readonly State<int> countState = new State<int>(default);
//     partial int Count
//     {
//         get => countState.Value;
//         set => countState.Value = value;
//     }
// }


record Product(string Title);
// struct Product
// {
//     public string Title { get; set; }
// }





// var try1 = new Try1
// {
//     One = 1,
//     Two = "two",
//     IsShowing = true, // Here's the problem.  I definitely need to pass in a state object!
//     DT = DateTime.Now,
// };

// var try2 = new Try1();

// var try3 = new Try1(
//     one: 1,
//     two: null,
//     isShowing: true
// );

// var show = new State<bool>(true);
// var try4 = new Try1(
//     one: 1,
//     two: null,
//     isShowing: show
// );

// readonly struct Try1(
//     int one,
//     string? two,
//     State<bool> isShowing,
//     double three = 4.6,
//     DateTime? dt = default)
// {
//     string Render => $"""
//         <p onclick={() => Console.WriteLine(One) }> // Another shortcoming.
//             Hello {Two}
//             {new Try1(one: 1, two: "two", isShowing: IsShowing)} // With casting, this would be VERY easy to mess up.
//         </p>
//     """;

//     void OnClick()
//     {
//         Console.WriteLine(One);
//         Console.WriteLine(one); // "one" does not show up as a part of "this."
//         // one = 5; // error
//         // One = 4; // error

//         IsShowing = true;
//         isShowing <<= true;

//         Console.WriteLine(isShowing);

//         const State<bool> a = true;
//         a = false; // Careful!  That didn't update the value, it replaced the struct!
//         a <<= false;
//     }

//     public int One { get; init; } = one;
//     public string? Two { get; } = two;
//     public double Three { get; init; } = three;
//     public bool IsShowing { get => isShowing.Value; set => isShowing.Value = value; }
//     public DateTime? DT { get; init; } = dt;
// }

// public class State<T>(T value)
// {
//     public T Value { get; set; } = value;

//     public static implicit operator State<T>(T value) => new State<T>(value);

//     public static State<T> operator << (State<T> a, T b)
//     {
//         a.Value = b;
//         return a;
//     }

//     public static State<T> operator >> (State<T> a, T b)
//     {
//         throw new NotImplementedException();
//     }

// }
