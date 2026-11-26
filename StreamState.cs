namespace ScreenStreamer;

public static class StreamState
{
    private static volatile bool _isStreaming;

    public static bool IsStreaming
    {
        get => _isStreaming;
        set => _isStreaming = value;
    }
}