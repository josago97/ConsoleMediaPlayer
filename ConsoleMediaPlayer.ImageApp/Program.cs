namespace ConsoleMediaPlayer.ImageApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Introduce ruta de una imagen:");
            string filePath = Console.ReadLine().Replace("\"", "");

            ImagePlayer imagePlayer = new ImagePlayer(filePath);
            imagePlayer.ResizeConsoleScreen();
            imagePlayer.Play();

            Task.Delay(-1).Wait();
        }
    }
}