// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    /// <summary>
    /// This will ensure that using directives are placed outside of the namespace.
    /// </summary>
    [SyntaxRuleOrder(SyntaxRuleOrder.UsingLocationFormattingRule)]
    internal sealed class SA1200_UsingLocationRule : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        public SyntaxNode Process(SyntaxNode syntaxNode, string languageName)
        {
            var root = syntaxNode as CompilationUnitSyntax;
            if (root == null)
            {
                return syntaxNode;
            }

            // This rule can only be done safely as a syntax transformation when there is a single namespace
            // declaration in the file.  Once there is more than one it opens up the possibility of introducing
            // ambiguities to essentially make using directives global which were previously local.
            var namespaceDeclarationList = root.Members.OfType<NamespaceDeclarationSyntax>().ToList();
            if (namespaceDeclarationList.Count == 0)
            {
                return syntaxNode;
            }

            var usingList = root.Usings;
            if (usingList.Count == 0)
            {
                return syntaxNode;
            }

            var newRoot = root;

            // we need to remove the leading trivia with is the file header comment and put it back to the beginning
            // of the file.
            var firstUsing = usingList.First();

            if (firstUsing.HasLeadingTrivia)
            {
                var trivia = firstUsing.GetLeadingTrivia();

                usingList = usingList.Replace(firstUsing, firstUsing.WithoutLeadingTrivia());

                // assign the trivia to the file start
                var firstNode = newRoot.Members.First();
                newRoot = newRoot.ReplaceNode(firstNode, firstNode.WithLeadingTrivia(trivia));
            }

            for (int i = 0; i < namespaceDeclarationList.Count; i++)
            {
                var namespaceDeclaration = newRoot.Members.OfType<NamespaceDeclarationSyntax>().Skip(i).First();
                newRoot = newRoot.ReplaceNode(namespaceDeclaration, namespaceDeclaration.WithUsings(usingList));
            }

            newRoot = newRoot.WithUsings(SyntaxFactory.List<UsingDirectiveSyntax>());

            return newRoot;
        }
    }
}
