global using Xtml.WebSocket;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// var _name = "World";

app.MapGet("/classic", () => "Hello World");
app.MapGet("/wat1/{name}", (string name) => "Hello {name}");
app.MapGet("/hello/{name:alpha}", (string name) => $"Hello {name}!");

app.MapGet("/wat2/{c}", (int c) => $"Hello {c:c}");
app.MapGet("/interpolation", () => $"Hello {14:c}");

app.MapUI("xtml", () => $"Hello {14:c}");
// app.MapXtml("xtml/ctx", ctx => $"Hello {14:c}");


app.Run();