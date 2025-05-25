using System;
using UnityEngine;
using System; // Для IEquatable
using System.Globalization; // Для ToString("F3")


public struct MatrixData : IEquatable<MatrixData>
{
    public float M11, M12, M21, M22, M31, M32; // M13, M23 (dx, dy) в OpenGL/DirectX термінології

    public MatrixData(float m11, float m12, float m21, float m22, float m31, float m32)
    {
        M11 = m11; M12 = m12;
        M21 = m21; M22 = m22;
        M31 = m31; M32 = m32; // Компоненти трансляції
    }
    public static MatrixData Identity => new MatrixData(1, 0, 0, 1, 0, 0);

    public PointData Transform(PointData p)
    {
        return new PointData(
            p.X * M11 + p.Y * M21 + M31,
            p.X * M12 + p.Y * M22 + M32
        );
    }
    public override string ToString() => $"Matrix[[{M11:F3},{M12:F3}],[{M21:F3},{M22:F3}],[{M31:F3},{M32:F3}]]";

    public bool Equals(MatrixData other) =>
        M11.Equals(other.M11) && M12.Equals(other.M12) &&
        M21.Equals(other.M21) && M22.Equals(other.M22) &&
        M31.Equals(other.M31) && M32.Equals(other.M32);

    public override bool Equals(object obj) => obj is MatrixData other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(M11, M12, M21, M22, M31, M32);

    public static bool operator ==(MatrixData left, MatrixData right) => left.Equals(right);
    public static bool operator !=(MatrixData left, MatrixData right) => !(left == right);
    public static MatrixData operator *(MatrixData a, MatrixData b)
    {
        return new MatrixData(
            a.M11 * b.M11 + a.M12 * b.M21,
            a.M11 * b.M12 + a.M12 * b.M22,

            a.M21 * b.M11 + a.M22 * b.M21,
            a.M21 * b.M12 + a.M22 * b.M22,

            a.M31 * b.M11 + a.M32 * b.M21 + b.M31,
            a.M31 * b.M12 + a.M32 * b.M22 + b.M32
        );
    }

}

