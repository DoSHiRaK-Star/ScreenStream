using System.Net;
using System.Net.Sockets;
using ScreenStreamer;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var capturer = new ScreenCapturer();

// ---- MJPEG-поток ----
app.MapGet("/stream", async (HttpContext context,
                             int fps = 10,
                             int quality = 60,
                             int monitor = 0) =>
{
    context.Response.ContentType = "multipart/x-mixed-replace; boundary=--frame";
    context.Response.Headers["Cache-Control"] = "no-cache";

    int delay = 1000 / Math.Clamp(fps, 1, 30);

    while (StreamState.IsStreaming && !context.RequestAborted.IsCancellationRequested)
    {
        try
        {
            byte[] jpeg = capturer.CaptureJpeg(quality, monitor);

            string header =
                $"--frame\r\n" +
                $"Content-Type: image/jpeg\r\n" +
                $"Content-Length: {jpeg.Length}\r\n\r\n";

            await context.Response.WriteAsync(header);
            await context.Response.Body.WriteAsync(jpeg);
            await context.Response.WriteAsync("\r\n");
            await Task.Delay(delay);
        }
        catch
        {
            break;
        }
    }
});

// ---- Управление ----
app.MapPost("/start", () =>
{
    StreamState.IsStreaming = true;
    return Results.Ok();
});

app.MapPost("/stop", () =>
{
    StreamState.IsStreaming = false;
    return Results.Ok();
});

// ---- IP ----
app.MapGet("/api/ip", () =>
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    var localIp = host.AddressList
        .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
        ?.ToString() ?? "127.0.0.1";
    return Results.Text(localIp);
});

// ---- Статус ----
app.MapGet("/api/status", () =>
    Results.Json(new { streaming = StreamState.IsStreaming }));

app.Run("http://0.0.0.0:5000");