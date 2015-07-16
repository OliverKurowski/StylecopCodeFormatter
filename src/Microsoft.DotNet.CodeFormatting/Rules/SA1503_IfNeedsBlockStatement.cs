namespace Microsoft.DotNet.CodeFormatting.Rules
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [SyntaxRuleOrder(SyntaxRuleOrder.IfNeedsBlockStatementRule)]
    internal sealed class SA1503_IfNeedsBlockStatement : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        private sealed class MissingBlockRewriter : CSharpSyntaxRewriter
        {
            private bool addedAnnotations;

            internal bool AddedAnnotations
            {
                get { return this.addedAnnotations; }
            }

            internal MissingBlockRewriter()
            {
            }

            public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
            {
                // parse the if statements recursivley 
                node = (IfStatementSyntax)base.VisitIfStatement(node);

                if (!node.Statement.IsKind(SyntaxKind.Block))
                {
                    this.addedAnnotations = true;
                    node = node.WithStatement(SyntaxFactory.Block(node.Statement));
                }

                if (node.Else != null)
                {
                    if (!node.Else.Statement.IsKind(SyntaxKind.Block) &&
                        !node.Else.Statement.IsKind(SyntaxKind.IfStatement))
                    {
                        this.addedAnnotations = true;
                        node = node.WithElse(
                            node.Else.WithStatement(SyntaxFactory.Block(node.Else.Statement)));
                    }
                }

                return node;
            }

            public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
            {
                node = (ForEachStatementSyntax)base.VisitForEachStatement(node);

                if (!node.Statement.IsKind(SyntaxKind.Block))
                {
                    this.addedAnnotations = true;
                    node = node.WithStatement(SyntaxFactory.Block(node.Statement));
                }

                return node;
            }

            public override SyntaxNode VisitForStatement(ForStatementSyntax node)
            {
                node = (ForStatementSyntax)base.VisitForStatement(node);

                if (!node.Statement.IsKind(SyntaxKind.Block))
                {
                    this.addedAnnotations = true;
                    node = node.WithStatement(SyntaxFactory.Block(node.Statement));
                }

                return node;
            }

            public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
            {
                node = (WhileStatementSyntax)base.VisitWhileStatement(node);

                if (!node.Statement.IsKind(SyntaxKind.Block))
                {
                    this.addedAnnotations = true;
                    node = node.WithStatement(SyntaxFactory.Block(node.Statement));
                }

                return node;
            }

            public override SyntaxNode VisitDoStatement(DoStatementSyntax node)
            {
                node = (DoStatementSyntax)base.VisitDoStatement(node);

                if (!node.Statement.IsKind(SyntaxKind.Block))
                {
                    this.addedAnnotations = true;
                    node = node.WithStatement(SyntaxFactory.Block(node.Statement));
                }

                return node;
            }
        }

        public SyntaxNode Process(SyntaxNode syntaxNode, string languageName)
        {
            var rewriter = new MissingBlockRewriter();
            var newNode = rewriter.Visit(syntaxNode);
            if (!rewriter.AddedAnnotations)
            {
                return syntaxNode;
            }

            return newNode;
        }
    }
}