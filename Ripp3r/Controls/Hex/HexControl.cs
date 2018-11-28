using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Ripp3r.Controls.Hex
{
    public class HexControl : UserControl
    {
        private readonly Container components = null;
        private int hexDataOffset_;
        private Cache cache_;
        private Size cellSize;
        private byte columns_ = 8;
        private int hexAscLimit;
        private ScrollBar hs;
        private ArrayList ranges_;
        private SolidBrush selBackground_;
        private SolidBrush selColor_;
        private ScrollBar vs;

        public HexControl()
        {
            InitializeComponent();
            InitializeRanges();
            InitializeControl();
        }
        private void InitializeControl()
        {
            using (Graphics graphics = CreateGraphics())
            {
                cellSize = MeasureCellSize(graphics, Font);
                hexDataOffset_ = 10 + (int) MeasureDisplayStringWidth(graphics, "00000000", Font) + 10;
                CalculateHexAscIILimit();
            }
            EnableDoubleBuffering();
        }

        [Browsable(false)]
        public Stream Stream
        {
            get { return cache_.Stream; }
            set
            {
                if (value != null)
                {
                    if (!value.CanSeek)
                        throw new ArgumentException(@"Stream must support seek operation.", "value");
                    cache_ = new Cache(value, (int) Utilities.SectorSize);
                }
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Selection Color")]
        public Color SelectionColor
        {
            get { return selColor_ != null ? selColor_.Color : Color.Yellow; }
            set { selColor_ = new SolidBrush(value); }
        }

        [Category("Appearance")]
        [Description("Selection Background Color")]
        public Color SelectionBackground
        {
            get { return selBackground_ != null ? selBackground_.Color : Color.Black; }
            set { selBackground_ = new SolidBrush(value); }
        }

        [Description("# of byte columns to show")]
        [Category("Appearance")]
        public byte Columns
        {
            get { return columns_; }
            set
            {
                columns_ = value;
                CalculateHexAscIILimit();
                Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            AutoScroll = true;
            BackColor = Color.Black;
            Font = new Font("Courier New", 11.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
            ForeColor = Color.FromArgb(byte.MaxValue, byte.MaxValue, 128);
            Name = "HexControl";
            Size = new Size(376, 344);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (vs == null)
                return;
            vs.Value = Math.Min(Math.Max(vs.Minimum, vs.Value - (e.Delta > 0 ? cellSize.Height : -cellSize.Height)),
                                vs.Maximum);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ActiveControl = null;
            int y = 10 - VerticalScrollPosition();
            float x1 = HorizontalScrollPosition();
            float num = x1;
            long offset = 0L;
            if (y < 0)
            {
                offset = Math.Abs(y)/(cellSize.Height + 5)*columns_;
                y %= cellSize.Height + 5;
            }
            var color = new Colors(ForeColor, BackColor);
            string s = string.Format("{0:X8}", offset);
            e.Graphics.DrawString(s, Font, Brushes.DimGray, x1, y);
            float x2 = num + hexDataOffset_;
            bool newLine;
            float asciiX;
            while (DrawHex(e.Graphics, ref offset, out asciiX, ref x2, y, color, out newLine) &&
                   y < (double) e.Graphics.ClipBounds.Height)
            {
                if (newLine)
                {
                    y += cellSize.Height + 5;
                    e.Graphics.DrawString(string.Format("{0:X8}", offset), Font, Brushes.DimGray, x1, y);
                    x2 = x1 + hexDataOffset_;
                }
            }
            UpdateHorizontalScrollBar(asciiX);
            if (y + cellSize.Height > Height && vs == null)
                AddVerticalScrollBar();
            UpdateScrollBars(cellSize.Height);
        }

        private void DrawByte(Graphics g, float x, float y, byte b, Brush color)
        {
            x -= Font.Size/5f;
            g.DrawString(char.ToString(HexToChar((byte) ((uint) b >> 4))), Font, color, x, y);
            g.DrawString(char.ToString(HexToChar((byte) (b & 15U))), Font, color, x + cellSize.Width, y);
        }

        private void DrawAscII(Graphics g, float x, float y, byte b, Brush color)
        {
            g.DrawString(char.ToString((int) b < 32 || (int) b > 126 ? '.' : (char) b), Font, color, x - Font.Size/5f, y);
        }

        private bool DrawHex(Graphics g, ref long offset, out float asciiX, ref float x, int y, Colors color,
                             out bool newLine)
        {
            asciiX = 0.0f;
            bool flag = false;
            if (offset < cache_.StreamLength && y < Height)
            {
                Colors color1 = color;
                long start = offset;
                long end;
                GetDataBounds(cache_.StreamLength, offset, out end, ref color1);
                asciiX = IndexToAscIIScreenColumn(start);
                g.FillRectangle(color1.BackColor, asciiX, y, (cellSize.Width*(end - start)), cellSize.Height);
                g.FillRectangle(color1.BackColor, x, y, ((cellSize.Width*2 + 10)*(end - start) - 10L), cellSize.Height);
                for (long index = start; index < end; ++index)
                {
                    byte @byte = cache_.GetByte(index);
                    DrawByte(g, x, y, @byte, color1.ForeColor);
                    DrawAscII(g, asciiX, y, @byte, color1.ForeColor);
                    asciiX += cellSize.Width;
                    x += (cellSize.Width*2 + 10);
                }
                offset = end;
                flag = true;
            }
            newLine = offset%columns_ == 0L;
            return flag;
        }

        private void UpdateHorizontalScrollBar(float asciiX)
        {
            if (asciiX < (double) Width)
                return;
            AddHorizontalScrollBar();
        }

        private void DOScroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        private void AddHorizontalScrollBar()
        {
            if (hs != null)
                return;
            hs = new HScrollBar {Dock = DockStyle.Bottom};
            hs.Scroll += DOScroll;
            Controls.Add(hs);
            HScroll = true;
        }

        private void AddVerticalScrollBar()
        {
            vs = new VScrollBar {Dock = DockStyle.Right, LargeChange = cellSize.Height};
            Controls.Add(vs);
            vs.Scroll += DOScroll;
        }

        private void UpdateScrollBars(float height)
        {
            if (cache_.StreamLength <= 0 || vs == null)
                return;
            vs.Maximum = (int) ((cache_.StreamLength/columns_)*(double) height);
        }

        private void EnableDoubleBuffering()
        {
            DoubleBuffered = true;
        }

        private void InitializeRanges()
        {
            ranges_ = new ArrayList();
        }

        private int VerticalScrollPosition()
        {
            return vs == null ? 0 : vs.Value;
        }

        private int HorizontalScrollPosition()
        {
            return hs == null ? 0 : hs.Value;
        }

        private char HexToChar(byte b)
        {
            return (int) b > 9 ? (char) (b - 10 + 65) : (char) (b + 48);
        }

        private long IndexToDataColumn(long index)
        {
            return index%columns_;
        }

        private long IndexToDataLine(long index)
        {
            return index/columns_;
        }

        private long IndexToAscIIScreenColumn(long start)
        {
            return hexAscLimit + IndexToDataColumn(start)*cellSize.Width;
        }

        private float IndexToScreenLine(long index)
        {
            return (10L + IndexToDataLine(index)*(cellSize.Height + 5) - VerticalScrollPosition());
        }

        private bool IsSameLine(long p1, long p2, long columns)
        {
            long num1 = p1%columns;
            long num2 = p2%columns;
            return p1 - p2 == num1 - num2;
        }

        private static float MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
        {
            var stringFormat = new StringFormat();
            var layoutRect = new RectangleF(0.0f, 0.0f, 1000f, 1000f);
            var ranges = new[]
                {
                    new CharacterRange(0, text.Length)
                };
            stringFormat.SetMeasurableCharacterRanges(ranges);
            return graphics.MeasureCharacterRanges(text, font, layoutRect, stringFormat)[0].GetBounds(graphics).Right;
        }

        private static Size MeasureCellSize(Graphics graphics, Font f)
        {
            var stringFormat = new StringFormat {FormatFlags = StringFormatFlags.NoClip};
            var layoutRect = new RectangleF(0.0f, 0.0f, 1000f, 1000f);
            var ranges = new[]
                {
                    new CharacterRange(0, 1)
                };
            stringFormat.SetMeasurableCharacterRanges(ranges);
            return
                graphics.MeasureCharacterRanges("W", f, layoutRect, stringFormat)[0].GetBounds(graphics).Size.ToSize();
        }

        private void CalculateHexAscIILimit()
        {
            hexAscLimit = 10 + hexDataOffset_ + 10 + columns_*cellSize.Width*2 + 10*(columns_ - 1);
        }

        private void GetDataBounds(long maxLen, long start, out long end, ref Colors color)
        {
            end = Math.Min(start + columns_ - start%columns_, maxLen);
            foreach (Range range in ranges_)
            {
                if (range.Enabled)
                {
                    if (range.Start <= start && range.End >= start)
                    {
                        end = Math.Min(end, range.End + 1L);
                        color.ForeColor = range.Brush;
                        color.BackColor = range.BackGround;
                        break;
                    }
                    if (start < range.Start && IsSameLine(start, range.Start, columns_))
                    {
                        end = range.Start;
                        break;
                    }
                }
            }
        }

        private struct Colors
        {
            internal Brush BackColor;
            internal Brush ForeColor;

            internal Colors(Color foreColor, Color backColor)
            {
                ForeColor = new SolidBrush(foreColor);
                BackColor = new SolidBrush(backColor);
            }
        }
    }
}