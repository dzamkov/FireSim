using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

/// <summary>
/// Main game window.
/// </summary>
public class Window : Form
{
    public Window()
    {
        this._World = World.Load(1.0, "Map.png");

        this.Text = "Fire Sim";
        this.BackColor = System.Drawing.Color.Black;
        this.Size = new Size(this._World.Width * 3, this._World.Height * 3);

        this._View = new View();
        // this._View.Anchor = AnchorStyles.None;
        this._View.Dock = DockStyle.Fill;
        this.Controls.Add(this._View);

        this._View.MouseClick += delegate(object sender, MouseEventArgs e)
        {
            int i = e.X * this._World.Width / this._View.Width;
            int j = e.Y * this._World.Height / this._View.Height;
            if (e.Button == MouseButtons.Left)
                this._World.IgniteSplotch(i, j, 10.0, 1000.0);
            else
                this._World.SplashSplotch(i, j, 10.0, 1.0);
        };

        this._Initialize();
    }

    /// <summary>
    /// Updates this window by the given amount of time in seconds.
    /// </summary>
    public void Update(double Time)
    {
        this._World.Update(Math.Min(0.5, Time));
        this._View.Refresh();
    }

    /// <summary>
    /// Initializes/resets the view for this window.
    /// </summary>
    private void _Initialize()
    {
        Size clientSize = this.ClientSize;
        if (this._View != null)
            this._View.Initialize(this._World,
                Math.Max(1, clientSize.Width / this._World.Width),
                Math.Max(1, clientSize.Height / this._World.Height));
    }

    protected override void OnResize(EventArgs e)
    {
        this._Initialize();
    }

    private World _World;
    private View _View;
}