using ConsoleMediaPlayer.Common;
using System.Drawing;
using System.Drawing.Imaging;

namespace ConsoleMediaPlayer.ImageApp;

public class ImagePlayer : MediaPlayer
{
    private const int DEFAULT_HEIGHT = 600;
    private static char ASCII_PIXEL = '█';
    private readonly static Dictionary<ConsoleColor, Color> CONSOLE_COLORS = new Dictionary<ConsoleColor, Color>()
    {
       { ConsoleColor.Black, Color.Black },
       { ConsoleColor.DarkBlue, Color.FromArgb(0, 55, 218) },
       { ConsoleColor.DarkGreen, Color.FromArgb(19, 161, 14) },
       { ConsoleColor.DarkCyan, Color.FromArgb(58, 150, 221) },
       { ConsoleColor.DarkRed, Color.FromArgb(197, 15, 31) },
       { ConsoleColor.DarkMagenta, Color.FromArgb(136, 23, 152) },
       { ConsoleColor.DarkYellow, Color.FromArgb(193, 156, 0) },
       { ConsoleColor.Gray, Color.FromArgb(204, 204, 204) },
       { ConsoleColor.DarkGray, Color.FromArgb(118, 118, 118) },
       { ConsoleColor.Blue, Color.FromArgb(59, 120, 255) },
       { ConsoleColor.Green, Color.FromArgb(22, 198, 12) },
       { ConsoleColor.Cyan, Color.FromArgb(97, 214, 214) },
       { ConsoleColor.Red, Color.Red },
       { ConsoleColor.Magenta, Color.FromArgb(180, 0, 158) },
       { ConsoleColor.Yellow, Color.FromArgb(234, 250, 2) },
       { ConsoleColor.White, Color.White }
    };

    public Bitmap Image { get; private set; }

    protected override short FontSize => 2;

    public ImagePlayer(string filePath, int height = DEFAULT_HEIGHT) : base(filePath)
    {
        ExtractImage(height);
    }

    private void ExtractImage(int height)
    {
        Bitmap image = (Bitmap)Bitmap.FromFile(FilePath);
        Resolution = CalculateResolution(image.Size, height);
        image = new Bitmap(image, Resolution);
        Image = image.Clone(new Rectangle(new Point(0, 0), Resolution), PixelFormat.Format24bppRgb);
    }

    public override void Play()
    {
        Console.BackgroundColor = ConsoleColor.White;
        Console.SetCursorPosition(0, 0);

        Rectangle rect = new Rectangle(0, 0, Image.Width, Image.Height);
        BitmapData imageData = Image.LockBits(rect, ImageLockMode.ReadWrite, Image.PixelFormat);

        byte[] rgbByRow = new byte[Math.Abs(imageData.Stride)];
        IntPtr readIndex = imageData.Scan0;

        for (int h = 0; h < imageData.Height; h++)
        {
            System.Runtime.InteropServices.Marshal.Copy(readIndex, rgbByRow, 0, rgbByRow.Length);

            for (int w = 0; w < imageData.Width; w++)
            {
                int startIndex = w * 3;
                int b = rgbByRow[startIndex];
                int g = rgbByRow[startIndex + 1];
                int r = rgbByRow[startIndex + 2];

                ConsoleColor color = ColorToConsoleColor(Color.FromArgb(r, g, b));
                Console.ForegroundColor = color;
                Console.Write(ASCII_PIXEL);
            }

            readIndex = IntPtr.Add(readIndex, rgbByRow.Length);
            Console.WriteLine();
        }

        Image.UnlockBits(imageData);
    }

    private ConsoleColor ColorToConsoleColor(Color color)
    {
        ConsoleColor nearest = ConsoleColor.White;
        double minDistance = int.MaxValue;

        foreach (var pair in CONSOLE_COLORS)
        {
            ConsoleColor consoleColor = pair.Key;
            Color colorRGB = pair.Value;

            double distance = Distance(colorRGB, color);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = consoleColor;
            }
        }

        return nearest;
    }

    private double Distance(Color a, Color b)
    {
        return Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
    }
}
