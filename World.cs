using System;
using System.Collections.Generic;
using System.Drawing;

/// <summary>
/// A cell in the world.
/// </summary>
public class Cell
{
    /// <summary>
    /// The total thermal energy in this cell (J).
    /// </summary>
    public double Energy;

    /// <summary>
    /// The specific heat of this cell (J/K).
    /// </summary>
    public double SpecificHeat;

    /// <summary>
    /// Gets or sets temperature of this cell (kelvin).
    /// </summary>
    public double Temperature
    {
        get
        {
            return this.Energy / this.SpecificHeat;
        }
        set
        {
            this.Energy = value * this.SpecificHeat;
        }
    }

    /// <summary>
    /// The amount of water in this cell (Kg).
    /// </summary>
    public double Water;

    /// <summary>
    /// The specific heat of water (J/KgK).
    /// </summary>
    public const double WaterSpecificHeat = 4180;

    /// <summary>
    /// The enthalpy of vaporization for water (J/Kg).
    /// </summary>
    public const double WaterEnthalpyOfVaporization = 2.26e6;

    /// <summary>
    /// The amount of grass in this cell (Kg).
    /// </summary>
    public double Grass;

    /// <summary>
    /// The burn rate of grass (exposed area).
    /// </summary>
    public const double GrassRate = 2.5;

    /// <summary>
    /// The energy content of grass (J/KgK).
    /// </summary>
    public const double GrassEnergy = 1.70e7;

    /// <summary>
    /// The specific heat of grass (J/KgK).
    /// </summary>
    public const double GrassSpecificHeat = 300;

    /// <summary>
    /// The amount of tree in this cell (Kg).
    /// </summary>
    public double Tree;

    /// <summary>
    /// The burn rate of tree (exposed area).
    /// </summary>
    public const double TreeRate = 0.05;

    /// <summary>
    /// The energy content of tree (J/KgK).
    /// </summary>
    public const double TreeEnergy = 1.62e7;

    /// <summary>
    /// The specific heat of tree (J/KgK).
    /// </summary>
    public const double TreeSpecificHeat = 200;

    /// <summary>
    /// The amount of coal in this cell (Kg).
    /// </summary>
    public double Coal;

    /// <summary>
    /// The burn rate of coal (exposed area).
    /// </summary>
    public const double CoalRate = 0.02;

    /// <summary>
    /// The energy content of coal (J/KgK).
    /// </summary>
    public const double CoalEnergy = 2.40e7;

    /// <summary>
    /// The specific heat of coal (J/KgK).
    /// </summary>
    public const double CoalSpecificHeat = 200;
        
    /// <summary>
    /// The amount of ash in this cell (Kg).
    /// </summary>
    public double Ash;

    /// <summary>
    /// The specific heat of ash (J/KgK).
    /// </summary>
    public const double AshSpecificHeat = 350;

    /// <summary>
    /// The maximum combustion rate for a burning cell (W/m^2).
    /// </summary>
    public const double BurnCapacity = 1.0e7;

    /// <summary>
    /// Independently updates this cell by the given amount of time in seconds.
    /// </summary>
    public void Update(double Area, double Time)
    {
        double temp = this.Temperature;

        // Evaporation
        if (temp > 373.0 && this.Water > 0.0)
        {
            double extraEnergy = this.Energy - 373.0 * this.SpecificHeat;
            double efficiency = Math.Pow(0.5, Time);

            this.Energy -= extraEnergy * efficiency;
            this.InvalidateEnergy(extraEnergy * efficiency);

            double vaporized = extraEnergy * efficiency / WaterEnthalpyOfVaporization;
            this.Water = Math.Max(0.0, this.Water - vaporized);
            this.InvalidateMatter(vaporized);
        }

        // Pryolysis
        if (temp > 700.0 && this.Tree > 0.0)
        {
            double rate = Math.Pow((temp - 700.0) / 200.0, 0.3) * this.Tree;
            double mass = rate * Time;

            this.Tree = Math.Max(0.0, this.Tree - mass);
            this.Coal += mass;
            this.InvalidateMatter(mass * 2.0);
        }

        // Burning
        if (temp > 600.0)
        {
            double burnRate = Math.Pow((temp - 600.0) / 200.0, 0.3);
            double grassRate = this.Grass * burnRate * GrassRate;
            double treeRate = this.Tree * burnRate * TreeRate;
            double coalRate = this.Coal * burnRate * CoalRate;
            double grassEnergyRate = grassRate * GrassEnergy;
            double treeEnergyRate = treeRate * TreeEnergy;
            double coalEnergyRate = coalRate * CoalEnergy;
            double totalEnergyRate = grassEnergyRate + treeEnergyRate + coalEnergyRate;

            double burnCapacity = BurnCapacity * Area;
            double efficiency = burnCapacity / (totalEnergyRate + burnCapacity);

            this.Energy += totalEnergyRate * Time * efficiency;
            this.Grass = Math.Max(0.0, this.Grass - grassRate * Time * efficiency);
            this.Tree = Math.Max(0.0, this.Tree - treeRate * Time * efficiency);
            this.Coal = Math.Max(0.0, this.Coal - coalRate * Time * efficiency);
            this.Ash += (grassRate + treeRate + coalRate) * Time * efficiency;

            this.InvalidateMatter((grassRate + treeRate + coalRate) * Time * efficiency * 2.0);
            this.InvalidateEnergy(totalEnergyRate * Time * efficiency);
        }
    }

    /// <summary>
    /// Adds or subtracts energy to this cell.
    /// </summary>
    public void Exchange(double Energy)
    {
        this.Energy = Math.Max(0.0, this.Energy + Energy);
        this.InvalidateEnergy(Energy);
    }

    /// <summary>
    /// Invalidates this cell in response to a change in composition.
    /// </summary>
    public void InvalidateMatter(double Change)
    {
        this.SpecificHeat =
            this.Water * WaterSpecificHeat +
            this.Grass * GrassSpecificHeat +
            this.Tree * TreeSpecificHeat +
            this.Coal * CoalSpecificHeat +
            this.Ash * AshSpecificHeat;
        this.MatterError += Change;
    }

    /// <summary>
    /// Invalidates this cell in response to a change in energy.
    /// </summary>
    public void InvalidateEnergy(double Change)
    {
        this.EnergyError += Math.Abs(Change);
    }

    /// <summary>
    /// The change in matter that has occured in this cell since the last time this field was reset (Kg).
    /// </summary>
    public double MatterError;

    /// <summary>
    /// The change in energy that has occured in this cell since the last time this field was reset (J).
    /// </summary>
    public double EnergyError;
}

/// <summary>
/// A world full of burning trees and grass.
/// </summary>
public class World
{
    public World(double Scale, int Width, int Height)
    {
        this.Scale = Scale;
        this.Cells = new Cell[Width, Height];
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                this.Cells[i, j] = new Cell();
            }
        }
    }

    /// <summary>
    /// Loads a world from a file.
    /// </summary>
    public static World Load(double Scale, string File)
    {
        Bitmap bmp = new Bitmap(File);
        World world = new ToridalWorld(Scale, bmp.Width, bmp.Height);
        Random r = new Random();
        double cellArea = Scale * Scale;
        for (int i = 0; i < world.Width; i++)
        {
            for (int j = 0; j < world.Height; j++)
            {
                System.Drawing.Color col = bmp.GetPixel(i, j);
                double grass = col.G / 256.0 * cellArea * 0.1 * (3.0 + r.NextDouble()) / 3.0;
                double tree = col.B / 256.0 * cellArea * 4.0 * (3.0 + r.NextDouble()) / 3.0;
                Cell cell = world[i, j];
                cell.Grass = grass;
                cell.Tree = tree;
                cell.Water = tree * 0.1 + grass * 0.1;
                cell.InvalidateMatter(double.PositiveInfinity);
            }
        }
        return world;
    }

    /// <summary>
    /// The cell data for this world.
    /// </summary>
    public readonly Cell[,] Cells;

    /// <summary>
    /// Gets the width of this world.
    /// </summary>
    public int Width
    {
        get
        {
            return this.Cells.GetLength(0);
        }
    }

    /// <summary>
    /// Gets the height of this world.
    /// </summary>
    public int Height
    {
        get
        {
            return this.Cells.GetLength(1);
        }
    }

    /// <summary>
    /// Gets the world cell with the given index, or null if that cell does not exist.
    /// </summary>
    public virtual Cell this[int I, int J]
    {
        get
        {
            int width = this.Width;
            int height = this.Height;
            if (I >= 0 && I < width && J >= 0 && J < height)
                return this.Cells[I, J];
            else
                return null;
        }
    }

    /// <summary>
    /// The length of a cell in this world (m).
    /// </summary>
    public readonly double Scale;

    /// <summary>
    /// The ambient temperature for this world.
    /// </summary>
    public double AmbientTemperature;

    /// <summary>
    /// Sets the temperature of this world.
    /// </summary>
    public double Temperature
    {
        set
        {
            for (int i = 0; i < this.Width; i++)
            {
                for (int j = 0; j < this.Height; j++)
                {
                    Cell cell = this[i, j];
                    cell.Temperature = value;
                    cell.InvalidateEnergy(double.PositiveInfinity);
                }
            }
            this.AmbientTemperature = value;
        }
    }

    /// <summary>
    /// Gets the mass of living matter left in this world.
    /// </summary>
    public double Score
    {
        get
        {
            double score = 0.0;
            for (int i = 0; i < this.Width; i++)
            {
                for (int j = 0; j < this.Height; j++)
                {
                    Cell cell = this[i, j];
                    score += cell.Grass;
                    score += cell.Tree;
                }
            }
            return score;
        }
    }

    /// <summary>
    /// Alters all cells in a circular area surronding the given cell using the given operation.
    /// </summary>
    public void Splotch(int I, int J, double Radius, Action<double, Cell> Action)
    {
        if (Action != null)
        {
            double crad = Radius / this.Scale;
            for (int i = (int)Math.Floor(-crad); i <= (int)Math.Ceiling(crad); i++)
            {
                for (int j = (int)Math.Floor(-crad); j <= (int)Math.Ceiling(crad); j++)
                {
                    double dis = Math.Sqrt(i * i + j * j);
                    if (dis < Radius)
                    {
                        double s = 1.0 - dis / Radius;
                        Cell cell = this[I + i, J + j];
                        if (cell != null)
                        {
                            Action(s, cell);
                            cell.InvalidateMatter(double.PositiveInfinity);
                            cell.InvalidateEnergy(double.PositiveInfinity);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Increases the temperature in the given splotch region.
    /// </summary>
    public void IgniteSplotch(int I, int J, double Radius, double Temperature)
    {
        this.Splotch(I, J, Radius, delegate(double s, Cell cell)
        {
            cell.Temperature += s * Temperature;
        });
    }

    /// <summary>
    /// Adds the given amount of water (Kg) over the given splotch region.
    /// </summary>
    public void SplashSplotch(int I, int J, double Radius, double Amount)
    {
        Amount /= Math.PI * this.Scale * this.Scale;
        this.Splotch(I, J, Radius, delegate(double s, Cell cell)
        {
            double m = s * Amount;
            cell.Water += m;
            cell.Energy += this.AmbientTemperature * Cell.WaterSpecificHeat * m;
        });
    }

    /// <summary>
    /// The length of a cell (m).
    /// </summary>
    public const double Length = 1.0;

    /// <summary>
    /// The Stefan-Boltzmann constant (W/m^2K^4).
    /// </summary>
    public const double StefanBoltzmann = 5.67E-8;

    /// <summary>
    /// The convective heat transfer that occurs between a cell and the air above (W/Km^2).
    /// </summary>
    public const double VerticalHeatTransfer = 5.0e2;

    /// <summary>
    /// The convective heat transfer that occurs between two adjacent cells (W/K).
    /// </summary>
    public const double HorizontalHeatTransfer = 5.0e2;

    /// <summary>
    /// Updates this world by the given amount of time in seconds.
    /// </summary>
    public void Update(double Time)
    {
        double cellArea = this.Scale * this.Scale;
        for (int i = 0; i < this.Width; i++)
        {
            for (int j = 0; j < this.Height; j++)
            {
                Cell cell = this.Cells[i, j];
                cell.Update(cellArea, Time);

                // Convection.
                cell.Exchange((this.AmbientTemperature - cell.Temperature) * cellArea * VerticalHeatTransfer * Time);

                // Outflow radiation.
                double temperature = cell.Temperature;
                double k = StefanBoltzmann * Length * Length / cell.SpecificHeat;
                double nTemperature = Math.Pow(3.0 * k * Time + Math.Pow(temperature, -3.0), -1.0 / 3.0);
                cell.Temperature = nTemperature;
                cell.InvalidateEnergy((temperature - nTemperature) * cell.SpecificHeat);
            }
        }

        this.Exchange(Time);
    }

    /// <summary>
    /// Performs mutual exchange between all cells in this world.
    /// </summary>
    protected virtual void Exchange(double Time)
    {
        for (int i = 0; i < this.Width - 1; i++)
        {
            int ni = i + 1;
            int pi = i - 1;
            for (int j = 0; j < this.Height - 1; j++)
            {
                int nj = j + 1;
                Exchange(Time, this.Cells[i, j], this.Cells[ni, j]);
                Exchange(Time, this.Cells[i, j], this.Cells[ni, nj]);
                Exchange(Time, this.Cells[i, j], this.Cells[i, nj]);
                if (i > 0) Exchange(Time, this.Cells[i, j], this.Cells[pi, nj]);
            }
        }
    }

    /// <summary>
    /// Performs a mutual exchange between the given cells.
    /// </summary>
    public static void Exchange(double Time, Cell A, Cell B)
    {
        double dif = A.Temperature - B.Temperature;
        if (Math.Abs(dif) > 0.001)
        {
            double transferRate = dif * HorizontalHeatTransfer * Time;
            A.Exchange(-transferRate);
            B.Exchange(transferRate);
        }
    }
}

/// <summary>
/// A world with a toridal geometry (with the edges wrapping around).
/// </summary>
public class ToridalWorld : World
{
    public ToridalWorld(double Scale, int Width, int Height)
        : base(Scale, Width, Height)
    {

    }

    public override Cell this[int I, int J]
    {
        get
        {
            int width = this.Width;
            int height = this.Height;
            return this.Cells[(I + width) % width, (J + height) % height];
        }
    }

    protected override void Exchange(double Time)
    {
        for (int i = 0; i < this.Width; i++)
        {
            int ni = (i + 1) % this.Width;
            int pi = (i + this.Width - 1) % this.Width;
            for (int j = 0; j < this.Height; j++)
            {
                int nj = (j + 1) % this.Height;
                Exchange(Time, this.Cells[i, j], this.Cells[ni, j]);
                Exchange(Time, this.Cells[i, j], this.Cells[ni, nj]);
                Exchange(Time, this.Cells[i, j], this.Cells[i, nj]);
                Exchange(Time, this.Cells[i, j], this.Cells[pi, nj]);
            }
        }
    }
}