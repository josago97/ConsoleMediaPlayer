using ConsoleMediaPlayer.Common;
using CSCore.Codecs.MP3;
using CSCore.SoundOut;
using FFMpegCore;
using FFMpegCore.Pipes;
using MemoryTributaryS;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace ConsoleMediaPlayer.VideoApp
{
    public class VideoPlayer : MediaPlayer
    {
        private const int DEFAULT_HEIGHT = 250;
        private const int FPS_LIMIT = 24;
        private static string ASCII_PIXEL_TABLE = @"█▓@8#x+o=:-. ";

        private ISoundOut _soundOut;

        public int FramesCount { get; private set; }
        public int BytesPerFrame { get; private set; }
        public double FPS { get; private set; }
        public ConcurrentList<string> Frames { get; private set; }
        protected override short FontSize => 6;

        public VideoPlayer(string filePath, int height = DEFAULT_HEIGHT) : base(filePath)
        {
            _soundOut = GetSoundOut();
            Frames = new ConcurrentList<string>();
            ExtractMetadata(height);
        }

        private void ExtractMetadata(int height)
        {
            VideoStream? videoData = FFProbe.Analyse(FilePath).PrimaryVideoStream;

            if (videoData == null) throw new Exception("No se puede extraer información del vídeo");

            FPS = Math.Min(videoData.AvgFrameRate, FPS_LIMIT);
            Resolution = CalculateResolution(new Size(videoData.Width, videoData.Height), height);
            BytesPerFrame = CalculateBytesPerFrame(Resolution.Width, Resolution.Height);
            FramesCount = (int)(FPS * videoData.Duration.TotalSeconds);
        }

        private int CalculateBytesPerFrame(int width, int height)
        {
            int lineSizeWithoutPadding = width * 3;
            int lineSizeWithPadding = (int)Math.Ceiling(lineSizeWithoutPadding / 4f) * 4;
            return 54 + lineSizeWithPadding * height;
        }

        public override async void Play()
        {
            ExtractFramesAsync();
            Stream audio = await ExtractAudioAsync();

            SpinWait.SpinUntil(() => Frames.Count > 0);

            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            while (true)
            {
                Stopwatch timer = new Stopwatch();
                long waitTicks = 0;

                PlaySound(audio);
                timer.Start();

                for (int i = 0; i < Frames.Count; i++)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine(Frames[i]);

                    waitTicks += (long)(1000 / FPS * TimeSpan.TicksPerMillisecond - timer.ElapsedTicks);
                    timer.Restart();

                    if (waitTicks > 0)
                    {
                        Thread.Sleep(TimeSpan.FromTicks(waitTicks));
                    }
                    else
                    {
                    }
                }

                SpinWait.SpinUntil(() => _soundOut.PlaybackState != PlaybackState.Playing);
            }
        }

        public async Task<Stream> ExtractAudioAsync()
        {
            MemoryStream stream = new MemoryStream();

            await FFMpegArguments
                    .FromFileInput(FilePath)
                    .OutputToPipe(new StreamPipeSink(stream), options => options
                        .WithCustomArgument("-vn -f mp3"))
                    .ProcessAsynchronously();

            stream.Position = 0;

            return stream;
        }

        public async void ExtractFramesAsync()
        {
            using (var stream = new MemoryTributary())
            {
                await FFMpegArguments
                    .FromFileInput(FilePath)
                    .OutputToPipe(new StreamPipeSink(stream), options => options
                        .ForceFormat("rawvideo")
                        .WithFramerate(FPS)
                        .WithVideoCodec("bmp")
                        .WithCustomArgument("-ss 00:00")
                        .Resize(Resolution.Width, Resolution.Height)
                    ).ProcessAsynchronously();

                stream.Position = 0;
                byte[] buffer = new byte[BytesPerFrame];
                Frames.Clear();

                while (stream.Position < stream.Length)
                {
                    int bytesReaded = stream.Read(buffer, 0, BytesPerFrame);

                    using (MemoryStream auxStream = new MemoryStream(buffer, 0, bytesReaded))
                    {
                        auxStream.Position = 0;
                        Bitmap frame = (Bitmap)Image.FromStream(auxStream);
                        Frames.Add(RenderFrame(frame));
                    }
                }
            }
        }

        private string RenderFrame(Bitmap frame)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Rectangle rect = new Rectangle(0, 0, frame.Width, frame.Height);
            BitmapData frameData = frame.LockBits(rect, ImageLockMode.ReadWrite, frame.PixelFormat);

            byte[] rgbByRow = new byte[Math.Abs(frameData.Stride)];
            IntPtr readIndex = frameData.Scan0;

            for (int h = 0; h < frameData.Height; h++)
            {
                System.Runtime.InteropServices.Marshal.Copy(readIndex, rgbByRow, 0, rgbByRow.Length);

                for (int w = 0; w < frame.Width; w++)
                {
                    int startIndex = w * 3;
                    int b = rgbByRow[startIndex];
                    int g = rgbByRow[startIndex + 1];
                    int r = rgbByRow[startIndex + 2];

                    int grayScaledPixel = (int)Math.Min(r * 0.2627 + g * 0.6780 + b * 0.0593, 255);

                    int saturationLevel = (int)Math.Round((grayScaledPixel / 255.0) * (ASCII_PIXEL_TABLE.Length - 1), MidpointRounding.AwayFromZero);
                    stringBuilder.Append(ASCII_PIXEL_TABLE[saturationLevel]);
                }

                readIndex = IntPtr.Add(readIndex, rgbByRow.Length);
                stringBuilder.AppendLine();
            }

            frame.UnlockBits(frameData);

            return stringBuilder.ToString();
        }

        private void PlaySound(Stream stream)
        {
            stream.Position = 0;

            _soundOut.Initialize(new DmoMp3Decoder(stream));
            _soundOut.Play();
        }

        private ISoundOut GetSoundOut()
        {
            ISoundOut soundOut;

            if (WasapiOut.IsSupportedOnCurrentPlatform)
                soundOut = new WasapiOut();
            else
                soundOut = new DirectSoundOut();

            return soundOut;
        }
    }
}
