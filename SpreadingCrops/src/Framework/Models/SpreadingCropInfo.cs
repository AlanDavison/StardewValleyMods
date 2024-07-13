using System;

namespace SpreadingCrops.Framework.Models;

public class SpreadingCropInfo
{
    // The order of these fields indicate the correct order in the CustomField.
    public double SpreadChance { get; init; }
    public bool SpreadToAllValidTiles { get; init; }

    public SpreadingCropInfo(string cropField)
    {
        string[] fields = cropField.Split("/");

        if (!double.TryParse(fields[0], out double spreadChance))
            throw new ArgumentException("Couldn't parse first value as a double.");

        this.SpreadChance = spreadChance;

        if (fields.Length >= 2)
        {
            if (!bool.TryParse(fields[1], out bool spreadToAllValidTiles))
                throw new ArgumentException(
                    "Couldn't parse third value as the bool for whether the crop should spread to all valid tiles.");

            this.SpreadToAllValidTiles = spreadToAllValidTiles;
        }
    }
}
