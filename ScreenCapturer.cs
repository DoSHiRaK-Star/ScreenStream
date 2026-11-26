using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ScreenStreamer;

public class ScreenCapturer
{
    public byte[] CaptureJpeg(int quality = 60, int monitorIndex = 0)
    {
        var screens = Screen.AllScreens;
        if (monitorIndex < 0 || monitorIndex >= screens.Length)
            monitorIndex = 0;

        var bounds = screens[monitorIndex].Bounds;

        using var bitmap = new Bitmap(bounds.Width, bounds.Height);

        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(
                bounds.X, bounds.Y,
                0, 0,
                bounds.Size);

            // Рисуем курсор
            try
            {
                var cursorPos = Cursor.Position;
                Cursors.Default.Draw(graphics, new Rectangle(
                    cursorPos.X - bounds.X - 8,
                    cursorPos.Y - bounds.Y - 8,
                    32, 32));
            }
            catch
            {
                // курсор не критичен
            }
        }

        using var ms = new MemoryStream();

        var jpegEncoder = ImageCodecInfo.GetImageEncoders()
            .First(c => c.FormatID == ImageFormat.Jpeg.Guid);

        using var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(
            System.Drawing.Imaging.Encoder.Quality, quality);

        bitmap.Save(ms, jpegEncoder, parameters);
        return ms.ToArray();
    }
}