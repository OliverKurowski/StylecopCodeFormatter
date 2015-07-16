namespace Microsoft.DotNet.CodeFormatting.Rules
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [SyntaxRuleOrder(SyntaxRuleOrder.CommentMustStartWithSpaceRule)]
    internal sealed class SA1505_OpenCurlyBracketMayNotBeFollowedBySpace : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        public SyntaxNode Process(SyntaxNode syntaxRoot, string languageName)
        {
            var visitor = new CurlyBracketRewriter();
            return visitor.Visit(syntaxRoot);
        }

        private class CurlyBracketRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitBlock(BlockSyntax node)
            {
                var newNode = base.VisitBlock(node);
                node = newNode as BlockSyntax;

                if (node == null)
                {
                    return newNode;
                }

                var triviaList = node.OpenBraceToken.GetNextToken().LeadingTrivia;
                var newList = new SyntaxTriviaList();
                foreach (var trivia in triviaList)
                {
                    if (trivia.Kind() != SyntaxKind.EndOfLineTrivia)
                    {
                        newList.Add(trivia);
                    }
                }

                return node.WithLeadingTrivia(newList);
            }
        }
    }
}
