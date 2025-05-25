// Value.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public enum LangType { Unknown, Void, Integer, Float, Boolean, Point, Array, Matrix, FunctionRef }

public class Value : IEquatable<Value>
{
    public static readonly Value VoidInstance = new Value(null, LangType.Void);
    public static readonly Value TrueInstance = new Value(1.0f, LangType.Boolean);
    public static readonly Value FalseInstance = new Value(0.0f, LangType.Boolean);

    public object RawValue { get; }
    public LangType Type { get; }
    public LangType ElementType { get; } 

    public Value(object rawValue, LangType type, LangType elementType = LangType.Unknown)
    {
        RawValue = rawValue;
        Type = type;
        ElementType = elementType;
    }

    public float AsFloat()
    {
        if (RawValue == null && Type != LangType.Void) throw new NullReferenceException("Cannot convert null to Float.");
        if (Type == LangType.Float) return (float)RawValue;
        if (Type == LangType.Integer) return (int)RawValue;
        if (Type == LangType.Boolean) return (float)RawValue; // 1.0f or 0.0f
        throw new InvalidCastException($"Cannot convert '{RawValue}' (Type: {Type}) to Float.");
    }

    public int AsInt()
    {
        if (RawValue == null && Type != LangType.Void) throw new NullReferenceException("Cannot convert null to Int.");
        if (Type == LangType.Integer) return (int)RawValue;
        if (Type == LangType.Float) return (int)(float)RawValue; // Втрата точності
        if (Type == LangType.Boolean) return (float)RawValue == 0.0f ? 0 : 1;
        throw new InvalidCastException($"Cannot convert '{RawValue}' (Type: {Type}) to Int.");
    }

    public bool AsBool()
    {
        if (Type == LangType.Boolean) return (float)RawValue != 0.0f; // Використовуємо порівняння з float
        if (Type == LangType.Float) return (float)RawValue != 0.0f;
        if (Type == LangType.Integer) return (int)RawValue != 0;
        // Для Point, Array, Matrix - вважаємо істинним, якщо не null (об'єкт існує)
        if (Type == LangType.Point || Type == LangType.Array || Type == LangType.Matrix) return RawValue != null;
        if (Type == LangType.Void) return false; // Void завжди false
        // Для FunctionRef - можна вважати true, якщо RawValue не null
        if (Type == LangType.FunctionRef) return RawValue != null;

        // Загальна істинність для невідомих або інших типів (якщо з'являться)
        return RawValue != null && !string.IsNullOrEmpty(RawValue.ToString());
    }

    public PointData AsPoint()
    {
        if (Type == LangType.Point && RawValue is PointData pd) return pd;
        throw new InvalidCastException($"Cannot convert '{RawValue}' (Type: {Type}) to PointData.");
    }

    public List<Value> AsArray()
    {
        if (Type == LangType.Array && RawValue is List<Value> list) return list;
        throw new InvalidCastException($"Cannot convert '{RawValue}' (Type: {Type}) to Array (List<Value>).");
    }
    public MatrixData AsMatrix()
    {
        if (Type == LangType.Matrix && RawValue is MatrixData md) return md;
        throw new InvalidCastException($"Cannot convert '{RawValue}' (Type: {Type}) to MatrixData.");
    }

    public override string ToString()
    {
        if (Type == LangType.Void) return "void";
        if (Type == LangType.Boolean) return (float)RawValue != 0.0f ? "true" : "false"; // Представлення Boolean
        if (Type == LangType.Array)
        {
            var arr = RawValue as List<Value>;
            return "[" + string.Join(", ", arr.Select(v => v.ToString())) + "]";
        }
        return RawValue?.ToString() ?? "null_rawValue";
    }

    public bool Equals(Value other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Type == LangType.Void && other.Type == LangType.Void) return true;
        if (Type == LangType.Void || other.Type == LangType.Void) return false; // Void не дорівнює нічому іншому

        if (Type != other.Type)
        {
            // Дозволити порівняння числових типів (Integer, Float, Boolean представлений як Float)
            bool thisIsNumericOrBool = Type == LangType.Integer || Type == LangType.Float || Type == LangType.Boolean;
            bool otherIsNumericOrBool = other.Type == LangType.Integer || other.Type == LangType.Float || other.Type == LangType.Boolean;

            if (thisIsNumericOrBool && otherIsNumericOrBool)
            {
                // Порівнюємо як float, оскільки Boolean представлений як float
                return AsFloat().Equals(other.AsFloat());
            }
            return false; // Різні нечислові/небулеві типи не рівні
        }

        // Типи однакові, порівнюємо RawValue
        if (RawValue == null && other.RawValue == null) return true;
        if (RawValue == null || other.RawValue == null) return false;

        switch (Type)
        {
            case LangType.Array:
                var arr1 = AsArray();
                var arr2 = other.AsArray();
                return arr1.SequenceEqual(arr2); // Порівнює елементи по черзі
            case LangType.Point:
                return AsPoint().Equals(other.AsPoint()); // Потрібно PointData.Equals
            case LangType.Matrix:
                return AsMatrix().Equals(other.AsMatrix()); // Потрібно MatrixData.Equals
            default:
                return RawValue.Equals(other.RawValue);
        }
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false; // Перевірка типу самого Value
        return Equals((Value)obj);
    }
    public override int GetHashCode()
    {
        if (Type == LangType.Void) return 0;
        if (Type == LangType.Integer || Type == LangType.Float || Type == LangType.Boolean)
        {
            return AsFloat().GetHashCode();
        }
        return HashCode.Combine(RawValue, Type);
    }

}