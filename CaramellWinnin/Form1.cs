using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace CaramellWinnin
{
    public partial class Form1 : Form
    {
        Image image = Image.FromFile("Background.png");
        private int color = 0;

        private bool AutoLoadMusic = true;
        private string MusicLink = "https://www.youtube.com/watch?v=V-KSyjmhwE0";
        private bool ImageBased = false;
        private int ColorStep = 32;
        private int TimerInterval = 4;
        private int index = 0;
        private double OpacityPercent = 5;
        private Settings settings;

        private int oldWindowLong;
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReadSettings();

            AllowTransparency = true;
            if (AutoLoadMusic)
            {
                Process.Start(MusicLink);
            }

            SetFormTransparent(this.Handle);
            KeyDown += Form1_KeyDown;
        }

        private void ReadSettings()
        {
            if (File.Exists("settings.json"))
            {
                string settingsFromFile = File.ReadAllText("settings.json");
                settings = JsonConvert.DeserializeObject<Settings>(settingsFromFile);
            }
            else
            {
                settings = new Settings(AutoLoadMusic, MusicLink, ImageBased, ColorStep, TimerInterval, OpacityPercent);
                string settingsJson = JsonConvert.SerializeObject(settings);
                File.WriteAllText("settings.json", settingsJson);
            }

            AutoLoadMusic = settings.AutoLoadMusic;
            MusicLink = settings.MusicLink;
            ImageBased = settings.UseImage;
            ColorStep = settings.ColorStep;
            TimerInterval = settings.TimerInterval;
            OpacityPercent = settings.OpacityPercent;

            Opacity = OpacityPercent * 0.01;
            timer1.Interval = TimerInterval;
            timer1.Enabled = true;
        }

        public void SetFormTransparent(IntPtr Handle)
        {
            oldWindowLong = GetWindowLong(Handle, -20);
            SetWindowLong(Handle, -20, Convert.ToInt32(oldWindowLong | 0x00080000 | 0x00000020L));
        }

        public void SetFormNormal(IntPtr Handle)
        {
            SetWindowLong(Handle, -20, Convert.ToInt32(oldWindowLong | 0x00080000));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            index++;
            if(index < TimerInterval)
            {
                return;
            }
            index = 0;

            if (color < 255)
            {
                color += ColorStep;
            }
            else
            {
                color = 0;
            }


            int r;
            int gr;
            int b;
            HsvToRgb(color, 1, 1, out r, out gr, out b);

            if (!ImageBased) {
                BackColor = Color.FromArgb(r, gr, b);
                return;
            }

            Graphics g = e.Graphics;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

            Bitmap newImage = new Bitmap(image, Width, Height);

            float[][] ptsArray =
            {
                new float[] { r, 0, 0, 0, 0 },
                new float[] { 0, gr, 0, 0, 0 },
                new float[] { 0, 0, b, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 0 }
            };

            ColorMatrix matrix = new ColorMatrix(ptsArray);
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Default);

            g.DrawImage(newImage, new Rectangle(new Point(0, 0), new Size(newImage.Width, newImage.Height)), 0, 0, newImage.Width, newImage.Height, GraphicsUnit.Pixel, attributes);

            newImage.Dispose();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Refresh();
        }

        void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            // ######################################################################
            // T. Nathan Mundhenk
            // mundhenk@usc.edu
            // C/C++ Macro HSV to RGB
            h = Math.Abs(h);

            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.F4)
            {
                Application.Exit();
            }
        }
    }
}
