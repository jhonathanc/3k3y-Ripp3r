using System.Drawing;

namespace Ripp3r.Controls.Hex
{
    public class Range
    {
        public Range(int start, int end, Brush brush, Brush backGround)
        {
            Start = start;
            End = end;
            Brush = brush;
            BackGround = backGround;
        }

        public long Start { get; set; }

        public long End { get; set; }

        public Brush Brush { get; set; }

        public Brush BackGround { get; set; }

        public bool Enabled { get; set; }

        public bool Contains(int start, int end)
        {
            return Start <= start && End >= end;
        }

        public override string ToString()
        {
            return string.Format("Start: {0}, End: {1}, Enabled: {2}", Start, End, Enabled);
        }
    }
}