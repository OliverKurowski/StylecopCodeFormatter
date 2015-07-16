namespace Microsoft.DotNet.CodeFormatting.Rules
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [LocalSemanticRuleOrder(LocalSemanticRuleOrder.RemoveExplicitThisRule)]
    internal sealed class SA1101_ExplicitThisRule : CSharpOnlyFormattingRule, ILocalSemanticFormattingRule
    {
        private sealed class ExplicitThisRewriter : CSharpSyntaxRewriter
        {
            private readonly Document document;
            private readonly CancellationToken cancellationToken;
            private SemanticModel semanticModel;
            private bool addedAnnotations;

            internal bool AddedAnnotations
            {
                get { return this.addedAnnotations; }
            }

            internal ExplicitThisRewriter(Document document, CancellationToken cancellationToken)
            {
                this.document = document;
                this.cancellationToken = cancellationToken;
            }

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (this.semanticModel == null)
                {
                    this.semanticModel = this.document.GetSemanticModelAsync(this.cancellationToken).Result;
                }

                var symbolInfo = this.semanticModel.GetSymbolInfo(node, this.cancellationToken);

                if (symbolInfo.Symbol == null)
                {
                    return node;
                }
                    
                if ((symbolInfo.Symbol.Kind == SymbolKind.Field ||
                    symbolInfo.Symbol.Kind == SymbolKind.Property ||
                    symbolInfo.Symbol.Kind == SymbolKind.Method ||
                    symbolInfo.Symbol.Kind == SymbolKind.Event) &&
                    symbolInfo.Symbol.CanBeReferencedByName &&
                    !symbolInfo.Symbol.IsStatic &&
                    !HasThisAccess(node) && 
                    CanAnnotateProperty(node))
                {
                    this.addedAnnotations = true;
                    var trivia = node.GetLeadingTrivia();

                    var memberAccess = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        node.WithoutLeadingTrivia());

                    return memberAccess.WithLeadingTrivia(trivia);
                }

                return base.VisitIdentifierName(node);
            }

            private static bool HasThisAccess(IdentifierNameSyntax node)
            {
                // detect 'this.cacheList' and 'this.cacheList.count'
                // but not 'cacheList.count'
                var hasSimpleMemberAccess = node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression);

                if (!hasSimpleMemberAccess)
                {
                    return false;
                }

                var memberAccess = (MemberAccessExpressionSyntax)node.Parent;

                if (!memberAccess.Expression.IsKind(SyntaxKind.IdentifierName))
                {
                    return false;
                }

                return memberAccess.Expression != null && memberAccess.Kind() == SyntaxKind.ThisKeyword;
            }

            private static bool CanAnnotateProperty(IdentifierNameSyntax node)
            {
                if (node.Parent.IsKind(SyntaxKind.NameEquals))
                {
                    return false;
                }

                // object initializer: new Obj { Test = '' }
                if (node.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression)
                    && node.Parent.Parent.IsKind(SyntaxKind.ObjectInitializerExpression))
                {
                    return false;
                }

                // nested member access: cacheList.AddLast, when AddLast is visited
                if (node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                    ((MemberAccessExpressionSyntax)node.Parent).Name == node)
                {
                    return false;
                }

                return true;
            }
        }

        public async Task<SyntaxNode> ProcessAsync(Document document, SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            var rewriter = new ExplicitThisRewriter(document, cancellationToken);
            var newNode = rewriter.Visit(syntaxNode);
            if (!rewriter.AddedAnnotations)
            {
                return syntaxNode;
            }

            return newNode;
        }
    }
}