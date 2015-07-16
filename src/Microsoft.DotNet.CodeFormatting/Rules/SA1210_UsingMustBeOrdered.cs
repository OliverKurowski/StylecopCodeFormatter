// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Tieto Corporation" file="SA1210_UsingMustBeOrdered.cs">
//   Copyright (c) Tieto Corporation. All rights reserved.
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------
namespace Microsoft.DotNet.CodeFormatting.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// This will ensure that using directives are placed outside of the namespace.
    /// </summary>
    [SyntaxRuleOrder(SyntaxRuleOrder.UsingMustBeOrderedRule)]
    internal sealed class SA1210_UsingMustBeOrdered : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        public SyntaxNode Process(SyntaxNode syntaxNode, string languageName)
        {
            var root = syntaxNode as CompilationUnitSyntax;
            if (root == null)
            {
                return syntaxNode;
            }

            var namespaces = root.Members.OfType<NamespaceDeclarationSyntax>();

            foreach (var namespaceDeclaration in namespaces)
            {
                var orderedUsings = namespaceDeclaration.Usings.OrderBy(x => x, new UsingDirectiveComparer());

                root = root.ReplaceNode(
                    namespaceDeclaration, 
                    namespaceDeclaration.WithUsings(SyntaxFactory.List(orderedUsings)));
            }

            return root;
        }

        private class UsingDirectiveComparer : IComparer<UsingDirectiveSyntax>
        {
            public int Compare(UsingDirectiveSyntax x, UsingDirectiveSyntax y)
            {
                if (IsSystemNamespace(x) && !IsSystemNamespace(y))
                {
                    return -1;
                }
                
                if (!IsSystemNamespace(x) && IsSystemNamespace(y))
                {
                    return 1;
                }

                var xn = GetName(x);
                var yn = GetName(y);
                return string.Compare(xn, yn, StringComparison.Ordinal);
            }

            private static bool IsSystemNamespace(UsingDirectiveSyntax usingDirective)
            {
                var name = GetName(usingDirective);
                return name.StartsWith("System");
            }

            private static string GetName(UsingDirectiveSyntax usingDirective)
            {
                return GetNameInternal(usingDirective.Name);
            }

            private static string GetNameInternal(NameSyntax name)
            {
                var id = name as IdentifierNameSyntax;

                if (id == null)
                {
                    var globalId = name as QualifiedNameSyntax;

                    if (globalId == null)
                    {
                        return string.Empty;
                    }

                    return GetNameInternal(globalId.Left) + "." + GetNameInternal(globalId.Right);
                }

                return id.Identifier.Text;
            }
        }
    }
}
