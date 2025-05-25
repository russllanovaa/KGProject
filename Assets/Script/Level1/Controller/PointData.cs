using UnityEngine;
using System; // Для IEquatable
using System.Globalization; // Для ToString("F3")

public struct PointData : IEquatable<PointData>
{
    public float X { get; set; } // Додано set для можливості зміни через pointAccess
    public float Y { get; set; }

    public PointData(float x, float y) { X = x; Y = y; }

    public override string ToString() => $"Point({X.ToString("F3", CultureInfo.InvariantCulture)}, {Y.ToString("F3", CultureInfo.InvariantCulture)})";

    public bool Equals(PointData other) => X.Equals(other.X) && Y.Equals(other.Y);

    public override bool Equals(object obj) => obj is PointData other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(PointData left, PointData right) => left.Equals(right);
    public static bool operator !=(PointData left, PointData right) => !(left == right);
}
