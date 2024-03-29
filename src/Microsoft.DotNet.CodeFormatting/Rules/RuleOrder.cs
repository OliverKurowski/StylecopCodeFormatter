﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    // Please keep these values sorted by number, not rule name.    
    internal static class SyntaxRuleOrder
    {
        public const int HasNoCustomCopyrightHeaderFormattingRule = 1;
        public const int CopyrightHeaderRule = 2;
        public const int UsingLocationFormattingRule = 3;
        public const int UsingMustBeOrderedRule = 4;
        public const int NewLineAboveFormattingRule = 5;
        public const int BraceNewLineRule = 6;
        public const int NonAsciiCharactersAreEscapedInLiteralsRule = 7;
        public const int NoEmptyStringLiteralRule = 8;
        public const int IfNeedsBlockStatementRule = 9;
        public const int NoWhitespaceBeforeBracketRule = 10;
        public const int NoRegionsAllowedRule = 11; 
        public const int MembersMustBeOrderedRule = 12;
        public const int CommentMustStartWithSpaceRule = 13; 
    }

    // Please keep these values sorted by number, not rule name.    
    internal static class LocalSemanticRuleOrder
    {
        public const int HasNoIllegalHeadersFormattingRule = 1;
        public const int ExplicitVisibilityRule = 2;
        public const int RemoveExplicitThisRule = 3;
        public const int IsFormattedFormattingRule = 4;
        public const int UseBuiltinTypesRule = 5;
    }

    // Please keep these values sorted by number, not rule name.    
    internal static class GlobalSemanticRuleOrder
    {
        public const int PrivateFieldNamingRule = 1;

    }
}
