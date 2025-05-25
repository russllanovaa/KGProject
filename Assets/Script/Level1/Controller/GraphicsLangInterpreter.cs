// GraphicsLangInterpreter.cs
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Drawing.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class ReturnException : Exception
{
    public Value ReturnValue { get; }

    public ReturnException(Value returnValue)
        : base("Return statement executed.") 
    {
        ReturnValue = returnValue;
    }
}
public class GraphicsLangInterpreterException : Exception
{
    public GraphicsLangInterpreterException(string message) : base(message) { }
    public GraphicsLangInterpreterException(string message, Exception inner) : base(message, inner) { }
    public GraphicsLangInterpreterException(IParseTree context, string message)
        : base($"Error near '{context?.GetText()}' (line {GetLine(context)}, column {GetColumn(context)}): {message}") { }

    private static int GetLine(IParseTree context)
    {
        if (context is ITerminalNode terminalNode) return terminalNode.Symbol.Line;
        if (context is ParserRuleContext ruleContext) return ruleContext.Start.Line;
        return 0;
    }
    private static int GetColumn(IParseTree context)
    {
        if (context is ITerminalNode terminalNode) return terminalNode.Symbol.Column;
        if (context is ParserRuleContext ruleContext) return ruleContext.Start.Column;
        return 0;
    }
}


public class GraphicsLangInterpreter : GraphicsLangBaseVisitor<Value>
{
    private readonly IDrawingEngine _drawingEngine;
    private readonly Stack<Dictionary<string, Value>> _scopes = new Stack<Dictionary<string, Value>>();
    private readonly Dictionary<string, FunctionDefinition> _functions = new Dictionary<string, FunctionDefinition>();
    private const int MaxRecursionDepth = 1000; // Обмеження глибини рекурсії
    private bool _isInsideFunctionCallContext = false; // для функцій з return
    private int _currentRecursionDepth = 0;

    public GraphicsLangInterpreter(IDrawingEngine drawingEngine)
    {
        _drawingEngine = drawingEngine ?? throw new ArgumentNullException(nameof(drawingEngine));
        EnterScope(); // Глобальна область видимості
    }

    private void EnterScope() => _scopes.Push(new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase));
    private void ExitScope() => _scopes.Pop();

    private LangType ParseLangType(GraphicsLangParser.TypeContext typeContext, out LangType elementLangType)
    {
        elementLangType = LangType.Unknown;
        if (typeContext == null) return LangType.Unknown; // Або кидати помилку

        if (typeContext.INT() != null) return LangType.Integer;
        if (typeContext.FLOAT() != null) return LangType.Float;
        if (typeContext.POINT() != null) return LangType.Point;
        if (typeContext.MATRIX() != null) return LangType.Matrix;
        // Boolean не є окремим типом у граматиці, але може бути корисним внутрішньо
        if (typeContext.ARRAY() != null)
        {
            if (typeContext.type() != null) // ARRAY < type >
            {
                // Рекурсивно отримуємо тип елемента
                elementLangType = ParseLangType(typeContext.type(), out _); // Внутрішній elementLangType не потрібен тут
            }
            else
            {
                throw new GraphicsLangInterpreterException(typeContext, "Array type declaration is missing element type.");
            }
            return LangType.Array;
        }
        throw new GraphicsLangInterpreterException(typeContext, $"Unknown type: {typeContext.GetText()}");
    }

    private Value GetDefaultValueForType(LangType type, LangType elementType)
    {
        switch (type)
        {
            case LangType.Integer: return new Value(0, LangType.Integer);
            case LangType.Float: return new Value(0.0f, LangType.Float);
            case LangType.Boolean: return Value.FalseInstance; // Хоча Boolean не є явним типом
            case LangType.Point: return new Value(new PointData(0, 0), LangType.Point);
            case LangType.Matrix: return new Value(MatrixData.Identity, LangType.Matrix);
            case LangType.Array: return new Value(new List<Value>(), LangType.Array, elementType);
            case LangType.Void: return Value.VoidInstance;
            default: return Value.VoidInstance; // Або кидати помилку для невідомих типів
        }
    }


    private void DefineVariable(GraphicsLangParser.VariableDeclarationContext ctx, string name, Value value)
    {
        if (_scopes.Peek().ContainsKey(name))
            throw new GraphicsLangInterpreterException(ctx.IDENTIFIER(), $"Variable '{name}' is already defined in this scope.");

        LangType declaredLangType = ParseLangType(ctx.type(), out LangType declaredElementType);

        // Перевірка типів (спрощена)
        if (value.Type != LangType.Void && declaredLangType != LangType.Unknown) // Якщо є ініціалізатор
        {

            if (declaredLangType == LangType.Array && value.Type == LangType.Array)
            {
                // Для масивів, перевіряємо сумісність типів елементів, якщо потрібно
                // Наразі просто приймаємо, якщо обидва - масиви
                if (declaredElementType != LangType.Unknown && value.ElementType != LangType.Unknown && declaredElementType != value.ElementType)
                {
                    // Можна спробувати конвертувати елементи або кинути помилку
                    // throw new GraphicsLangInterpreterException(ctx.expression(), $"Cannot assign array of {value.ElementType} to array of {declaredElementType} for variable '{name}'.");
                }
                if (value.ElementType == LangType.Unknown && declaredElementType != LangType.Unknown)
                {
                    value = new Value(value.RawValue, LangType.Array, declaredElementType);
                }
            }
            else if (declaredLangType == LangType.Matrix && value.Type == LangType.Matrix)
            {
                // Нічого додаткового не треба, просто приймаємо
            }
            else if (declaredLangType != value.Type)
            {
                // Спроба конвертації для базових типів
                try
                {
                    if (declaredLangType == LangType.Float && value.Type == LangType.Integer)
                        value = new Value(value.AsFloat(), LangType.Float);
                    else if (declaredLangType == LangType.Integer && value.Type == LangType.Float)
                        value = new Value(value.AsInt(), LangType.Integer); // Втрата точності
                    else
                        throw new GraphicsLangInterpreterException(ctx.expression() ?? (IParseTree)ctx.IDENTIFIER(), $"Type mismatch for variable '{name}'. Expected {declaredLangType}, got {value.Type}.");
                }
                catch (InvalidCastException ex)
                {
                    throw new GraphicsLangInterpreterException(ctx.expression() ?? (IParseTree)ctx.IDENTIFIER(), $"Cannot convert value for variable '{name}' from {value.Type} to {declaredLangType}: {ex.Message}");
                }
            }
        }
        else if (value.Type == LangType.Void && declaredLangType != LangType.Unknown) // Немає ініціалізатора, встановлюємо значення за замовчуванням
        {
            value = GetDefaultValueForType(declaredLangType, declaredElementType);
        }


        _scopes.Peek()[name] = value;
    }


    private Value AssignVariable(IParseTree assignTargetContext, string name, Value value)
    {
        foreach (var scope in _scopes)
        {
            if (scope.TryGetValue(name, out Value existingValue))
            {
                // Перевірка типів при присвоєнні (спрощена)
                if (existingValue.Type == LangType.Array && value.Type == LangType.Array)
                {
                    if (existingValue.ElementType != LangType.Unknown && value.ElementType != LangType.Unknown && existingValue.ElementType != value.ElementType)
                    {
                        // throw new GraphicsLangInterpreterException(assignTargetContext, $"Cannot assign array of {value.ElementType} to array variable '{name}' of {existingValue.ElementType}.");
                    }
                    // Якщо існуючий масив має тип елементів, а новий - ні (напр. порожній), то тип нового стає як в існуючого
                    if (existingValue.ElementType != LangType.Unknown && value.ElementType == LangType.Unknown)
                    {
                        value = new Value(value.RawValue, LangType.Array, existingValue.ElementType);
                    }
                }
                else if (existingValue.Type == LangType.Matrix && value.Type == LangType.Matrix)
                {
                    // Нічого додаткового, просто приймаємо
                }
                else if (existingValue.Type != value.Type && existingValue.Type != LangType.Unknown && value.Type != LangType.Void)
                {
                    try
                    {
                        if (existingValue.Type == LangType.Float && value.Type == LangType.Integer)
                            value = new Value(value.AsFloat(), LangType.Float);
                        else if (existingValue.Type == LangType.Integer && value.Type == LangType.Float)
                            value = new Value(value.AsInt(), LangType.Integer);
                        else if (existingValue.Type == LangType.Boolean && (value.Type == LangType.Integer || value.Type == LangType.Float)) // Присвоєння числових в Boolean
                            value = value.AsFloat() != 0.0f ? Value.TrueInstance : Value.FalseInstance;
                        else
                            throw new GraphicsLangInterpreterException(assignTargetContext, $"Type mismatch for variable '{name}'. Expected {existingValue.Type}, got {value.Type}.");
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new GraphicsLangInterpreterException(assignTargetContext, $"Cannot convert value for assignment to variable '{name}' from {value.Type} to {existingValue.Type}: {ex.Message}");
                    }
                }
                scope[name] = value;
                return value;
            }
        }
        throw new GraphicsLangInterpreterException(assignTargetContext, $"Variable '{name}' not declared before assignment.");
    }

    private Value ResolveVariable(IParseTree context, string name)
    {
        foreach (var scope in _scopes)
        {
            if (scope.TryGetValue(name, out var value))
            {
                return value;
            }
        }
        throw new GraphicsLangInterpreterException(context, $"Variable '{name}' is not defined.");
    }

    // --- Program & Statements ---
    public override Value VisitProgram(GraphicsLangParser.ProgramContext context)
    {
        try
        {
            foreach (var stmt in context.statement())
            {
                Visit(stmt);
            }
        }
        catch (GraphicsLangInterpreterException) { throw; } // Прокидуємо наші помилки
        catch (Exception ex) // Ловимо інші непередбачені помилки
        {
            throw new GraphicsLangInterpreterException(context, $"An unexpected runtime error occurred: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
        return Value.VoidInstance;
    }

    // VisitStatement не потрібен, якщо кожен підтип statement обробляється окремо
    // public override Value VisitStatement(GraphicsLangParser.StatementContext context) { ... }

    public override Value VisitStatement(GraphicsLangParser.StatementContext context)
    {
        if (context.variableDeclaration() != null) return Visit(context.variableDeclaration());
        if (context.assignment() != null) return Visit(context.assignment());
        if (context.functionDeclaration() != null) return Visit(context.functionDeclaration());
        if (context.ifStatement() != null) return Visit(context.ifStatement());
        if (context.forStatement() != null) return Visit(context.forStatement());
        if (context.whileStatement() != null) return Visit(context.whileStatement());
        if (context.startDrawStatement() != null) return Visit(context.startDrawStatement());
        if (context.drawStatement() != null) return Visit(context.drawStatement());
        if (context.drawControlStatement() != null) return Visit(context.drawControlStatement()); 
        if (context.expressionStatement() != null) return Visit(context.expressionStatement());
        if(context.returnStatement() != null) return Visit(context.returnStatement());

        throw new GraphicsLangInterpreterException(context, "Unsupported statement.");
    }


    public override Value VisitVariableDeclaration(GraphicsLangParser.VariableDeclarationContext context)
    {
        string varName = context.IDENTIFIER().GetText();
        Value value = Value.VoidInstance; // Значення за замовчуванням

        if (context.expression() != null)
        {
            value = Visit(context.expression());
        }
        // DefineVariable обробляє типи та значення за замовчуванням
        DefineVariable(context, varName, value);
        return Value.VoidInstance;
    }

    public override Value VisitAssignment(GraphicsLangParser.AssignmentContext context)
    {
        Value valueToAssign = Visit(context.expression());
        IParseTree targetContext;

        if (context.IDENTIFIER() != null)
        {
            targetContext = context.IDENTIFIER();
            string varName = targetContext.GetText();
            AssignVariable(targetContext, varName, valueToAssign);
        }
        else if (context.pointAccess() != null)
        {
            targetContext = context.pointAccess();
            string pointVarName = context.pointAccess().IDENTIFIER().GetText();
            Value pointVal = ResolveVariable(context.pointAccess().IDENTIFIER(), pointVarName);
            if (pointVal.Type != LangType.Point)
                throw new GraphicsLangInterpreterException(context.pointAccess().IDENTIFIER(), $"Variable '{pointVarName}' is not a Point.");

            PointData currentPoint = pointVal.AsPoint(); // Отримуємо копію, бо структура
            float componentValue = valueToAssign.AsFloat();

            if (context.pointAccess().X() != null) currentPoint.X = componentValue;
            else if (context.pointAccess().Y() != null) currentPoint.Y = componentValue;
            else throw new GraphicsLangInterpreterException(context.pointAccess(), "Invalid point component access for assignment.");

            AssignVariable(context.pointAccess().IDENTIFIER(), pointVarName, new Value(currentPoint, LangType.Point));
        }
        else if (context.arrayAccess() != null)
        {
            targetContext = context.arrayAccess();
            string arrayVarName = context.arrayAccess().IDENTIFIER().GetText();
            Value arrayVal = ResolveVariable(context.arrayAccess().IDENTIFIER(), arrayVarName);
            if (arrayVal.Type != LangType.Array)
                throw new GraphicsLangInterpreterException(context.arrayAccess().IDENTIFIER(), $"Variable '{arrayVarName}' is not an Array.");

            List<Value> list = arrayVal.AsArray();
            Value indexVal = Visit(context.arrayAccess().expression());
            int index = indexVal.AsInt();

            if (index < 0 || index >= list.Count)
                throw new GraphicsLangInterpreterException(context.arrayAccess().expression(), $"Index {index} out of bounds for array '{arrayVarName}' (size {list.Count}).");

            // Перевірка типу елемента при присвоєнні в масив
            if (arrayVal.ElementType != LangType.Unknown && valueToAssign.Type != arrayVal.ElementType)
            {
                // Спроба конвертації, якщо базові типи сумісні
                if (arrayVal.ElementType == LangType.Float && valueToAssign.Type == LangType.Integer)
                    valueToAssign = new Value(valueToAssign.AsFloat(), LangType.Float);
                else if (arrayVal.ElementType == LangType.Integer && valueToAssign.Type == LangType.Float)
                    valueToAssign = new Value(valueToAssign.AsInt(), LangType.Integer); // Втрата точності
                else
                    throw new GraphicsLangInterpreterException(context.expression(), $"Cannot assign value of type {valueToAssign.Type} to array '{arrayVarName}' expecting element type {arrayVal.ElementType}.");
            }
            list[index] = valueToAssign;
        }
        else
        {
            throw new GraphicsLangInterpreterException(context, "Invalid target for assignment.");
        }
        return Value.VoidInstance;
    }

    public override Value VisitFunctionDeclaration(GraphicsLangParser.FunctionDeclarationContext context)
    {
        string funcName = context.IDENTIFIER().GetText();
        if (_functions.ContainsKey(funcName))
            throw new GraphicsLangInterpreterException(context.IDENTIFIER(), $"Function '{funcName}' is already declared.");

        List<string> paramNames = context.parameterList()?.IDENTIFIER()?.Select(id => id.GetText()).ToList() ?? new List<string>();
        var bodyStmts = context.statement()?.ToArray() ?? Array.Empty<GraphicsLangParser.StatementContext>(); // Отримуємо масив

        // Наразі тип повернення не використовується для статичної перевірки, але можна зберігати
        // GraphicsLangParser.TypeContext returnTypeCtx = null; // Якщо граматика підтримує явний тип повернення

        _functions[funcName] = new FunctionDefinition(funcName, paramNames, bodyStmts);
        return Value.VoidInstance;
    }

    public override Value VisitIfStatement(GraphicsLangParser.IfStatementContext context)
    {
        Value condition = Visit(context.expression());
        bool isTrue = condition.AsBool();

        var allStatements = context.statement()?.ToList() ?? new List<GraphicsLangParser.StatementContext>();
        List<GraphicsLangParser.StatementContext> ifBody = new List<GraphicsLangParser.StatementContext>();
        List<GraphicsLangParser.StatementContext> elseBody = new List<GraphicsLangParser.StatementContext>();

        var elseNode = context.ELSE();
        if (elseNode != null)
        {
            int elseStartIndex = elseNode.Symbol.StartIndex;
            foreach (var stmtCtx in allStatements)
            {
                if (stmtCtx.Start.StartIndex < elseStartIndex)
                {
                    ifBody.Add(stmtCtx);
                }
                else
                {
                    elseBody.Add(stmtCtx);
                }
            }
        }
        else
        {
            ifBody.AddRange(allStatements);
        }


        if (isTrue)
        {
            EnterScope();
            try { foreach (var stmt in ifBody) Visit(stmt); }
            finally { ExitScope(); }
        }
        else if (elseNode != null)
        {
            EnterScope();
            try { foreach (var stmt in elseBody) Visit(stmt); }
            finally { ExitScope(); }
        }
        return Value.VoidInstance;
    }

    public override Value VisitForStatement(GraphicsLangParser.ForStatementContext context)
    {
        EnterScope(); // Область видимості для всього циклу for (включаючи ініціалізатор)
        try
        {
            // Ініціалізатор
            if (context.variableDeclaration() != null)
            {
                Visit(context.variableDeclaration());
            }
            else if (context.assignment(0) != null && context.assignment(0).Start.StartIndex < context.SEMICOLON(0).Symbol.StartIndex)
            {
                // Перевіряємо, що це присвоєння є ініціалізатором (перед першою ';')
                Visit(context.assignment(0));
            }


            while (true)
            {
                Value conditionResult = Value.TrueInstance; // Якщо умова відсутня, цикл нескінченний (але break можливий)
                if (context.expression() != null) // Умова є
                {
                    conditionResult = Visit(context.expression());
                }

                if (!conditionResult.AsBool()) break; // Вихід з циклу

                // Тіло циклу
                EnterScope(); // Окрема область для кожної ітерації тіла
                try
                {
                    foreach (var stmt in context.statement()) Visit(stmt);
                }
                finally { ExitScope(); }


                // Інкремент/декремент
                // Шукаємо присвоєння, яке є інкрементом (після другої ';')
                var assignments = context.assignment();
                GraphicsLangParser.AssignmentContext incrementAssignment = null;
                if (assignments != null && assignments.Length > 0)
                {
                    if (context.SEMICOLON().Length > 1) // Повинно бути дві крапки з комою
                    {
                        int secondSemicolonIndex = context.SEMICOLON(1).Symbol.StartIndex;
                        incrementAssignment = assignments.FirstOrDefault(a => a.Start.StartIndex > secondSemicolonIndex);
                    }
                }
                if (incrementAssignment != null)
                {
                    Visit(incrementAssignment);
                }
            }
        }
        finally { ExitScope(); } // Завершення області видимості циклу for
        return Value.VoidInstance;
    }


    public override Value VisitWhileStatement(GraphicsLangParser.WhileStatementContext context)
    {
        // While не створює власної зовнішньої області видимості для умови,
        // але кожна ітерація тіла може мати свою область.
        while (Visit(context.expression()).AsBool())
        {
            EnterScope(); // Область видимості для тіла циклу
            try
            {
                foreach (var stmt in context.statement()) Visit(stmt);
            }
            finally { ExitScope(); }
        }
        return Value.VoidInstance;
    }


    public override Value VisitExpressionStatement(GraphicsLangParser.ExpressionStatementContext context)
    {
        return Visit(context.expression()); // Повертаємо значення виразу (важливо для функцій)
    }

    // --- Drawing Statements ---
    public override Value VisitStartDrawStatement(GraphicsLangParser.StartDrawStatementContext context)
    {
        _drawingEngine.StartDraw(); 
        return Value.VoidInstance;
    }

    public override Value VisitDrawStatement(GraphicsLangParser.DrawStatementContext context)
    {
        Value pointVal = Visit(context.expression());
        if (pointVal.Type != LangType.Point)
            throw new GraphicsLangInterpreterException(context.expression(), $"DRAWTO expects a POINT argument, got {pointVal.Type}.");
        _drawingEngine.DrawTo(pointVal.AsPoint());
        return Value.VoidInstance;
    }

    public override Value VisitDrawControlStatement(GraphicsLangParser.DrawControlStatementContext context)
    {
        if (context.STARTDRAW() != null)
        {
            _drawingEngine.StartDraw();
        }
        else if (context.colorSetStatement() != null)
        {
            Visit(context.colorSetStatement());
        }
        return Value.VoidInstance;
    }

    public override Value VisitColorSetStatement(GraphicsLangParser.ColorSetStatementContext context)
    {
        var expr = context.expression();
        if (expr.Length != 3) throw new GraphicsLangInterpreterException(context, "Color set statement expects 3 expressions.");

        Value v1_val = Visit(expr[0]); float v1 = v1_val.AsFloat();
        Value v2_val = Visit(expr[1]); float v2 = v2_val.AsFloat();
        Value v3_val = Visit(expr[2]); float v3 = v3_val.AsFloat();

        if (context.COLORRGB() != null) _drawingEngine.SetColorRGB(v1, v2, v3);
        else if (context.COLORCMY() != null) _drawingEngine.SetColorCMY(v1, v2, v3);
        else if (context.COLORHSV() != null) _drawingEngine.SetColorHSV(v1, v2, v3);
        else if (context.COLORLAB() != null) _drawingEngine.SetColorLAB(v1, v2, v3);
        else throw new GraphicsLangInterpreterException(context, "Unknown color set statement.");
        return Value.VoidInstance;
    }

    public override Value VisitReturnStatement(GraphicsLangParser.ReturnStatementContext context)
    {
        if (!_isInsideFunctionCallContext)
        {
            throw new GraphicsLangInterpreterException(context, "RETURN statement can only be used inside a function body.");
        }

        Value valueToReturn = Visit(context.expression());
        throw new ReturnException(valueToReturn); // Кидаємо виняток зі значенням
    }

    // --- Expressions ---
    public override Value VisitExpression(GraphicsLangParser.ExpressionContext context)
    {
        try
        {
            if (context.ChildCount == 2 &&
            context.GetChild(0) is Antlr4.Runtime.Tree.ITerminalNode terminalNode &&
            terminalNode.Symbol.Text == "-" &&
            context.expression().Length == 1) // Переконуємося, що другий Child є expression
            {
                Value operand = Visit(context.expression(0)); // Отримуємо єдиний дочірній вираз

                switch (operand.Type)
                {
                    case LangType.Integer:
                        return new Value(-operand.AsInt(), LangType.Integer);
                    case LangType.Float:
                        return new Value(-operand.AsFloat(), LangType.Float);
                    case LangType.Point:
                        PointData p = operand.AsPoint();
                        return new Value(new PointData(-p.X, -p.Y), LangType.Point);
                    default:
                        throw new GraphicsLangInterpreterException(context, $"Unary minus operation is not supported for type {operand.Type}.");
                }
            }
            // Обробка бінарних операцій
            else if (context.op != null)
            {
                Value left = Visit(context.expression(0));
                Value right = Visit(context.expression(1));
                string op = context.op.Text;

                switch (op)
                {
                    case "*": return ValueOperations.Multiply(left, right);
                    case "/": return ValueOperations.Divide(left, right);
                    case "%": return ValueOperations.Modulo(left, right);
                    case "+": return ValueOperations.Add(left, right);
                    case "-": return ValueOperations.Subtract(left, right);
                    case "==": return ValueOperations.AreEqual(left, right);
                    case "!=": return ValueOperations.AreNotEqual(left, right);
                    case "<": return ValueOperations.LessThan(left, right);
                    case "<=": return ValueOperations.LessThanOrEqual(left, right);
                    case ">": return ValueOperations.GreaterThan(left, right);
                    case ">=": return ValueOperations.GreaterThanOrEqual(left, right);
                    default: throw new GraphicsLangInterpreterException(context, $"Operator '{op}' not supported.");
                }
            }
            
            // Замість: else if (context.LPAREN() != null && context.RPAREN() != null && ...)
            else if (context.ChildCount == 3 &&
                     context.GetChild(0) is Antlr4.Runtime.Tree.ITerminalNode lparenToken &&
                     lparenToken.Symbol.Type == GraphicsLangLexer.LPAREN && // Замініть на ваш тип токена з лексера
                     context.GetChild(2) is Antlr4.Runtime.Tree.ITerminalNode rparenToken &&
                     rparenToken.Symbol.Type == GraphicsLangLexer.RPAREN && // Замініть на ваш тип токена з лексера
                     context.expression().Length == 1 &&
                     context.expression(0) == context.GetChild(1)) // Переконуємося, що середній елемент - це context.expression(0)
            {
                return Visit(context.expression(0));
            }
            else if (context.functionCall() != null) return Visit(context.functionCall());
            else if (context.pointAccess() != null) return Visit(context.pointAccess());
            else if (context.arrayAccess() != null) return Visit(context.arrayAccess());
            else if (context.literal() != null) return Visit(context.literal());
            else if (context.IDENTIFIER() != null) return ResolveVariable(context.IDENTIFIER(), context.IDENTIFIER().GetText());

            // Якщо context.expression().Length > 1, але немає op - це помилка в граматиці або дереві
            throw new GraphicsLangInterpreterException(context, "Unhandled or malformed expression structure.");
        }
        catch (GraphicsLangInterpreterException) { throw; } // Прокидуємо наші помилки
        catch (NullReferenceException nre)
        {
            throw new GraphicsLangInterpreterException(context, $"Null reference encountered during expression evaluation: {nre.Message}");
        }
        catch (InvalidCastException ice)
        {
            throw new GraphicsLangInterpreterException(context, $"Invalid type cast during expression evaluation: {ice.Message}");
        }
        catch (DivideByZeroException dbze)
        {
            throw new GraphicsLangInterpreterException(context, dbze.Message);
        }
        catch (Exception ex)
        {
            throw new GraphicsLangInterpreterException(context, $"Unexpected error during expression evaluation: {ex.Message}");
        }
    }



    public override Value VisitFunctionCall(GraphicsLangParser.FunctionCallContext context)
    {
        if (_currentRecursionDepth >= MaxRecursionDepth)
        {
            throw new GraphicsLangInterpreterException(context, $"Maximum recursion depth ({MaxRecursionDepth}) exceeded.");
        }
        _currentRecursionDepth++;

        try
        {
            // Обробка вбудованих функцій (COS, SIN, TRANSFORM) ...
            // Ваш існуючий код для COS, SIN, TRANSFORM
            if (context.COS() != null)
            {
                GraphicsLangParser.ExpressionContext argCtx = context.expression();
                if (argCtx == null)
                {
                    throw new GraphicsLangInterpreterException(context, "COS function expects 1 argument, but it's missing (parser error).");
                }
                Value arg = Visit(argCtx);
                if (arg.Type != LangType.Float && arg.Type != LangType.Integer)
                {
                    throw new GraphicsLangInterpreterException(argCtx, $"COS function expects a numeric argument, got {arg.Type}.");
                }
                return new Value((float)Math.Cos(arg.AsFloat()), LangType.Float);
            }

            if (context.SIN() != null)
            {
                GraphicsLangParser.ExpressionContext argCtx = context.expression();
                if (argCtx == null)
                {
                    throw new GraphicsLangInterpreterException(context, "SIN function expects 1 argument, but it's missing (parser error).");
                }
                Value arg = Visit(argCtx);
                if (arg.Type != LangType.Float && arg.Type != LangType.Integer)
                {
                    throw new GraphicsLangInterpreterException(argCtx, $"SIN function expects a numeric argument, got {arg.Type}.");
                }
                return new Value((float)Math.Sin(arg.AsFloat()), LangType.Float);
            }
            if (context.SQRT() != null) // Припускаємо, що SQRT() тепер існує в GraphicsLangParser.FunctionCallContext
            {
                GraphicsLangParser.ExpressionContext argCtx = context.expression(); // SQRT, як і COS/SIN, очікує один expression
                if (argCtx == null)
                {
                    throw new GraphicsLangInterpreterException(context, "SQRT function expects 1 argument, but it's missing (parser error).");
                }
                Value arg = Visit(argCtx);
                if (arg.Type != LangType.Float && arg.Type != LangType.Integer)
                {
                    throw new GraphicsLangInterpreterException(argCtx, $"SQRT function expects a numeric argument, got {arg.Type}.");
                }
                float floatArg = arg.AsFloat();
                if (floatArg < 0)
                {
                    throw new GraphicsLangInterpreterException(argCtx, $"SQRT function input cannot be negative, got {floatArg}.");
                }
                return new Value((float)Math.Sqrt(floatArg), LangType.Float);
            }
            if (context.TRANSFORM() != null)
            {
                GraphicsLangParser.ExpressionContext argCtx = context.expression();
                if (argCtx == null)
                {
                    throw new GraphicsLangInterpreterException(context, "TRANSFORM function expects 1 argument, but it's missing (parser error).");
                }
                Value arg = Visit(argCtx);
                if (arg.Type == LangType.Point)
                {
                    return new Value(_drawingEngine.ApplyCurrentTransform(arg.AsPoint()), LangType.Point);
                }
                else if (arg.Type == LangType.Matrix)
                {
                    _drawingEngine.SetCurrentTransformMatrix(arg.AsMatrix());
                    return Value.VoidInstance;
                }
                throw new GraphicsLangInterpreterException(argCtx, $"TRANSFORM function expects a POINT or MATRIX argument, got {arg.Type}.");
            }

            // Обробка користувацьких функцій
            string funcNameForUserDefined = context.IDENTIFIER()?.GetText();
            if (!string.IsNullOrEmpty(funcNameForUserDefined))
            {
                List<Value> argValues = new List<Value>();
                if (context.argumentList() != null)
                {
                    argValues = context.argumentList().expression().Select(expCtx => Visit(expCtx)).ToList();
                }

                if (!_functions.TryGetValue(funcNameForUserDefined, out var funcDef))
                {
                    throw new GraphicsLangInterpreterException(context.IDENTIFIER(), $"Function '{funcNameForUserDefined}' is not defined.");
                }

                // Припускаємо, що funcDef.ParameterNames це List<string>, тому .Count є властивістю
                if (funcDef.ParameterNames.Count != argValues.Count)
                {
                    IParseTree errorNode = context.argumentList() ?? (IParseTree)context.IDENTIFIER() ?? context;
                    throw new GraphicsLangInterpreterException(errorNode,
                        $"Function '{funcNameForUserDefined}' expects {funcDef.ParameterNames.Count} arguments, but got {argValues.Count}.");
                }

                EnterScope();
                bool previousInsideFunctionState = _isInsideFunctionCallContext; // Зберігаємо попередній стан
                _isInsideFunctionCallContext = true;                              // Встановлюємо прапорець
                try
                {
                    // Прив'язка аргументів до параметрів
                    for (int i = 0; i < funcDef.ParameterNames.Count; i++)
                    {
                        _scopes.Peek()[funcDef.ParameterNames[i]] = argValues[i];
                    }

                    Value calculatedReturnValue = Value.VoidInstance; // Значення за замовчуванням, якщо немає явного return
                                                                      // або якщо остання інструкція не є expressionStatement

                    // funcDef.BodyStatements це StatementContext[], тому використовуємо .Length
                    if (funcDef.BodyStatements != null && funcDef.BodyStatements.Length > 0)
                    {
                        for (int i = 0; i < funcDef.BodyStatements.Length; i++)
                        {
                            var stmtCtx = funcDef.BodyStatements[i];
                            Value lastStmtValue = Visit(stmtCtx); // Якщо stmtCtx - це RETURN, то тут буде кинуто ReturnException

                            // Неявне повернення значення останнього expressionStatement,
                            // ЯКЩО не було явного RETURN раніше.
                            if (i == funcDef.BodyStatements.Length - 1)
                            {
                                if (stmtCtx is GraphicsLangParser.ExpressionStatementContext)
                                {
                                    calculatedReturnValue = lastStmtValue;
                                }
                                // Якщо останній statement був return, ReturnException вже оброблено.
                                // Якщо це інший тип statement (не expression і не return),
                                // то calculatedReturnValue залишиться VoidInstance (або тим, що було до цього).
                            }
                        }
                    }
                    // Якщо цикл завершився без ReturnException, повертаємо calculatedReturnValue
                    return calculatedReturnValue;
                }
                catch (ReturnException rex)
                {
                    // Спіймали явний RETURN з функції
                    return rex.ReturnValue;
                }
                finally
                {
                    ExitScope();
                    _isInsideFunctionCallContext = previousInsideFunctionState; // Відновлюємо прапорець
                }
            }

            // Якщо не вбудована і не користувацька функція
            throw new GraphicsLangInterpreterException(context, "Invalid function call syntax or unrecognized function.");
        }
        finally
        {
            _currentRecursionDepth--;
        }
    }

    public override Value VisitPointAccess(GraphicsLangParser.PointAccessContext context)
    {
        Value pointVal = ResolveVariable(context.IDENTIFIER(), context.IDENTIFIER().GetText());
        if (pointVal.Type != LangType.Point)
            throw new GraphicsLangInterpreterException(context.IDENTIFIER(), $"Variable '{context.IDENTIFIER().GetText()}' is not a Point (actual type: {pointVal.Type}).");

        PointData p = pointVal.AsPoint();
        if (context.X() != null) return new Value(p.X, LangType.Float);
        if (context.Y() != null) return new Value(p.Y, LangType.Float);
        throw new GraphicsLangInterpreterException(context, "Invalid point component access.");
    }

    public override Value VisitArrayAccess(GraphicsLangParser.ArrayAccessContext context)
    {
        Value arrayVal = ResolveVariable(context.IDENTIFIER(), context.IDENTIFIER().GetText());
        if (arrayVal.Type != LangType.Array)
            throw new GraphicsLangInterpreterException(context.IDENTIFIER(), $"Variable '{context.IDENTIFIER().GetText()}' is not an Array (actual type: {arrayVal.Type}).");

        List<Value> list = arrayVal.AsArray();
        Value indexVal = Visit(context.expression());
        int index = indexVal.AsInt();

        if (index < 0 || index >= list.Count)
            throw new GraphicsLangInterpreterException(context.expression(), $"Index {index} out of bounds for array '{context.IDENTIFIER().GetText()}' (size {list.Count}).");
        return list[index];
    }

    // --- Literals ---
    public override Value VisitLiteral(GraphicsLangParser.LiteralContext context)
    {
        if (context.NUMBER() != null)
        {
            string numStr = context.NUMBER().GetText();
            try
            {
                // Використовуємо InvariantCulture для кращої обробки '.' як десяткового роздільника
                if (numStr.Contains(".")) return new Value(float.Parse(numStr, CultureInfo.InvariantCulture), LangType.Float);
                return new Value(int.Parse(numStr, CultureInfo.InvariantCulture), LangType.Integer);
            }
            catch (FormatException fe)
            {
                throw new GraphicsLangInterpreterException(context.NUMBER(), $"Invalid number format: {numStr}. {fe.Message}");
            }
        }
        if (context.pointLiteral() != null) return Visit(context.pointLiteral());
        if (context.arrayLiteral() != null) return Visit(context.arrayLiteral());
        if (context.matrixLiteral() != null) return Visit(context.matrixLiteral());

        if (context.PI() != null) return new Value((float)Math.PI, LangType.Float);
        throw new GraphicsLangInterpreterException(context, "Unknown or unhandled literal type.");
    }

    public override Value VisitPointLiteral(GraphicsLangParser.PointLiteralContext context)
    {
        if (context.expression() == null || context.expression().Length != 2)
            throw new GraphicsLangInterpreterException(context, "Point literal expects exactly 2 expressions for X and Y coordinates.");
        Value xVal = Visit(context.expression(0)); float x = xVal.AsFloat();
        Value yVal = Visit(context.expression(1)); float y = yVal.AsFloat();
        return new Value(new PointData(x, y), LangType.Point);
    }

    public override Value VisitArrayLiteral(GraphicsLangParser.ArrayLiteralContext context)
    {
        List<Value> elements = context.expression()?.Select(exp => Visit(exp)).ToList() ?? new List<Value>();
        LangType detectedElementType = LangType.Unknown;
        if (elements.Any())
        {
            detectedElementType = elements.First().Type;
            // Перевірка, чи всі елементи одного типу (опціонально, для більш строгої типізації)
            // if (elements.Any(e => e.Type != detectedElementType))
            //    throw new GraphicsLangInterpreterException(context, "Array literal contains elements of mixed types. This is currently not fully supported for typed arrays if declared with specific element type.");
        }
        return new Value(elements, LangType.Array, detectedElementType);
    }

    public override Value VisitMatrixLiteral(GraphicsLangParser.MatrixLiteralContext context)
    {
        var expr = context.expression();
        if (expr == null || expr.Length != 6)
            throw new GraphicsLangInterpreterException(context, "Matrix literal expects exactly 6 expressions for a 2x3 matrix (m11, m12, m21, m22, dx, dy).");

        float m11 = Visit(expr[0]).AsFloat(); float m12 = Visit(expr[1]).AsFloat();
        float m21 = Visit(expr[2]).AsFloat(); float m22 = Visit(expr[3]).AsFloat();
        float dx = Visit(expr[4]).AsFloat(); float dy = Visit(expr[5]).AsFloat();
        return new Value(new MatrixData(m11, m12, m21, m22, dx, dy), LangType.Matrix);
    }



    // Перевизначаємо VisitChildren та VisitTerminal, щоб уникнути випадкових викликів,
    // якщо якийсь Visit-метод не було реалізовано. Краще отримати явну помилку.
    public override Value VisitChildren(IRuleNode node)
    {
        throw new NotImplementedException($"VisitChildren called on {node.GetType().Name}. Ensure all relevant Visit methods are implemented or call specific child visits.");
    }

    public override Value VisitTerminal(ITerminalNode node)
    {
        // Зазвичай не викликається напряму, якщо граматика обробляє термінали в правилах.
        // Може бути корисним для отримання тексту токена, але це зазвичай робиться в Visit-методах правил.
        // Наприклад, context.IDENTIFIER().GetText()
        return base.VisitTerminal(node); // Або кидати NotImplementedException
    }
}
