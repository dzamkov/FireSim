using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

/// <summary>
/// A control that displays a world.
/// </summary>
public class View : Control
{
    public View()
    {
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        this.SetStyle(ControlStyles.UserPaint, true);
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    }

    /// <summary>
    /// Gets the horizontal scale of this view (pixels/cell).
    /// </summary>
    public int XScale
    {
        get
        {
            return this._Buffer.Width / this._World.Width;
        }
    }

    /// <summary>
    /// Gets the vertical scale of this view (pixels/cell).
    /// </summary>
    public int YScale
    {
        get
        {
            return this._Buffer.Height / this._World.Height;
        }
    }

    /// <summary>
    /// Initializes/resets this view.
    /// </summary>
    public void Initialize(World World, int XScale, int YScale)
    {
        this._World = World;
        if (this._Buffer != null)
        {
            this._Buffer.Dispose();
        }
        this._Buffer = new Bitmap(this._World.Width * XScale, this._World.Height * YScale, PixelFormat.Format24bppRgb);
        this.Size = this._Buffer.Size;
        for (int i = 0; i < this._World.Width; i++)
        {
            for (int j = 0; j < this._World.Height; j++)
            {
                this._World.Cells[i, j].MatterError = double.PositiveInfinity;
            }
        }

        this._Grainy = new byte[XScale * YScale];
        for (int t = 0; t < this._Grainy.Length; t++)
        {
            this._Grainy[t] = (byte)Program.Random.Next(10);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Size drawSize = this._Buffer.Size;

        // Draw to bitmap
        unsafe
        {
            BitmapData bd = this._Buffer.LockBits(new Rectangle(new Point(0, 0), drawSize), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            byte* yptr = (byte*)bd.Scan0.ToPointer();

            int xScale = drawSize.Width / this._World.Width;
            int yScale = drawSize.Height / this._World.Height;
            for (int y = 0; y < this._World.Height; y++)
            {
                byte* xptr = yptr;
                for (int x = 0; x < this._World.Width; x++)
                {
                    Cell cell = this._World[x, y];
                    if (cell.MatterError >= 0.5 || cell.EnergyError >= 1.0e3)
                    {
                        Color col = Color.ForCell(this._World.Scale, cell);
                        cell.MatterError = 0.0;
                        cell.EnergyError = 0.0;

                        int g = 0;
                        byte* iptr = xptr;
                        for (int i = 0; i < yScale; i++)
                        {
                            byte* jptr = iptr;
                            for (int j = 0; j < xScale; j++)
                            {
                                col.Write(jptr);
                                int grain = Program.Random.Next((int)Math.Max(5.0, (cell.Temperature - 200.0) / 50.0));
                                jptr[0] = (byte)Math.Min(255, jptr[0] + grain);
                                jptr[1] = (byte)Math.Min(255, jptr[1] + grain);
                                jptr[2] = (byte)Math.Min(255, jptr[2] + grain);
                                jptr += 3;
                                g++;
                            }
                            iptr += bd.Stride;
                        }
                    }
                    xptr += 3 * xScale;
                }
                yptr += bd.Stride * yScale;
            }

            this._Buffer.UnlockBits(bd);
        }

        // Draw bitmap
        e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
        e.Graphics.DrawImage(this._Buffer, 0, 0, this.Width, this.Height);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        this._MouseButtons |= e.Button;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        this._MousePosition = new Point(
            e.X / this.XScale,
            e.Y / this.YScale);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        this._MouseButtons &= ~e.Button;
    }

    private byte[] _Grainy;
    private MouseButtons _MouseButtons;
    private Point _MousePosition;
    private World _World;
    private Bitmap _Buffer;
}