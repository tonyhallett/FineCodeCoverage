using System;

namespace ColourUtilities
{
    /*
        Conversions from https://www.codeproject.com/Articles/19045/Manipulating-colors-in-NET-Part-1#hsb
        This is potentially better https://github.com/tompazourek/Colourful/
 
    */
    public struct RGB
    {
        /// <summary>
        /// Gets an empty RGB structure;
        /// </summary>
        public static readonly RGB Empty = new RGB();

        private int red;
        private int green;
        private int blue;

        public static bool operator ==(RGB item1, RGB item2) => item1.Red == item2.Red
                && item1.Green == item2.Green
                && item1.Blue == item2.Blue
                ;

        public static bool operator !=(RGB item1, RGB item2) => item1.Red != item2.Red
                || item1.Green != item2.Green
                || item1.Blue != item2.Blue
                ;

        /// <summary>
        /// Gets or sets red value.
        /// </summary>
        public int Red
        {
            get => this.red;
            set => this.red = (value > 255) ? 255 : ((value < 0) ? 0 : value);
        }

        /// <summary>
        /// Gets or sets red value.
        /// </summary>
        public int Green
        {
            get => this.green;
            set => this.green = (value > 255) ? 255 : ((value < 0) ? 0 : value);
        }

        /// <summary>
        /// Gets or sets red value.
        /// </summary>
        public int Blue
        {
            get => this.blue;
            set => this.blue = (value > 255) ? 255 : ((value < 0) ? 0 : value);
        }

        public RGB(int R, int G, int B)
        {
            this.red = (R > 255) ? 255 : ((R < 0) ? 0 : R);
            this.green = (G > 255) ? 255 : ((G < 0) ? 0 : G);
            this.blue = (B > 255) ? 255 : ((B < 0) ? 0 : B);
        }

        public override bool Equals(object obj) => obj != null && this.GetType() == obj.GetType() && this == (RGB)obj;

        public override int GetHashCode() => this.Red.GetHashCode() ^ this.Green.GetHashCode() ^ this.Blue.GetHashCode();
    }
    public static class ColorConversion
    {
        /// <summary>
        /// Converts HSL to RGB.
        /// </summary>
        /// <param name="h">Hue, must be in [0, 360].</param>
        /// <param name="s">Saturation, must be in [0, 1].</param>
        /// <param name="l">Luminance, must be in [0, 1].</param>
        public static RGB HSLtoRGB(double h, double s, double l)
        {
            if (s == 0)
            {
                // achromatic color (gray scale)
                return new RGB(
                    Convert.ToInt32(double.Parse(string.Format("{0:0.00}",
                        l * 255.0))),
                    Convert.ToInt32(double.Parse(string.Format("{0:0.00}",
                        l * 255.0))),
                    Convert.ToInt32(double.Parse(string.Format("{0:0.00}",
                        l * 255.0)))
                    );
            }
            else
            {
                double q = (l < 0.5) ? (l * (1.0 + s)) : (l + s - (l * s));
                double p = (2.0 * l) - q;

                double Hk = h / 360.0;
                double[] T = new double[3];
                T[0] = Hk + (1.0 / 3.0);    // Tr
                T[1] = Hk;                // Tb
                T[2] = Hk - (1.0 / 3.0);    // Tg

                for (int i = 0; i < 3; i++)
                {
                    if (T[i] < 0) T[i] += 1.0;
                    if (T[i] > 1) T[i] -= 1.0;

                    if ((T[i] * 6) < 1)
                    {
                        T[i] = p + ((q - p) * 6.0 * T[i]);
                    }
                    else
                    {
                        T[i] = (T[i] * 2.0) < 1 ? q : (T[i] * 3.0) < 2 ? p + ((q - p) * ((2.0 / 3.0) - T[i]) * 6.0) : p;
                    }
                }

                return new RGB(
                    Convert.ToInt32(double.Parse(string.Format("{0:0.00}",
                        T[0] * 255.0))),
                    Convert.ToInt32(double.Parse(string.Format("{0:0.00}",
                        T[1] * 255.0))),
                    Convert.ToInt32(double.Parse(string.Format("{0:0.00}",
                        T[2] * 255.0)))
                    );
            }
        }

        public static HSL RGBtoHSL(int red, int green, int blue)
        {
            double h = 0, s = 0;

            // normalize red, green, blue values
            double r = red / 255.0;
            double g = green / 255.0;
            double b = blue / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            // hue
            if (max == min)
            {
                h = 0; // undefined
            }
            else if (max == r && g >= b)
            {
                h = 60.0 * (g - b) / (max - min);
            }
            else if (max == r && g < b)
            {
                h = (60.0 * (g - b) / (max - min)) + 360.0;
            }
            else if (max == g)
            {
                h = (60.0 * (b - r) / (max - min)) + 120.0;
            }
            else if (max == b)
            {
                h = (60.0 * (r - g) / (max - min)) + 240.0;
            }

            // luminance
            double l = (max + min) / 2.0;

            // saturation
            if (l == 0 || max == min)
            {
                s = 0;
            }
            else if (0 < l && l <= 0.5)
            {
                s = (max - min) / (max + min);
            }
            else if (l > 0.5)
            {
                s = (max - min) / (2 - (max + min)); //(max-min > 0)?
            }

            return new HSL(
                double.Parse(string.Format("{0:0.##}", h)),
                double.Parse(string.Format("{0:0.##}", s)),
                double.Parse(string.Format("{0:0.##}", l))
                );
        }
    }

    public struct HSL
    {
        /// <summary>
        /// Gets an empty HSL structure;
        /// </summary>
        public static readonly HSL Empty = new HSL();

        private double hue;
        private double saturation;
        private double luminance;

        public static bool operator ==(HSL item1, HSL item2) => item1.Hue == item2.Hue
                && item1.Saturation == item2.Saturation
                && item1.Luminance == item2.Luminance
                ;

        public static bool operator !=(HSL item1, HSL item2) => item1.Hue != item2.Hue
                || item1.Saturation != item2.Saturation
                || item1.Luminance != item2.Luminance
                ;

        /// <summary>
        /// Gets or sets the hue component.
        /// </summary>
        public double Hue
        {
            get => this.hue;
            set => this.hue = (value > 360) ? 360 : ((value < 0) ? 0 : value);
        }

        /// <summary>
        /// Gets or sets saturation component.
        /// </summary>
        public double Saturation
        {
            get => this.saturation;
            set => this.saturation = (value > 1) ? 1 : ((value < 0) ? 0 : value);
        }

        /// <summary>
        /// Gets or sets the luminance component.
        /// </summary>
        public double Luminance
        {
            get => this.luminance;
            set => this.luminance = (value > 1) ? 1 : ((value < 0) ? 0 : value);
        }

        /// <summary>
        /// Creates an instance of a HSL structure.
        /// </summary>
        /// <param name="h">Hue value.</param>
        /// <param name="s">Saturation value.</param>
        /// <param name="l">Lightness value.</param>
        public HSL(double h, double s, double l)
        {
            this.hue = (h > 360) ? 360 : ((h < 0) ? 0 : h);
            this.saturation = (s > 1) ? 1 : ((s < 0) ? 0 : s);
            this.luminance = (l > 1) ? 1 : ((l < 0) ? 0 : l);
        }

        public override bool Equals(object obj) => obj != null && this.GetType() == obj.GetType() && this == (HSL)obj;

        public override int GetHashCode() => this.Hue.GetHashCode() ^ this.Saturation.GetHashCode() ^
                this.Luminance.GetHashCode();
    }
    public static class LightenssApplier
    {
        public static System.Drawing.Color Swap(System.Drawing.Color lightnessColor, System.Drawing.Color applyToColor)
        {
            HSL hsl = ColorConversion.RGBtoHSL(lightnessColor.R, lightnessColor.G, lightnessColor.B);
            HSL hsl2 = ColorConversion.RGBtoHSL(applyToColor.R, applyToColor.G, applyToColor.B);
            hsl2.Luminance = hsl.Luminance;
            RGB rgb = ColorConversion.HSLtoRGB(hsl2.Hue, hsl2.Saturation, hsl2.Luminance);
            return System.Drawing.Color.FromArgb(rgb.Red, rgb.Green, rgb.Blue);
        }
    }
}
