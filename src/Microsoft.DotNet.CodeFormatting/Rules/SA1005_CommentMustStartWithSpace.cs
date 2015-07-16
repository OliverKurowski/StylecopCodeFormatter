namespace Microsoft.DotNet.CodeFormatting.Rules
{
    using System;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    [SyntaxRuleOrder(SyntaxRuleOrder.CommentMustStartWithSpaceRule)]
    internal sealed class SA1005_CommentMustStartWithSpaceRule : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        public SyntaxNode Process(SyntaxNode syntaxRoot, string languageName)
        {
            var visitor = new CommentMustStartWithSpaceRewriter();
            return visitor.Visit(syntaxRoot);
        }

        private class CommentMustStartWithSpaceRewriter : CSharpSyntaxRewriter
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
                                            !y.ContainsDiagnostics &&
                                            this.HasIncorrectFormatting(y.ToFullString()));

                if (!needsRewrite)
                {
                    return node;
                }

                return node.WithLeadingTrivia(this.FixCommentWhitespace(node.GetLeadingTrivia()));
            }

            private bool HasIncorrectFormatting(string triviaText)
            {
                // we need to exclude some lines coming from the file header
                if (triviaText == "//-----------------------------------------------------------------------" ||
                    triviaText.StartsWith("//     Copyright"))
                {
                    return false;
                }

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
                    return false;
                }

                // there must be one whitespace character between comment token and text
                if (triviaText.Length > index && !char.IsWhiteSpace(triviaText[index]))
                {
                    return true;
                }

                // there must be at most one whitespace character between comment and text
                if (triviaText.Length <= index + 1 || !char.IsWhiteSpace(triviaText[index + 1]))
                {
                    return false;
                }

                return true;
            }

            private SyntaxTriviaList FixCommentWhitespace(SyntaxTriviaList textLines)
            {
                var changedLines = new SyntaxTriviaList();

                foreach (var text in textLines)
                {
                    var fixedText = this.FixCommentWhitespaceForLine(text);
                    changedLines = changedLines.Add(fixedText);
                }

                return changedLines;
            }

            private SyntaxTrivia FixCommentWhitespaceForLine(SyntaxTrivia text)
            {
                if (text.Kind() != SyntaxKind.SingleLineCommentTrivia)
                {
                    return text;
                }

                var triviaText = text.ToFullString();

                // we need to exclude some lines coming from the file header
                if (triviaText == "//-----------------------------------------------------------------------" ||
                    triviaText.StartsWith("//     Copyright"))
                {
                    return text;
                }

                // double comment is a sign for stylecop to ignore that comment
                if (triviaText.IndexOf("////", StringComparison.Ordinal) != -1)
                {
                    return text;
                }

                var index = triviaText.IndexOf("//", StringComparison.Ordinal);

                if (index == -1)
                {
                    return text;
                }

                while (triviaText.Length > index && triviaText[index] == '/')
                {
                    index++;
                }

                if (triviaText.Length <= index)
                {
                    return text;
                }

                // no space
                if (!char.IsWhiteSpace(triviaText[index]))
                {
                    triviaText = triviaText.Insert(index, " ");
                }
                else
                {
                    var count = 0;
                    while (triviaText.Length > index + count && char.IsWhiteSpace(triviaText[index + count]))
                    {
                        count++;
                    }

                    if (count <= 1)
                    {
                        return text;
                    }

                    // too many whitespace characters
                    triviaText = triviaText.Remove(index, count - 1);
                }

                return SyntaxFactory.SyntaxTrivia(text.Kind(), triviaText);
            }
        }
    }
}
