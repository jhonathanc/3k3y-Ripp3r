using System;
using System.Diagnostics;
using System.Timers;
using Ripp3r;

namespace ripp3rcon
{
    internal class ConsoleWriter
    {
        private bool _hasProgress;
        private int _maximum;
        private int _value;
        private Stopwatch _stopWatch;
        private readonly Timer _timer;

        public ConsoleWriter()
        {
            _timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
            _timer.Elapsed += OnTimer;
            _timer.Enabled = false;
        }

        private void ClearLastLine()
        {
            Console.CursorLeft = 0;
            if (Console.WindowWidth != 0)
                Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.CursorLeft = 0;
        }

        public void Write(string msg, ReportType reportType = ReportType.Normal)
        {
            if (_hasProgress) ClearLastLine();

            ConsoleColor fg = Console.ForegroundColor;
            switch (reportType)
            {
                case ReportType.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case ReportType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case ReportType.Fail:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case ReportType.Url:
                    return; // No urls in the console
            }
            // Change the foreground color
            Console.WriteLine(msg);

            Console.ForegroundColor = fg;
        }

        public void StartProgress()
        {
            _stopWatch = Stopwatch.StartNew();
            _timer.Enabled = true;
        }

        public void StopProgress()
        {
            _timer.Enabled = false;
            if (_hasProgress) ClearLastLine();
            _hasProgress = false;
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            // Calculate ETA
            TimeSpan remaining =
                new TimeSpan((long)((_stopWatch.Elapsed.Ticks / (_value * 1.0)) * _maximum) -
                             _stopWatch.Elapsed.Ticks);

            ReportProgress(_value, _maximum, remaining.ToString(@"hh\:mm\:ss"));
        }

        public void SetProgressValue(int val)
        {
            _value = val;
        }

        public void SetProgressMaximum(int max)
        {
            _maximum = max;
        }

        private void ReportProgress(int progress, int total, string eta)
        {
            int width = Console.WindowWidth - 1;
            int progressWidth = width - 14;

            _hasProgress = true;

            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write('['); //start

            float onechunk = ((progressWidth - 2) * 1.0f) / total;
            int progressChunk = (int) (onechunk*progress);

            //draw filled part
            Console.CursorLeft = 1;
            Console.Write(new string('-', progressChunk));

            // draw empty part
            Console.Write(new string(' ', progressWidth - progressChunk - 2));

            //draw eta
            Console.Write(@"] ETA: " + eta);
        }
    }
}
