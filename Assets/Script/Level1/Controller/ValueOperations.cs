// ValueOperations.cs
using System;
using System.Collections.Generic; // Для List<Value> в Add
using System.Globalization;

public static class ValueOperations
{
    private static bool IsNumericOrBool(Value val) => val.Type == LangType.Integer || val.Type == LangType.Float || val.Type == LangType.Boolean;

    public static Value Add(Value left, Value right)
    {
        if (IsNumericOrBool(left) && IsNumericOrBool(right))
        {
            if (left.Type == LangType.Float || right.Type == LangType.Float || left.Type == LangType.Boolean || right.Type == LangType.Boolean)
                return new Value(left.AsFloat() + right.AsFloat(), LangType.Float);
            return new Value(left.AsInt() + right.AsInt(), LangType.Integer);
        }
        if (left.Type == LangType.Array && right.Type == LangType.Array)
        {
            var newList = new List<Value>(left.AsArray());
            newList.AddRange(right.AsArray());
            return new Value(newList, LangType.Array, left.ElementType != LangType.Unknown ? left.ElementType : right.ElementType);
        }
        if (left.Type == LangType.Point && right.Type == LangType.Point)
        {
            PointData p1 = left.AsPoint(); PointData p2 = right.AsPoint();
            return new Value(new PointData(p1.X + p2.X, p1.Y + p2.Y), LangType.Point);
        }
        throw new NotSupportedException($"Cannot add {left.Type} (value: {left}) and {right.Type} (value: {right}).");
    }

    public static Value Subtract(Value left, Value right)
    {
        if (IsNumericOrBool(left) && IsNumericOrBool(right))
        {
            if (left.Type == LangType.Float || right.Type == LangType.Float || left.Type == LangType.Boolean || right.Type == LangType.Boolean)
                return new Value(left.AsFloat() - right.AsFloat(), LangType.Float);
            return new Value(left.AsInt() - right.AsInt(), LangType.Integer);
        }
        if (left.Type == LangType.Point && right.Type == LangType.Point)
        {
            PointData p1 = left.AsPoint(); PointData p2 = right.AsPoint();
            return new Value(new PointData(p1.X - p2.X, p1.Y - p2.Y), LangType.Point);
        }
        throw new NotSupportedException($"Cannot subtract {right.Type} from {left.Type}.");
    }

    public static Value Multiply(Value left, Value right)
    {
        if (IsNumericOrBool(left) && IsNumericOrBool(right))
        {
            if (left.Type == LangType.Float || right.Type == LangType.Float || left.Type == LangType.Boolean || right.Type == LangType.Boolean)
                return new Value(left.AsFloat() * right.AsFloat(), LangType.Float);
            return new Value(left.AsInt() * right.AsInt(), LangType.Integer);
        }
        // Множення матриці на точку
        if (left.Type == LangType.Matrix && right.Type == LangType.Point)
        {
            return new Value(left.AsMatrix().Transform(right.AsPoint()), LangType.Point);
        }
        // Множення точки на число (масштабування)
        if (left.Type == LangType.Point && IsNumericOrBool(right))
        {
            PointData p = left.AsPoint(); float scalar = right.AsFloat();
            return new Value(new PointData(p.X * scalar, p.Y * scalar), LangType.Point);
        }
        if (IsNumericOrBool(left) && right.Type == LangType.Point) // Комутативність
        {
            PointData p = right.AsPoint(); float scalar = left.AsFloat();
            return new Value(new PointData(p.X * scalar, p.Y * scalar), LangType.Point);
        }
        if (left.Type == LangType.Matrix && right.Type == LangType.Matrix)
        {
            return new Value(left.AsMatrix() * right.AsMatrix(), LangType.Matrix);
        }

        // TODO: Множення матриці на матрицю, якщо потрібно
        throw new NotSupportedException($"Cannot multiply {left.Type} by {right.Type}.");
    }

    public static Value Divide(Value left, Value right)
    {
        if (IsNumericOrBool(left) && IsNumericOrBool(right))
        {
            float divisor = right.AsFloat();
            if (Math.Abs(divisor) < 1e-9) throw new DivideByZeroException("Division by zero.");
            // Ділення завжди повертає Float для точності
            return new Value(left.AsFloat() / divisor, LangType.Float);
        }
        // Ділення точки на число (масштабування)
        if (left.Type == LangType.Point && IsNumericOrBool(right))
        {
            PointData p = left.AsPoint(); float scalar = right.AsFloat();
            if (Math.Abs(scalar) < 1e-9) throw new DivideByZeroException("Division by zero in point scaling.");
            return new Value(new PointData(p.X / scalar, p.Y / scalar), LangType.Point);
        }
        throw new NotSupportedException($"Cannot divide {left.Type} by {right.Type}.");
    }

    public static Value Modulo(Value left, Value right)
    {
        if (IsNumericOrBool(left) && IsNumericOrBool(right))
        {
            float divisor = right.AsFloat();
            if (Math.Abs(divisor) < 1e-9) throw new DivideByZeroException("Modulo by zero.");

            if (left.Type == LangType.Float || right.Type == LangType.Float || left.Type == LangType.Boolean || right.Type == LangType.Boolean)
            {
                return new Value(left.AsFloat() % divisor, LangType.Float);
            }
            // Цілочисельний модуль
            return new Value(left.AsInt() % right.AsInt(), LangType.Integer);
        }
        throw new NotSupportedException($"Cannot apply modulo to {left.Type} and {right.Type}.");
    }

    // Операції порівняння
    public static Value AreEqual(Value left, Value right) => left.Equals(right) ? Value.TrueInstance : Value.FalseInstance;
    public static Value AreNotEqual(Value left, Value right) => !left.Equals(right) ? Value.TrueInstance : Value.FalseInstance;

    public static Value LessThan(Value left, Value right)
    {
        if (IsNumericOrBool(left) && IsNumericOrBool(right))
            return left.AsFloat() < right.AsFloat() ? Value.TrueInstance : Value.FalseInstance;
        throw new NotSupportedException($"Cannot compare '{left.Type}' and '{right.Type}' with '<'.");
    }
    public static Value LessThanOrEqual(Value left, Value right)
    {
        if (IsNumericOrBool(left) && IsNumericOrBool(right))
            return left.AsFloat() <= right.AsFloat() ? Value.TrueInstance : Value.FalseInstance;
        throw new NotSupportedException($"Cannot compare '{left.Type}' and '{right.Type}' with '<='.");
    }
    public static Value GreaterThan(Value left, Value right)
    {
        if (IsNumericOrBool(left) && IsNumericOrBool(right))
            return left.AsFloat() > right.AsFloat() ? Value.TrueInstance : Value.FalseInstance;
        throw new NotSupportedException($"Cannot compare '{left.Type}' and '{right.Type}' with '>'.");
    }
    public static Value GreaterThanOrEqual(Value left, Value right)
    {
        if (IsNumericOrBool(left) && IsNumericOrBool(right))
            return left.AsFloat() >= right.AsFloat() ? Value.TrueInstance : Value.FalseInstance;
        throw new NotSupportedException($"Cannot compare '{left.Type}' and '{right.Type}' with '>='.");
    }
}
