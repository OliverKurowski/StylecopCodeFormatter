namespace Microsoft.DotNet.CodeFormatting.Rules
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [SyntaxRuleOrder(SyntaxRuleOrder.NoRegionsAllowedRule)]
    internal sealed class SA1124_NoRegionsAllowed : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        private sealed class RemoveRegionsRewriter : CSharpSyntaxRewriter
        {
            private bool addedAnnotations;

            internal bool AddedAnnotations
            {
                get { return this.addedAnnotations; }
            }

            internal RemoveRegionsRewriter()
            {
            }

            public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
            {
                var retVal = default(SyntaxTrivia);
                var isRegionOrEndRegionStructuredTrivia = trivia.HasStructure &&
                    (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) ||
                     trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia));

                if (isRegionOrEndRegionStructuredTrivia)
                {
                    this.addedAnnotations = true;
                    // Simply return default(SyntaxTrivia)
                }
                else
                {
                    retVal = base.VisitTrivia(trivia);
                }

                return retVal;
            }
        }

        public SyntaxNode Process(SyntaxNode syntaxNode, string languageName)
        {
            var rewriter = new RemoveRegionsRewriter();
            var newNode = rewriter.Visit(syntaxNode);
            if (!rewriter.AddedAnnotations)
            {
                return syntaxNode;
            }

            return newNode;
        }
    }
}