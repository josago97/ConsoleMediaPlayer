using System.Drawing;

namespace ConsoleMediaPlayer.Common
{
    public abstract class MediaPlayer
    {
        static readonly Size MARGIN = new Size(1, 2);
        protected abstract short FontSize { get; }
        public string FilePath { get; init; }
        public Size Resolution { get; protected set; }

        public MediaPlayer(string filePath)
        {
            if (!File.Exists(filePath)) throw new Exception($"El archivo {filePath} no existe");

            FilePath = filePath;
        }

        public abstract void Play();

        public void ResizeConsoleScreen()
        {
            ConsoleHelper.SetCurrentFont(FontSize);
            int width = Resolution.Width + MARGIN.Width;
            int height = Resolution.Height + MARGIN.Height;
            Console.SetWindowSize(width, height);
        }

        protected Size CalculateResolution(Size originalResolution, int desiredHeight)
        {
            double aspectRatio = originalResolution.Width / (double)originalResolution.Height;
            int maxWindowHeight = Console.LargestWindowHeight - MARGIN.Height;
            int height = Math.Min(maxWindowHeight * 2, desiredHeight);
            int width = (int)(height * aspectRatio);

            return new Size(width, height / 2);
        }

    }
}
