namespace Microsoft.DotNet.CodeFormatting.Rules
{
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [SyntaxRuleOrder(SyntaxRuleOrder.NoEmptyStringLiteralRule)]
    internal sealed class SA1122_NoEmptyStringLiteralRule : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        private sealed class NoEmptyStringLiteralRule : CSharpSyntaxRewriter
        {
            private bool addedAnnotations;

            private bool IsAttributeArgument;

            internal bool AddedAnnotations
            {
                get { return this.addedAnnotations; }
            }

            public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
            {
                this.IsAttributeArgument = node.Parent.IsKind(SyntaxKind.AttributeArgument);
                if (node.IsKind(SyntaxKind.StringLiteralExpression) &&
                    !IsParameterDefaultArgument(node) &&
                    !this.IsAttributeArgument &&
                    !IsConstVariableDeclaration(node))
                {
                    if (string.IsNullOrEmpty(node.Token.ValueText))
                    {
                        this.addedAnnotations = true;
                        return SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                            SyntaxFactory.IdentifierName("Empty"));
                    }
                }

                return base.VisitLiteralExpression(node);
            }

            private static bool IsParameterDefaultArgument(LiteralExpressionSyntax node)
            {
                return node.Parent.Parent.IsKind(SyntaxKind.Parameter);
            }

            private static bool IsConstVariableDeclaration(LiteralExpressionSyntax node)
            {
                if (
                    !(node.Parent.IsKind(SyntaxKind.EqualsValueClause)
                      && node.Parent.Parent.IsKind(SyntaxKind.VariableDeclarator)
                      && node.Parent.Parent.Parent.IsKind(SyntaxKind.VariableDeclaration)
                      && node.Parent.Parent.Parent.Parent.IsKind(SyntaxKind.FieldDeclaration)))
                {
                    return false;
                }

                foreach (var modifier in ((FieldDeclarationSyntax)node.Parent.Parent.Parent.Parent).Modifiers)
                {
                    switch (modifier.Kind())
                    {
                        case SyntaxKind.ConstKeyword:
                            return true;
                    }
                }

                return false;
            }
        }

        public SyntaxNode Process(SyntaxNode syntaxNode, string languageName)
        {
            var rewriter = new NoEmptyStringLiteralRule();
            var newNode = rewriter.Visit(syntaxNode);
            if (!rewriter.AddedAnnotations)
            {
                return syntaxNode;
            }

            return newNode;
        }
    }
}