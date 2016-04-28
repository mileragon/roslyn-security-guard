﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynSecurityGuard
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CommandInjectionAnalyzer : DiagnosticAnalyzer
    {

        private static DiagnosticDescriptor Rule = AnalyzerUtil.GetDescriptorFromResource(typeof(CommandInjectionAnalyzer), DiagnosticSeverity.Warning);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(VisitSyntaxNode, SyntaxKind.InvocationExpression);
            //context.RegisterSemanticModelAction(VisitSemanticModel);
        }

        private static void VisitSyntaxNode(SyntaxNodeAnalysisContext ctx)
        {
            
            InvocationExpressionSyntax node = ctx.Node as InvocationExpressionSyntax;
            if (node != null) {

                //DataContext.ExecuteQuery()
                var invokedSymbol = ctx.SemanticModel.GetSymbolInfo(node).Symbol;

                if (AnalyzerUtil.InvokeMatch(invokedSymbol, className : "Process", method: "Start") && node.ArgumentList.Arguments.Count > 0) {
                    //DataFlowAnalysis flow = ctx.SemanticModel.AnalyzeDataFlow(AnalyzerUtil.GetMethodFromNode(node));

                    //if(AnalyzerUtil.ValueIsExternal(flow, node.ArgumentList.Arguments[0]))

                    if(!(node.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax)) 
                    {
                        var diagnostic = Diagnostic.Create(Rule, node.Expression.GetLocation(), new string[0]);
                        ctx.ReportDiagnostic(diagnostic);
                    }
                }
            }

        }


        private static void VisitSemanticModel(SemanticModelAnalysisContext ctx)
        {
        }
    }
}