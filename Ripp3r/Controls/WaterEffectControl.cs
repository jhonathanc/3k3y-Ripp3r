﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Ripp3r.Controls
{
    /// <summary>
    /// </summary>
    public sealed class WaterEffectControl : Panel
    {
        private int _activeBuffer;
        private Bitmap _bmp;
        private BitmapData _bmpBitmapData;
        private byte[] _bmpBytes;
        private int _bmpHeight, _bmpWidth;
        private int _waveHeight;
        private int _waveWidth;
        private short[,,] _waves;
        private bool _weHaveWaves;
        private IContainer components;
        private Timer effectTimer;


        public WaterEffectControl()
        {
            InitializeComponent();
            effectTimer.Enabled = true;
            effectTimer.Interval = 50;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            BackColor = Color.White;
            _weHaveWaves = false;
            ResolutionScale = 1;
        }

        private void InitializeComponent()
        {
            components = new Container();
            effectTimer = new Timer(components);
            // 
            // effectTimer
            // 
            effectTimer.Tick += effectTimer_Tick;
            // 
            // WaterEffectControl
            // 
            Paint += WaterEffectControl_Paint;
            MouseMove += WaterEffectControl_MouseMove;
        }

        protected override void Dispose(bool disposing)
        {
            _bmp.UnlockBits(_bmpBitmapData);
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        ///     Timer handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void effectTimer_Tick(object sender, EventArgs e)
        {
            if (_weHaveWaves)
            {
                Invalidate();

                ProcessWaves();
            }
        }

        /// <summary>
        ///     Paint handler
        ///     Calculates the final effect-image out of
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaterEffectControl_Paint(object sender, PaintEventArgs e)
        {
            using (var tmp = (Bitmap) _bmp.Clone())
            {
                if (_weHaveWaves)
                {
                    BitmapData tmpData = tmp.LockBits(new Rectangle(0, 0, _bmpWidth, _bmpHeight),
                                                      ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                    var tmpBytes = new Byte[_bmpWidth*_bmpHeight*4];

                    Marshal.Copy(tmpData.Scan0, tmpBytes, 0, _bmpWidth*_bmpHeight*4);

                    for (int x = 1; x < _bmpWidth - 1; x++)
                    {
                        for (int y = 1; y < _bmpHeight - 1; y++)
                        {
                            int waveX = x >> ResolutionScale;
                            int waveY = y >> ResolutionScale;

                            //check bounds
                            if (waveX <= 0) waveX = 1;
                            if (waveY <= 0) waveY = 1;
                            if (waveX >= _waveWidth - 1) waveX = _waveWidth - 2;
                            if (waveY >= _waveHeight - 1) waveY = _waveHeight - 2;

                            //this gives us the effect of water breaking the light
                            int xOffset = (_waves[waveX - 1, waveY, _activeBuffer] - _waves[waveX + 1, waveY, _activeBuffer]) >>
                                          3;
                            int yOffset = (_waves[waveX, waveY - 1, _activeBuffer] - _waves[waveX, waveY + 1, _activeBuffer]) >>
                                          3;

                            if ((xOffset == 0) && (yOffset == 0)) continue;

                            //check bounds
                            if (x + xOffset >= _bmpWidth - 1) xOffset = _bmpWidth - x - 1;
                            if (y + yOffset >= _bmpHeight - 1) yOffset = _bmpHeight - y - 1;
                            if (x + xOffset < 0) xOffset = -x;
                            if (y + yOffset < 0) yOffset = -y;

                            //generate alpha
                            byte alpha = (byte) (200 - xOffset);
                            if (alpha > 255) alpha = 254;

                            //set colors
                            tmpBytes[4*(x + y*_bmpWidth)] = _bmpBytes[4*(x + xOffset + (y + yOffset)*_bmpWidth)];
                            tmpBytes[4*(x + y*_bmpWidth) + 1] =
                                _bmpBytes[4*(x + xOffset + (y + yOffset)*_bmpWidth) + 1];
                            tmpBytes[4*(x + y*_bmpWidth) + 2] =
                                _bmpBytes[4*(x + xOffset + (y + yOffset)*_bmpWidth) + 2];
                            tmpBytes[4*(x + y*_bmpWidth) + 3] = alpha;
                        }
                    }

                    //copy data back
                    Marshal.Copy(tmpBytes, 0, tmpData.Scan0, _bmpWidth*_bmpHeight*4);
                    tmp.UnlockBits(tmpData);
                }

                e.Graphics.DrawImage(tmp, 0, 0, ClientRectangle.Width, ClientRectangle.Height);
            }
        }

        /// <summary>
        ///     This is the method that actually does move the waves around and simulates the
        ///     behaviour of water.
        /// </summary>
        private void ProcessWaves()
        {
            int newBuffer = (_activeBuffer == 0) ? 1 : 0;
            bool wavesFound = false;

            for (int x = 1; x < _waveWidth - 1; x++)
            {
                for (int y = 1; y < _waveHeight - 1; y++)
                {
                    _waves[x, y, newBuffer] = (short) (
                                                          ((_waves[x - 1, y - 1, _activeBuffer] +
                                                            _waves[x, y - 1, _activeBuffer] +
                                                            _waves[x + 1, y - 1, _activeBuffer] +
                                                            _waves[x - 1, y, _activeBuffer] +
                                                            _waves[x + 1, y, _activeBuffer] +
                                                            _waves[x - 1, y + 1, _activeBuffer] +
                                                            _waves[x, y + 1, _activeBuffer] +
                                                            _waves[x + 1, y + 1, _activeBuffer]) >> 2) -
                                                          _waves[x, y, newBuffer]);

                    //damping
                    if (_waves[x, y, newBuffer] != 0)
                    {
                        _waves[x, y, newBuffer] -= (short) (_waves[x, y, newBuffer] >> 4);
                        wavesFound = true;
                    }
                }
            }

            _weHaveWaves = wavesFound;
            _activeBuffer = newBuffer;
        }


        /// <summary>
        ///     This function is used to start a wave by simulating a round drop
        /// </summary>
        /// <param name="x">x position of the drop</param>
        /// <param name="y">y position of the drop</param>
        /// <param name="height">Height position of the drop</param>
        private void PutDrop(int x, int y, short height)
        {
            _weHaveWaves = true;
            const int radius = 20;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    if (((x + i < 0) || (x + i >= _waveWidth - 1)) || ((y + j < 0) || (y + j >= _waveHeight - 1)))
                        continue;

                    double dist = Math.Sqrt(i*i + j*j);
                    if (dist < radius)
                        _waves[x + i, y + j, _activeBuffer] = (short) (Math.Cos(dist*Math.PI/radius)*height);
                }
            }
        }

        /// <summary>
        ///     The MouseMove handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaterEffectControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var realX = (int) ((e.X/(double) ClientRectangle.Width)*_waveWidth);
                var realY = (int) ((e.Y/(double) ClientRectangle.Height)*_waveHeight);
                PutDrop(realX, realY, 200);
            }
        }

        #region Properties

        /// <summary>
        ///     Our background image
        /// </summary>
        public Bitmap ImageBitmap
        {
            get { return _bmp; }
            set
            {
                _bmp = value;
                _bmpHeight = _bmp.Height;
                _bmpWidth = _bmp.Width;

                _waveWidth = _bmpWidth >> ResolutionScale;
                _waveHeight = _bmpHeight >> ResolutionScale;
                _waves = new Int16[_waveWidth,_waveHeight,2];

                _bmpBytes = new Byte[_bmpWidth*_bmpHeight*4];
                _bmpBitmapData = _bmp.LockBits(new Rectangle(0, 0, _bmpWidth, _bmpHeight), ImageLockMode.ReadWrite,
                                               PixelFormat.Format32bppArgb);
                Marshal.Copy(_bmpBitmapData.Scan0, _bmpBytes, 0, _bmpWidth*_bmpHeight*4);
            }
        }

        /// <summary>
        ///     The scale of the wave matrix compared to the size of the image.
        ///     Use it for large images to reduce processor load.
        ///     0 : wave resolution is the same than image resolution
        ///     1 : wave resolution is half the image resolution
        ///     ...and so on
        /// </summary>
        public int ResolutionScale { get; set; }

        #endregion
    }
}