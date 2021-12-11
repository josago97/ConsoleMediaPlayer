using ConsoleMediaPlayer.Common;
using System.Drawing;
using System.Drawing.Imaging;

namespace ConsoleMediaPlayer.ImageApp
{
    public class ImagePlayer : MediaPlayer
    {
        private const int DEFAULT_HEIGHT = 600;
        private static char ASCII_PIXEL = '█';
        private readonly static Dictionary<ConsoleColor, Color> CONSOLE_COLORS = new Dictionary<ConsoleColor, Color>()
        {
           { ConsoleColor.Black, Color.Black },
           { ConsoleColor.DarkBlue, Color.DarkBlue },
           { ConsoleColor.DarkGreen, Color.DarkGreen },
           { ConsoleColor.DarkCyan, Color.DarkCyan },
           { ConsoleColor.DarkRed, Color.DarkRed },
           { ConsoleColor.DarkMagenta, Color.DarkMagenta },
           { ConsoleColor.DarkYellow, Color.DarkGoldenrod },
           { ConsoleColor.Gray, Color.Gray },
           { ConsoleColor.DarkGray, Color.DarkGray },
           { ConsoleColor.Blue, Color.Blue },
           { ConsoleColor.Green, Color.Lime },
           { ConsoleColor.Cyan, Color.Cyan },
           { ConsoleColor.Red, Color.Red },
           { ConsoleColor.Magenta, Color.Magenta },
           { ConsoleColor.Yellow, Color.Yellow },
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
            float aHue = a.GetHue();
            float bHue = b.GetHue();

            if (aHue > 345) aHue = 0;
            if (bHue > 345) bHue = 0;

            float aSaturation = a.GetSaturation();
            float bSaturation = b.GetSaturation();

            float aBrightness = a.GetBrightness();
            float bBrightness = b.GetBrightness();

            return Math.Abs((aHue - bHue) / 360f) + Math.Abs(aSaturation - bSaturation) + Math.Abs(aBrightness - bBrightness);
        }
    }
}
