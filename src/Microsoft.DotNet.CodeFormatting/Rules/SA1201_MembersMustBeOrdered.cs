namespace Microsoft.DotNet.CodeFormatting.Rules
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [SyntaxRuleOrder(SyntaxRuleOrder.MembersMustBeOrderedRule)]
    internal sealed class SA1201_MembersMustBeOrdered : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        private sealed class OrderClassMembersRewriter : CSharpSyntaxRewriter
        {
            internal OrderClassMembersRewriter()
            {
            }

            public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
            {
                var sortedMembers = node.Members.OrderBy(x => x, new ClassMemberComparer());

                return node.WithMembers(SyntaxFactory.List(sortedMembers));
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                var sortedMembers = node.Members.OrderBy(x => x, new ClassMemberComparer());

                return node.WithMembers(SyntaxFactory.List(sortedMembers));
            }

            public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
            {
                var sortedMembers = node.Members.OrderBy(x => x, new ClassMemberComparer());

                return node.WithMembers(SyntaxFactory.List(sortedMembers));
            }
        }

        internal sealed class ClassMemberComparer : IComparer<MemberDeclarationSyntax>
        {
            public int Compare(MemberDeclarationSyntax x, MemberDeclarationSyntax y)
            {
                return GetRelativeOrder(x).CompareTo(GetRelativeOrder(y));
            }

            private static int GetRelativeOrder(MemberDeclarationSyntax member)
            {
                int baseSortingNumber = 0;

                switch (member.Kind())
                {
                case SyntaxKind.FieldDeclaration:
                    baseSortingNumber = 10;
                    break;
                case SyntaxKind.ConstructorDeclaration:
                    baseSortingNumber = 20;
                    break;
                case SyntaxKind.DestructorDeclaration:
                    baseSortingNumber = 30;
                    break;
                case SyntaxKind.EnumDeclaration:
                    baseSortingNumber = 40;
                    break;
                case SyntaxKind.DelegateDeclaration:
                    baseSortingNumber = 50;
                    break;
                case SyntaxKind.EventFieldDeclaration:
                    baseSortingNumber = 60;
                    break;
                case SyntaxKind.PropertyDeclaration:
                    baseSortingNumber = 70;
                    break;
                case SyntaxKind.IndexerDeclaration:
                    baseSortingNumber = 80;
                    break;
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.OperatorDeclaration:
                    baseSortingNumber = 90;
                    break;
                case SyntaxKind.ClassDeclaration:
                    baseSortingNumber = 100;
                    break;
                // TODO: Struct
                case SyntaxKind.StructDeclaration:
                    baseSortingNumber = 110;
                    break;
                // TODO: Interface
                case SyntaxKind.InterfaceDeclaration:
                    baseSortingNumber = 120;
                    break;
                default:
                    return int.MaxValue;
                }

                return GetSortingNumber(member, baseSortingNumber);
            }

            private static int GetSortingNumber(MemberDeclarationSyntax member, int baseNumber)
            {
                return baseNumber + ClassifyStatic(member) + ClassifyAccessibility(member);
            }

            private static int ClassifyAccessibility(MemberDeclarationSyntax member)
            {
                SyntaxTokenList list;

                if (member is BaseMethodDeclarationSyntax)
                {
                    list = ((BaseMethodDeclarationSyntax)member).Modifiers;
                }
                else if (member is BasePropertyDeclarationSyntax)
                {
                    list = ((BasePropertyDeclarationSyntax)member).Modifiers;
                }
                else if (member is BaseTypeDeclarationSyntax)
                {
                    list = ((BaseTypeDeclarationSyntax)member).Modifiers;
                }
                else if (member is BaseFieldDeclarationSyntax)
                {
                    list = ((BaseFieldDeclarationSyntax)member).Modifiers;
                }
                else if (member is DelegateDeclarationSyntax)
                {
                    list = ((DelegateDeclarationSyntax)member).Modifiers;
                }
                else
                {
                    return 0;
                }

                int modifierSum = 1;
                var hasModifier = false;

                foreach (var token in list)
                {
                    if (token.IsKind(SyntaxKind.PrivateKeyword))
                    {
                        hasModifier = true;
                        modifierSum += 8;
                    } 
                    else if (token.IsKind(SyntaxKind.ProtectedKeyword))
                    {
                        hasModifier = true;
                        modifierSum += 6;
                    }
                    else if (token.IsKind(SyntaxKind.InternalKeyword))
                    {
                        hasModifier = true;
                        modifierSum += 4;
                    }
                    else if (token.IsKind(SyntaxKind.PublicKeyword))
                    {
                        hasModifier = true;
                        modifierSum += 2;
                    }
                    else if (token.IsKind(SyntaxKind.StaticKeyword))
                    {
                        modifierSum += -1;
                    }
                }

                if (!hasModifier)
                {
                    // internal access
                    modifierSum += 4;
                }

                // public
                // internal
                // protected
                // private
                return modifierSum;
            }

            public static int ClassifyStatic(MemberDeclarationSyntax member)
            {
                // static
                // non-static
                return 0;
            }
        }

        public SyntaxNode Process(SyntaxNode syntaxNode, string languageName)
        {
            var rewriter = new OrderClassMembersRewriter();
            var newNode = rewriter.Visit(syntaxNode);
            return newNode;
        }
    }
}