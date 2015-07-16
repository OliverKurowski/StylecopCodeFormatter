namespace Microsoft.DotNet.CodeFormatting.Rules
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Linq;

    [SyntaxRuleOrder(SyntaxRuleOrder.NoWhitespaceBeforeBracketRule)]
    internal sealed class SA1008_NoWhitespaceBeforeParenthesis : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        private sealed class RemoveWhitespaceRewriter : CSharpSyntaxRewriter
        {
            private bool addedAnnotations;

            internal bool AddedAnnotations
            {
                get { return this.addedAnnotations; }
            }

            internal RemoveWhitespaceRewriter()
            {
            }

            public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
            {
                node = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node);

                // new QueryParameter ("nr", DataType.String, referenceVersion.PatchNumber)
                var identifier = node.Type as IdentifierNameSyntax;
                
                if (identifier != null && identifier.HasTrailingTrivia)
                {
                    this.addedAnnotations = true;
                    return node.ReplaceNode(identifier, identifier.WithoutTrailingTrivia());
                }

                return node;
            }

            public override SyntaxNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
            {
                node = (AnonymousMethodExpressionSyntax)base.VisitAnonymousMethodExpression(node);

                // zipFile.FileExtracting += delegate (object sender, ProgressStatusEventArgs ea) { };
                if (node.DelegateKeyword.HasTrailingTrivia)
                {
                    this.addedAnnotations = true;
                    return node.WithDelegateKeyword(node.DelegateKeyword.WithTrailingTrivia(null));
                }

                return node;
            }

            public override SyntaxNode VisitMethodDeclaration (MethodDeclarationSyntax node)
            {
                node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);

                if (node.Identifier.HasTrailingTrivia)
                {
                    this.addedAnnotations = true;
                    return node.WithIdentifier(node.Identifier.WithoutAnnotations());
                }

                return node;
            }
        }

        public SyntaxNode Process(SyntaxNode syntaxNode, string languageName)
        {
            var rewriter = new RemoveWhitespaceRewriter();
            var newNode = rewriter.Visit(syntaxNode);
            if (!rewriter.AddedAnnotations)
            {
                return syntaxNode;
            }

            return newNode;
        }
    }
}