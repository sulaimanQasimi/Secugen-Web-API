using System.Drawing;
using System.Drawing.Imaging;

namespace sgdm.Properties
{
    /// <summary>
    /// Simple fingerprint icon generator
    /// </summary>
    public static class Resources
    {
        public static Bitmap fingerprint_icon
        {
            get
            {
                return CreateFingerprintIcon();
            }
        }

        private static Bitmap CreateFingerprintIcon()
        {
            Bitmap icon = new Bitmap(40, 40);
            using (Graphics g = Graphics.FromImage(icon))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // Dark background for military theme
                g.FillRectangle(new SolidBrush(Color.FromArgb(25, 25, 35)), 0, 0, 40, 40);
                
                // Shield outline
                using (Pen shieldPen = new Pen(Color.FromArgb(255, 193, 7), 3))
                {
                    // Shield shape
                    Point[] shieldPoints = {
                        new Point(20, 5),
                        new Point(30, 12),
                        new Point(30, 25),
                        new Point(20, 35),
                        new Point(10, 25),
                        new Point(10, 12)
                    };
                    g.DrawPolygon(shieldPen, shieldPoints);
                }
                
                // Fingerprint pattern inside shield
                using (Pen fingerprintPen = new Pen(Color.FromArgb(255, 193, 7), 1.5f))
                {
                    // Fingerprint ridges
                    g.DrawEllipse(fingerprintPen, 12, 10, 16, 16);
                    g.DrawEllipse(fingerprintPen, 14, 12, 12, 12);
                    g.DrawEllipse(fingerprintPen, 16, 14, 8, 8);
                    
                    // Ridge lines
                    for (int i = 0; i < 3; i++)
                    {
                        g.DrawArc(fingerprintPen, 13 + i * 2, 11 + i * 2, 14 - i * 4, 14 - i * 4, 0, 180);
                    }
                }
            }
            return icon;
        }
    }
}
