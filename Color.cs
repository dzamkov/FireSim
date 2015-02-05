using System;
using System.Collections.Generic;

/// <summary>
/// Represents a color.
/// </summary>
public struct Color
{
    public Color(double R, double G, double B)
    {
        this.R = R;
        this.G = G;
        this.B = B;
    }


    public static Color Ground = new Color(0.0, 0.0, 0.0);
    public static Color Grass = new Color(0.3, 0.7, 0.1);
    public static Color Tree = new Color(0.1, 0.6, 0.2);
    public static Color Coal = new Color(0.3, 0.3, 0.3);
    public static Color Ash = new Color(0.7, 0.7, 0.7);
    public static Color Hot = new Color(1.0, 0.2, 0.0);
    public static Color ReallyHot = new Color(1.0, 0.9, 0.6);
    public static Color Water = new Color(0.0, 0.0, 1.0);

    /// <summary>
    /// Gets the color for a cell.
    /// </summary>
    public static Color ForCell(double Scale, Cell Cell)
    {
        double mix = Scale * Scale * 0.2;
        Color col = Ground * mix;

        col += Water * Cell.Water; mix += Cell.Water;
        col += Grass * Cell.Grass * 3.0; mix += Cell.Grass * 3.0;
        col += Tree * Cell.Tree; mix += Cell.Tree;
        col += Coal * Cell.Coal; mix += Cell.Coal;
        col += Ash * Cell.Ash; mix += Cell.Ash;
        col *= (1.0 / mix);

        double temp = Cell.Temperature;
        if (temp > 600.0)
        {
            mix = 1.0;
            double hotMix = (temp - 600.0) / 600.0;
            col += Hot * hotMix; mix += hotMix;
            if (temp > 1400.0)
            {
                double reallyHotMix = (temp - 1400.0) / 400.0;
                col += ReallyHot * reallyHotMix; mix += reallyHotMix;
            }
            col *= (1.0 / mix);
        }

        return col;
    }

    /// <summary>
    /// Adds two colors.
    /// </summary>
    public static Color operator +(Color A, Color B)
    {
        return new Color(A.R + B.R, A.G + B.G, A.B + B.B);
    }

    /// <summary>
    /// Scales a color.
    /// </summary>
    public static Color operator *(Color A, double B)
    {
        return new Color(A.R * B, A.G * B, A.B * B);
    }

    /// <summary>
    /// Writes the color to the given memory location.
    /// </summary>
    public unsafe void Write(byte* Ptr)
    {
        byte r = (byte)(this.R * 255.0);
        byte g = (byte)(this.G * 255.0);
        byte b = (byte)(this.B * 255.0);
        Ptr[0] = b;
        Ptr[1] = g;
        Ptr[2] = r;
    }

    /// <summary>
    /// The red component of the color.
    /// </summary>
    public double R;

    /// <summary>
    /// The green component of the color.
    /// </summary>
    public double G;

    /// <summary>
    /// The blue component of the color.
    /// </summary>
    public double B;
}
