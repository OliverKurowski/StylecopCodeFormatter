// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    //[SyntaxRuleOrder(SyntaxRuleOrder.CopyrightHeaderRule)]
    internal sealed partial class CopyrightHeaderRule : SyntaxFormattingRule, ISyntaxFormattingRule
    {
        private abstract class CommonRule
        {
            /// <summary>
            /// This is the normalized copyright header that has no comment delimeters.
            /// </summary>
            private readonly ImmutableArray<string> _header;

            protected CommonRule(ImmutableArray<string> header)
            {
                _header = header;
            }

            internal SyntaxNode Process(SyntaxNode syntaxNode)
            {
                if (_header.IsDefaultOrEmpty)
                {
                    return syntaxNode;
                }

                if (HasCopyrightHeader(syntaxNode))
                    return syntaxNode;

                return AddCopyrightHeader(syntaxNode);
            }

            private bool HasCopyrightHeader(SyntaxNode syntaxNode)
            {
                var existingHeader = GetExistingHeader(syntaxNode.GetLeadingTrivia());
                return _header.SequenceEqual(existingHeader);
            }

            private SyntaxNode AddCopyrightHeader(SyntaxNode syntaxNode)
            {
                var list = new List<SyntaxTrivia>();
                foreach (var headerLine in _header)
                {
                    list.Add(CreateLineComment(headerLine));
                    list.Add(CreateNewLine());
                }
                list.Add(CreateNewLine());

                var triviaList = RemoveExistingHeader(syntaxNode.GetLeadingTrivia());
                var i = 0;
                MovePastBlankLines(triviaList, ref i);

                while (i < triviaList.Count)
                {
                    list.Add(triviaList[i]);
                    i++;
                }

                return syntaxNode.WithLeadingTrivia(CreateTriviaList(list));
            }

            private List<string> GetExistingHeader(SyntaxTriviaList triviaList)
            {
                var i = 0;
                MovePastBlankLines(triviaList, ref i);

                var headerList = new List<string>();
                while (i < triviaList.Count && IsLineComment(triviaList[i]))
                {
                    headerList.Add(GetCommentText(triviaList[i].ToFullString()));
                    i++;
                    MoveToNextLineOrTrivia(triviaList, ref i);
                }

                return headerList;
            }

            /// <summary>
            /// Remove any copyright header that already exists.
            /// </summary>
            private SyntaxTriviaList RemoveExistingHeader(SyntaxTriviaList oldList)
            {
                var foundHeader = false;
                var i = 0;
                MovePastBlankLines(oldList, ref i);

                while (i < oldList.Count && IsLineComment(oldList[i]))
                {
                    if (oldList[i].ToFullString().IndexOf("copyright", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        foundHeader = true;
                    }
                    i++;
                }

                if (!foundHeader)
                {
                    return oldList;
                }

                MovePastBlankLines(oldList, ref i);
                return CreateTriviaList(oldList.Skip(i));
            }

            private void MovePastBlankLines(SyntaxTriviaList list, ref int index)
            {
                while (index < list.Count && (IsWhitespace(list[index]) || IsNewLine(list[index])))
                {
                    index++;
                }
            }
            
            private void MoveToNextLineOrTrivia(SyntaxTriviaList list, ref int index)
            {
                MovePastWhitespaces(list, ref index);

                if (index < list.Count && IsNewLine(list[index]))
                {
                    index++;
                }
            }

            private void MovePastWhitespaces(SyntaxTriviaList list, ref int index)
            {
                while (index < list.Count && IsWhitespace(list[index]))
                {
                    index++;
                }
            }

            protected abstract SyntaxTriviaList CreateTriviaList(IEnumerable<SyntaxTrivia> e);

            protected abstract bool IsLineComment(SyntaxTrivia trivia);

            protected abstract bool IsWhitespace(SyntaxTrivia trivia);
            protected abstract bool IsNewLine(SyntaxTrivia trivia);

            protected abstract SyntaxTrivia CreateLineComment(string commentText);

            protected abstract SyntaxTrivia CreateNewLine();
        }

        private readonly Options _options;
        private ImmutableArray<string> _cachedHeader;
        private ImmutableArray<string> _cachedHeaderSource;

        [ImportingConstructor]
        internal CopyrightHeaderRule(Options options)
        {
            _options = options;
        }

        private ImmutableArray<string> GetHeader()
        {
            if (_cachedHeaderSource != _options.CopyrightHeader)
            {
                _cachedHeaderSource = _options.CopyrightHeader;
                _cachedHeader = _options.CopyrightHeader.Select(GetCommentText).ToImmutableArray();
            }

            return _cachedHeader;
        }

        private static string GetCommentText(string line)
        {
            if (line.StartsWith("'"))
            {
                return line.Substring(1).TrimStart();
            }

            if (line.StartsWith("//"))
            {
                return line.Substring(2).TrimStart();
            }

            return line;
        }

        public override bool SupportsLanguage(string languageName)
        {
            return languageName == LanguageNames.CSharp || languageName == LanguageNames.VisualBasic;
        }

        public override SyntaxNode ProcessCSharp(SyntaxNode syntaxNode)
        {
            return (new CSharpRule(GetHeader())).Process(syntaxNode);
        }

        public override SyntaxNode ProcessVisualBasic(SyntaxNode syntaxNode)
        {
            return (new VisualBasicRule(GetHeader())).Process(syntaxNode);
        }

    }
}