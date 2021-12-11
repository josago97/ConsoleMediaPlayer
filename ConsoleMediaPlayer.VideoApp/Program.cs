namespace ConsoleMediaPlayer.VideoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Introduce ruta de un vídeo:");
            string filePath = Console.ReadLine().Replace("\"", "");

            VideoPlayer videoPlayer = new VideoPlayer(filePath);
            videoPlayer.ResizeConsoleScreen();
            videoPlayer.Play();

            Task.Delay(-1).Wait();
        }
    }
}