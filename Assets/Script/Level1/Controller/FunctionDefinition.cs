using Drawing.Parsing;
using System.Collections.Generic;


public class FunctionDefinition
{
    public string Name { get; }
    public List<string> ParameterNames { get; }
    public GraphicsLangParser.StatementContext[] BodyStatements { get; }
    public GraphicsLangParser.TypeContext DeclaredReturnTypeContext { get; } 

    public FunctionDefinition(string name, List<string> paramNames, GraphicsLangParser.StatementContext[] body, GraphicsLangParser.TypeContext returnType = null)
    {
        Name = name;
        ParameterNames = paramNames ?? new List<string>();
        BodyStatements = body;
        DeclaredReturnTypeContext = returnType; 
    }
}