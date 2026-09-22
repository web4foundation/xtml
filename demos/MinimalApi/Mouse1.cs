#!/usr/bin/dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:project ../../src/Xtml.WebSocket/Xtml.WebSocket.csproj

using Xtml.WebSocket;
using Xtml.Dom;

var app = WebApplication.Create(args);

var c = 0;
var x = 0.0;
var y = 0.0;

var window = app.MapUI("/", () => $"""
    <!doctype html>
    <html>
        <body>
            <h3>Mouse: {x},{y}</h3>
            <button onclick={OnClick}>
                Clicks: {c}
            </button>
        </body>
    </html>
    """);

void OnClick(Event.Mouse e)
{
    c++;
}

window.OnMouseMove = e =>
{
    x = e.X;
    y = e.Y;
};

app.Run();
