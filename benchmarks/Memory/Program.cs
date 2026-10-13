using System.Buffers;
using System.Drawing;
using System.IO.Pipelines;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Xtml.Templating;
using Xtml.Templating.Composers;
using Xtml.Composers;
using Xtml.Runtime;
using Xtml.Runtime.Composers;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

[ShortRunJob]
[MemoryDiagnoser]
[OperationsPerSecond]
public class Tests
{
    static readonly Pipe pipe = new();
    static readonly WindowBuilder window = new();
    static readonly NoOpComposer noOpComposer = new();
    static readonly NoOpWriter noOpWriter = new();
    static readonly HtmlComposer htmlComposer = new(noOpWriter);
    static readonly HtmlKeyComposer xtmlComposer = new(noOpWriter, []);
    static readonly Keyhole[] keyholeBuffer = ArrayPool<Keyhole>.Shared.Rent(2048);
    static readonly SnapshotKeyComposer snapshotComposer = new();
    static readonly FindKeyComposer findKeyholeComposer = new();
    static readonly string name = "Rylan";
    static readonly int c = 3;

    [Benchmark]
    public void Basic_String()
    {
        NoOpString noOpString = $"""
            <html>
                <head>
                </head>
                <body>
                    Hello {name}
                    <button>
                        Clicks: {c}
                    </button>
                </body>
            </html>
            """;
    }

    [Benchmark]
    public void Basic_NoOp()
    {
        noOpComposer.Compose($"""
            <html>
                <head>
                </head>
                <body>
                    Hello {name}
                    <button>
                        Clicks: {c}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void Basic_Html()
    {
        noOpWriter.Write($"""
            <html>
            <head>
            </head>
            <body>
                Hello {name}
                <button>
                    Clicks: {c}
                </button>
            </body>
            </html>
            """);
    }

    [Benchmark]
    public async Task Basic_Html_PipeWriter()
    {
        pipe.Writer.Write($"""
            <html>
                <head>
                </head>
                <body>
                    Hello {name}
                    <button>
                        Clicks: {c}
                    </button>
                </body>
            </html>
            """);

        await pipe.Writer.FlushAsync();
        if (pipe.Reader.TryRead(out ReadResult result))
            pipe.Reader.AdvanceTo(result.Buffer.End);
    }

    [Benchmark]
    public void Basic_Xtml()
    {
        xtmlComposer.Writer = noOpWriter;
        xtmlComposer.Listeners = [];
        noOpWriter.Write(xtmlComposer, $"""
            <html>
                <head>
                </head>
                <body>
                    Hello {name}
                    <button>
                        Clicks: {c}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public async Task Basic_Xtml_PipeWriter()
    {
        xtmlComposer.Writer = pipe.Writer;
        xtmlComposer.Listeners = [];
        pipe.Writer.Write(xtmlComposer, $"""
            <html>
                <head>
                </head>
                <body>
                    Hello {name}
                    <button>
                        Clicks: {c}
                    </button>
                </body>
            </html>
            """);

        await pipe.Writer.FlushAsync();
        if (pipe.Reader.TryRead(out ReadResult result))
            pipe.Reader.AdvanceTo(result.Buffer.End);
    }



    [Benchmark]
    public void InlineInterpolated_NoOp()
    {
        noOpComposer.Compose($"""
            <div>
                {$"<p>Hello {name}<p>"}
            </div>
            """);
    }

    [Benchmark]
    public void InlineInterpolated_Html()
    {
        noOpWriter.Write($"""
            <div>
                {$"<p>Hello {name}<p>"}
            </div>
            """);
    }

    [Benchmark]
    public void InlineMethod_NoOp()
    {
        noOpComposer.Compose($"""
            <div>
                {GetInline()}
            </div>
            """);
    }

    [Benchmark]
    public void InlineMethod_Html()
    {
        noOpWriter.Write($"""
            <div>
                {GetInline()}
            </div>
            """);
    }

    Html GetInline() => $"<p>Hello {name}</p>";




    // readonly static BaseComposer baseComposer = new();
    record Point(double X, double Y);
    private static Point[] tiles;

    [Benchmark]
    public void Spiral_NoOp()
    {
        // noOpWriter.Write(noOpComposer, $$"""
        // baseComposer.Compose($$"""
        noOpComposer.Compose($$"""
            <!DOCTYPE html>
            <html>
                <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                body {
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    background-color: #f0f0f0;
                    margin: 0;
                }
                #wrapper {
                    width: 960px;
                    height: 720px;
                    position: relative;
                    background-color: white;
                }
                .tile {
                    position: absolute;
                    width: 10px;
                    height: 10px;
                    background-color: #333;
                }
                </style>
                </head>
                <body>
                <div id="root">
                    {{ tiles.Select(t => Tile(t.X, t.Y)) }}
                </div>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void Spiral_Html()
    {
        noOpWriter.Write($$"""
            <!DOCTYPE html>
            <html>
                <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                body {
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    background-color: #f0f0f0;
                    margin: 0;
                }
                #wrapper {
                    width: 960px;
                    height: 720px;
                    position: relative;
                    background-color: white;
                }
                .tile {
                    position: absolute;
                    width: 10px;
                    height: 10px;
                    background-color: #333;
                }
                </style>
                </head>
                <body>
                <div id="root">
                    {{ tiles.Select(t => Tile(t.X, t.Y)) }}
                </div>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void Spiral_Xtml()
    {
        xtmlComposer.Writer = noOpWriter;
        xtmlComposer.Listeners = [];
        noOpWriter.Write(xtmlComposer, $$"""
            <!DOCTYPE html>
            <html>
                <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                body {
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    background-color: #f0f0f0;
                    margin: 0;
                }
                #wrapper {
                    width: 960px;
                    height: 720px;
                    position: relative;
                    background-color: white;
                }
                .tile {
                    position: absolute;
                    width: 10px;
                    height: 10px;
                    background-color: #333;
                }
                </style>
                </head>
                <body>
                <div id="root">
                    {{ tiles.Select(t => Tile(t.X, t.Y)) }}
                </div>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void Spiral_Find()
    {
        static Html template() => $$"""
            <!DOCTYPE html>
            <html>
                <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                body {
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    background-color: #f0f0f0;
                    margin: 0;
                }
                #wrapper {
                    width: 960px;
                    height: 720px;
                    position: relative;
                    background-color: white;
                }
                .tile {
                    position: absolute;
                    width: 10px;
                    height: 10px;
                    background-color: #333;
                }
                </style>
                </head>
                <body>
                <div id="root">
                    {{tiles.Select(t => Tile(t.X, t.Y))}}
                </div>
                </body>
            </html>
            """;
        findKeyholeComposer.FindEventListener(key: ""u8, template);
    }

    static Html Tile(double x, double y) => $"""
        <div 
            class="tile"
            style="left: {x:0.00}px; top: {y:0.00}px;">
        </div>
        """;





    static EntryRecord[] entries;
    
    [Benchmark]
    public void GuidTable_NoOp()
    {
        noOpComposer.Compose($"""
            <!doctype html>
            <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <link rel="icon" href="/favicon.png" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                </head>
                <body data-sveltekit-preload-data="hover">
                    <div style="display: contents">{GuidTableBody()}</div>
                </body>
            </html>
            """);
    }
    
    [Benchmark]
    public void GuidTable_Html()
    {
        htmlComposer.Writer = noOpWriter;
        noOpWriter.Write(htmlComposer, $"""
            <!doctype html>
            <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <link rel="icon" href="/favicon.png" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                </head>
                <body data-sveltekit-preload-data="hover">
                    <div style="display: contents">{GuidTableBody()}</div>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void GuidTable_Xtml()
    {
        xtmlComposer.Writer = noOpWriter;
        xtmlComposer.Listeners = [];
        noOpWriter.Write(xtmlComposer, $"""
            <!doctype html>
            <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <link rel="icon" href="/favicon.png" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                </head>
                <body data-sveltekit-preload-data="hover">
                    <div style="display: contents">{GuidTableBody()}</div>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void GuidTable_Find()
    {
        static Html template() => $"""
            <!doctype html>
            <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <link rel="icon" href="/favicon.png" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                </head>
                <body data-sveltekit-preload-data="hover">
                    <div style="display: contents">{GuidTableBody()}</div>
                </body>
            </html>
            """;
        FindKeyComposer.Shared.FindEventListener(key: ""u8, template);
    }






    static readonly int i = 1;
    static readonly long l = 1;
    static readonly float f = 3.14f;
    static readonly double d = 3.14;
    static readonly decimal m = 3.14m;
    static readonly DateTime dt = DateTime.Now;
    static readonly DateOnly d0 = DateOnly.MaxValue;
    static readonly TimeSpan ts = TimeSpan.MaxValue;
    static readonly TimeOnly t0 = TimeOnly.MaxValue;
    static readonly bool b = true;
    static readonly Color color = Color.Red;
    static readonly string str = "str";
    static readonly Uri uri = new Uri("https://xtml.dev");


        
    [Benchmark]
    public void SnapshotComposerInt()
    {
        snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {i}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerLong()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {l}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerFloat()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {f}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerDouble()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {d}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerDecimal()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {m}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerDateTime()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {dt}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerDateOnly()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {d0}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerTimeSpan()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {ts}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerTimeOnly()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {t0}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerBool()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {b}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerColor()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {color}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerString()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {str}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerUri()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {uri}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SnapshotComposerFormatString()
    {
        var snapshot = snapshotComposer.Capture(keyholeBuffer, () => $"""
            <html>
                <head>
                </head>
                <body>
                    <button>
                        Clicks: {c:c}
                    </button>
                </body>
            </html>
            """);
    }







    // https://github.com/rosetta-rs/template-benchmarks-rs
    // // Equivalent to passing `size: &usize` into big_table(...)
    // [Params(10, 100)]
    // public int Size { get; set; }

    private List<List<int>> _table = default!;
    private TeamsModel _teams = default!;

    [GlobalSetup]
    public void Setup()
    {
        int size = 100;
        // Build the big table once (like Rust code does outside b.iter)
        _table = new List<List<int>>(size);
        for (int r = 0; r < size; r++)
        {
            var inner = new List<int>(size);
            for (int i = 0; i < size; i++)
                inner.Add(i);

            _table.Add(inner);
        }

        // Build Teams once
        _teams = new TeamsModel(
            Year: 2015,
            Teams: new List<Team>
            {
                new Team("Jiangsu",   43, 0),
                new Team("Beijing",   27, 1),
                new Team("Guangzhou", 22, 2),
                new Team("Shandong",  12, 3),
            }
        );
    }

    [Benchmark]
    public void RustBigTable_NoOp()
    {
        noOpComposer.Compose($"""
            <table>
            {_table.Select(row => (Html)$"""
                <tr>{row.Select(col => (Html)$"""
                    <td>{ col }</td>
                """)}</tr>
            """)}
            </table>
            """);
    }
    
    [Benchmark]
    public void RustBigTable_Html()
    {
        htmlComposer.Writer = noOpWriter;
        noOpWriter.Write(htmlComposer, $"""
            <table>
            {_table.Select(row => (Html)$"""
                <tr>{row.Select(col => (Html)$"""
                    <td>{ col }</td>
                """)}</tr>
            """)}
            </table>
            """);
    }

    [Benchmark]
    public void RustTeams_NoOp()
    {
        noOpComposer.Compose($"""
            <html>
              <head>
                <title>{ _teams.Year }</title>
              </head>
              <body>
                <h1>CSL { _teams.Year }</h1>
                <ul>
                { _teams.Teams.Select(team => (Html)$"""
                    <li class="{(team.Index == 0 ? "champion" : "")}">
                    <b>{ team.Name }</b>: { team.Score }
                    </li>
                """)}
                </ul>
              </body>
            </html>
            """);
    }
    
    [Benchmark]
    public void RustTeams_Html()
    {
        htmlComposer.Writer = noOpWriter;
        noOpWriter.Write(htmlComposer, $"""
            <html>
              <head>
                <title>{ _teams.Year }</title>
              </head>
              <body>
                <h1>CSL { _teams.Year }</h1>
                <ul>
                { _teams.Teams.Select(team => (Html)$"""
                    <li class="{(team.Index == 0 ? "champion" : "")}">
                    <b>{ team.Name }</b>: { team.Score }
                    </li>
                """)}
                </ul>
              </body>
            </html>
            """);
    }

// ----------------- Data models -----------------

public record struct TeamsModel(ushort Year, List<Team> Teams);
public record struct Team(string Name, byte Score, int Index);








    static Html GuidTableBody() => $"""
        <main>
          <table>
            {from entry in entries
             select Entry(entry)}
          </table>
        </main>
    """;

    static Html Entry(EntryRecord entry) => $"""
    <tr>
      <td>{entry.ID}</td>
      <td>{entry.Name}</td>
    </tr>
    """;

    record class EntryRecord(string ID, string Name);



    static Tests()
    {
        var wrapperWidth = 960;
        var wrapperHeight = 720;
        var cellSize = 10;
        var centerX = wrapperWidth / 2d;
        var centerY = wrapperHeight / 2d;

        var angle = 0d;
        var radius = 0d;

        var tiles = new List<Point>();
        var step = cellSize;

        while (radius < Math.Min(wrapperWidth, wrapperHeight) / 2d)
        {
            var x = centerX + Math.Cos(angle) * radius;
            var y = centerY + Math.Sin(angle) * radius;

            if (x >= 0 && x <= wrapperWidth - cellSize && y >= 0 && y <= wrapperHeight - cellSize)
            {
                tiles.Add(new(x, y));
            }

            angle += 0.2;
            radius += step * 0.015;
        }

        Tests.tiles = tiles.ToArray();




        Tests.entries = [.. Enumerable
            .Range(0, 1000)
            .Select(i => new EntryRecord(
                ID: Guid.NewGuid().ToString(),
                Name: Guid.NewGuid().ToString()
            ))
        ];
    }
}








// if (args.Length == 0)
// {
//     Console.WriteLine("Which test?\n  benchmarkdotnet\n  threads n");
//     return;
// }

// switch (args[0])
// {
//     case "benchmarkdotnet":
//         BenchmarkRunner.Run<UI>();
//         break;
//     case "threads":
//         int threads = 1;
//         bool noop = false;
//         if (args.Length > 1)
//         {
//             threads = int.Parse(args[1]);
//             noop = args[^1] == "noop";
//         }
//         Threads.Test(threads, noop);
//         break;
//     default:
//         throw new NotSupportedException("");
// }

