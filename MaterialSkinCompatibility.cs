using System;
using System.Drawing;
using System.Windows.Forms;

namespace MaterialSkin
{
    /// <summary>
    /// Compatibility layer for MaterialSkin functionality
    /// Provides basic MaterialSkin-like styling without external dependencies
    /// </summary>
    public class MaterialSkinManager
    {
        public static MaterialSkinManager Instance { get; } = new MaterialSkinManager();
        
        public Themes Theme { get; set; } = Themes.LIGHT;
        public ColorScheme ColorScheme { get; set; }
        
        public void AddFormToManage(Form form)
        {
            // Apply MaterialSkin styling to the form
            form.BackColor = Color.White;
            form.Font = new Font("Segoe UI", 9F);
        }
    }

    public class ColorScheme
    {
        public Color Primary { get; set; }
        public Color DarkPrimary { get; set; }
        public Color LightPrimary { get; set; }
        public Color Accent { get; set; }
        public TextShade TextShade { get; set; }

        public ColorScheme(Color primary, Color darkPrimary, Color lightPrimary, Color accent, TextShade textShade)
        {
            Primary = primary;
            DarkPrimary = darkPrimary;
            LightPrimary = lightPrimary;
            Accent = accent;
            TextShade = textShade;
        }
    }

    public enum Themes
    {
        LIGHT,
        DARK
    }

    public enum TextShade
    {
        WHITE,
        BLACK
    }

    public static class Primary
    {
        public static readonly Color BlueGrey800 = Color.FromArgb(69, 90, 120);
        public static readonly Color BlueGrey900 = Color.FromArgb(55, 71, 79);
        public static readonly Color BlueGrey500 = Color.FromArgb(96, 125, 139);
    }

    public static class Accent
    {
        public static readonly Color LightBlue200 = Color.FromArgb(129, 212, 250);
    }

    public enum MouseState
    {
        HOVER,
        DOWN,
        OUT
    }
}

namespace MaterialSkin.Controls
{
    /// <summary>
    /// MaterialButton compatibility class
    /// </summary>
    public class MaterialButton : Button
    {
        public new bool AutoSize { get; set; } = false;
        public new AutoSizeMode AutoSizeMode { get; set; } = AutoSizeMode.GrowAndShrink;
        public MaterialButtonDensity Density { get; set; } = MaterialButtonDensity.Default;
        public int Depth { get; set; } = 0;
        public bool HighEmphasis { get; set; } = true;
        public object Icon { get; set; } = null;
        public MouseState MouseState { get; set; } = MouseState.OUT;
        public Color NoAccentTextColor { get; set; } = Color.Empty;
        public MaterialButtonType Type { get; set; } = MaterialButtonType.Contained;
        public bool UseAccentColor { get; set; } = false;

        public MaterialButton()
        {
            // Apply Material Design styling
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.Cursor = Cursors.Hand;
            this.BackColor = Color.FromArgb(33, 150, 243); // Material Blue
            this.ForeColor = Color.White;
            this.FlatAppearance.MouseOverBackColor = Color.FromArgb(25, 118, 210);
            this.FlatAppearance.MouseDownBackColor = Color.FromArgb(25, 118, 210);
        }

        public enum MaterialButtonDensity
        {
            Default,
            Compact,
            Comfortable
        }

        public enum MaterialButtonType
        {
            Contained,
            Outlined,
            Text
        }
    }

    /// <summary>
    /// MaterialForm compatibility class
    /// </summary>
    public class MaterialForm : Form
    {
        public MaterialForm()
        {
            // Apply Material Design styling to the form
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
        }
    }
}
