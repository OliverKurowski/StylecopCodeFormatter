namespace Microsoft.DotNet.CodeFormatting.Rules
{
    using System;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    [SyntaxRuleOrder(SyntaxRuleOrder.CommentMustStartWithSpaceRule)]
    internal sealed class SA1120_CommentMustNotBeEmpty : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        public SyntaxNode Process(SyntaxNode syntaxRoot, string languageName)
        {
            var visitor = new RemoveEmptyCommentRewriter();
            return visitor.Visit(syntaxRoot);
        }

        private class RemoveEmptyCommentRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode Visit(SyntaxNode node)
            {
                node = base.Visit(node);

                if (node == null)
                {
                    return null;
                }

                var needsRewrite = node.HasLeadingTrivia &&
                                   node.GetLeadingTrivia().Any(
                                       y => y.Kind() == SyntaxKind.SingleLineCommentTrivia &&
                                            !y.IsDirective &&
                                            !y.ContainsDiagnostics);

                if (!needsRewrite)
                {
                    return node;
                }

                return node.WithLeadingTrivia(this.FixCommentWhitespace(node.GetLeadingTrivia()));
            }

            private bool HasEmptyComment(SyntaxTrivia trivia)
            {
                if (trivia.Kind() != SyntaxKind.SingleLineCommentTrivia)
                {
                    return false;
                }

                var triviaText = trivia.ToFullString();

                // double comment is a sign for stylecop to ignore that comment
                if (triviaText.IndexOf("////", StringComparison.Ordinal) != -1)
                {
                    return false;
                }

                var index = triviaText.IndexOf("//", StringComparison.Ordinal);

                if (index == -1)
                {
                    return false;
                }

                while (triviaText.Length > index && triviaText[index] == '/')
                {
                    index++;
                }

                if (triviaText.Length <= index)
                {
                    // empty comment
                    return true;
                }

                while (triviaText.Length > index && char.IsWhiteSpace(triviaText[index]))
                {
                    index++;
                }

                if (triviaText.Length <= index)
                {
                    // comment contains only whitespace
                    return true;
                }

                return false;
            }

            private SyntaxTriviaList FixCommentWhitespace(SyntaxTriviaList textLines)
            {
                var changedLines = new SyntaxTriviaList();
                bool skipNextNewline = false;

                foreach (var text in textLines)
                {
                    if (skipNextNewline)
                    {
                        skipNextNewline = false;

                        if (text.Kind() == SyntaxKind.EndOfLineTrivia)
                        {
                            continue;
                        }
                    }

                    var removeTrivia = this.HasEmptyComment(text);

                    if (!removeTrivia)
                    {
                        changedLines = changedLines.Add(text);
                    }
                    else
                    {
                        skipNextNewline = true;
                    }
                }

                return changedLines;
            }
        }
    }
}
