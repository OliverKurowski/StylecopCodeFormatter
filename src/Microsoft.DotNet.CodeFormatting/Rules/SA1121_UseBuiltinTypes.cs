namespace Microsoft.DotNet.CodeFormatting.Rules
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    //[LocalSemanticRuleOrder(LocalSemanticRuleOrder.UseBuiltinTypesRule)]
    internal sealed class SA1121_UseBuiltinTypes : CSharpOnlyFormattingRule, ILocalSemanticFormattingRule
    {
        private sealed class BuiltinTypesRewriter : CSharpSyntaxRewriter
        {
            private static Dictionary<SpecialType, SyntaxKind> replacements = new Dictionary<SpecialType, SyntaxKind>
            {
                { SpecialType.System_Byte, SyntaxKind.ByteKeyword },
                { SpecialType.System_SByte, SyntaxKind.SByteKeyword },
                { SpecialType.System_String, SyntaxKind.StringKeyword },
                { SpecialType.System_Char, SyntaxKind.CharKeyword },
                { SpecialType.System_Int16, SyntaxKind.ShortKeyword },
                { SpecialType.System_UInt16, SyntaxKind.UShortKeyword },
                { SpecialType.System_Int32, SyntaxKind.IntKeyword },
                { SpecialType.System_UInt32, SyntaxKind.UIntKeyword },
                { SpecialType.System_Int64, SyntaxKind.LongKeyword },
                { SpecialType.System_UInt64, SyntaxKind.ULongKeyword },
                { SpecialType.System_Single, SyntaxKind.FloatKeyword },
                { SpecialType.System_Double, SyntaxKind.DoubleKeyword },
                { SpecialType.System_Decimal, SyntaxKind.DecimalKeyword },
                { SpecialType.System_Object, SyntaxKind.ObjectKeyword },
                { SpecialType.System_Void, SyntaxKind.VoidKeyword },
                { SpecialType.System_Boolean, SyntaxKind.BoolKeyword }
            };

            private readonly Document document;

            private readonly CancellationToken cancellationToken;

            private SemanticModel semanticModel;

            private bool addedAnnotations;

            internal bool AddedAnnotations
            {
                get
                {
                    return this.addedAnnotations;
                }
            }

            internal BuiltinTypesRewriter(Document document, CancellationToken cancellationToken)
            {
                this.document = document;
                this.cancellationToken = cancellationToken;
            }

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (node.IsVar)
                {
                    return base.VisitIdentifierName(node);
                }

                if (this.semanticModel == null)
                {
                    this.semanticModel = this.document.GetSemanticModelAsync(this.cancellationToken).Result;
                }

                var symbolInfo = this.semanticModel.GetSymbolInfo(node, this.cancellationToken);

                if (symbolInfo.Symbol == null)
                {
                    return base.VisitIdentifierName(node);
                }

                if (symbolInfo.Symbol.Kind != SymbolKind.NamedType)
                {
                    return base.VisitIdentifierName(node);
                }

                var name = (INamedTypeSymbol)symbolInfo.Symbol;

                if (replacements.ContainsKey(name.SpecialType))
                {
                    this.addedAnnotations = true;

                    if (node.Parent.IsKind(SyntaxKind.QualifiedName))
                    {
                        return base.VisitIdentifierName(node);
                    }
                    else
                    {
                        return SyntaxFactory
                            .PredefinedType(SyntaxFactory.Token(replacements[name.SpecialType]))
                            .WithTriviaFrom(node);
                    }
                }

                return base.VisitIdentifierName(node);
            }
        }

        public async Task<SyntaxNode> ProcessAsync(Document document, SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            var rewriter = new BuiltinTypesRewriter(document, cancellationToken);
            var newNode = rewriter.Visit(syntaxNode);
            if (!rewriter.AddedAnnotations)
            {
                return syntaxNode;
            }

            return newNode;
        }
    }
}