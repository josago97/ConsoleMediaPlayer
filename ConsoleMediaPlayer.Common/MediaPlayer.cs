using System.Drawing;

namespace ConsoleMediaPlayer.Common
{
    public abstract class MediaPlayer
    {
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
            Console.SetWindowSize(Resolution.Width + 1, Resolution.Height + 2);
        }

        protected Size CalculateResolution(Size originalResolution, int desiredHeight)
        {
            double aspectRatio = originalResolution.Width / (double)originalResolution.Height;
            int width = (int)(desiredHeight * aspectRatio);
            int height = desiredHeight / 2;

            return new Size(width, height);
        }

    }
}
