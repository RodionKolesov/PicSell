using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace CourseWork
{
    public class Pixel
    {
 
        public Point Point { get; set; }
        public Color Color { get; set; }
        public Pixel() { }
        
        public Pixel(Pixel originalPixel)
        {
            Point = originalPixel.Point;
            Color = originalPixel.Color;
        }

        public void SetHSL(float h, float s, float l)
        {
            float C = (1 - Math.Abs(2 * l - 1)) * s;
            float X = C * (1 - Math.Abs(h / 60 % 2 - 1));
            float m = l - C / 2;
            float _r, _g, _b;
            if (h >= 0 && h < 60)
            {
                _r = C;
                _g = X;
                _b = 0;
            }
            else if (h >= 60 && h < 120)
            {
                _r = X;
                _g = C;
                _b = 0;
            }
            else if (h >= 120 && h < 180)
            {
                _r = 0;
                _g = C;
                _b = X;
            }
            else if (h >= 180 && h < 240)
            {
                _r = 0;
                _g = X;
                _b = C;
            }
            else if (h >= 240 && h < 300)
            {
                _r = X;
                _g = 0;
                _b = C;
            }
            else if (h >= 300 && h < 360)
            {
                _r = C;
                _g = 0;
                _b = X;
            }
            else return;

            int R = Convert.ToInt32((_r + m) * 255);
            int G = Convert.ToInt32((_g + m) * 255);
            int B = Convert.ToInt32((_b + m) * 255);

            this.Color = Color.FromArgb(R, G, B);
        }
    }
}
