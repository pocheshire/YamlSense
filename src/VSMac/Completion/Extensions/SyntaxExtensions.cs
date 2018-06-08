using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace YamlSense.VSMac.Completion.Extensions
{
    internal static class SyntaxNodeExtensions
    {
        /// <summary>
        /// Look inside a trivia list for a skipped token that contains the given position.
        /// </summary>
        private static readonly Func<SyntaxTriviaList, int, SyntaxToken> s_findSkippedTokenForward = FindSkippedTokenForward;

        /// <summary>
        /// Look inside a trivia list for a skipped token that contains the given position.
        /// </summary>
        private static SyntaxToken FindSkippedTokenForward(SyntaxTriviaList triviaList, int position)
        {
            foreach (var trivia in triviaList)
            {
                if (trivia.HasStructure)
                {
                    var skippedTokensTrivia = trivia.GetStructure() as ISkippedTokensTriviaSyntax;
                    if (skippedTokensTrivia != null)
                    {
                        foreach (var token in skippedTokensTrivia.Tokens)
                        {
                            if (token.Span.Length > 0 && position <= token.Span.End)
                            {
                                return token;
                            }
                        }
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Look inside a trivia list for a skipped token that contains the given position.
        /// </summary>
        private static readonly Func<SyntaxTriviaList, int, SyntaxToken> s_findSkippedTokenBackward = FindSkippedTokenBackward;

        /// <summary>
        /// Look inside a trivia list for a skipped token that contains the given position.
        /// </summary>
        private static SyntaxToken FindSkippedTokenBackward(SyntaxTriviaList triviaList, int position)
        {
            foreach (var trivia in triviaList.Reverse())
            {
                if (trivia.HasStructure)
                {
                    var skippedTokensTrivia = trivia.GetStructure() as ISkippedTokensTriviaSyntax;
                    if (skippedTokensTrivia != null)
                    {
                        foreach (var token in skippedTokensTrivia.Tokens)
                        {
                            if (token.Span.Length > 0 && token.SpanStart <= position)
                            {
                                return token;
                            }
                        }
                    }
                }
            }

            return default;
        }

        private static SyntaxToken GetInitialToken(
            SyntaxNode root,
            int position,
            bool includeSkipped = false,
            bool includeDirectives = false,
            bool includeDocumentationComments = false)
        {
            return (position < root.FullSpan.End || !(root is ICompilationUnitSyntax))
                ? root.FindToken(position, includeSkipped || includeDirectives || includeDocumentationComments)
                : root.GetLastToken(includeZeroWidth: true, includeSkipped: true, includeDirectives: true, includeDocumentationComments: true)
                      .GetPreviousToken(includeZeroWidth: false, includeSkipped: includeSkipped, includeDirectives: includeDirectives, includeDocumentationComments: includeDocumentationComments);
        }

        /// <summary>
        /// If the position is inside of token, return that token; otherwise, return the token to the right.
        /// </summary>
        public static SyntaxToken FindTokenOnRightOfPosition(
            this SyntaxNode root,
            int position,
            bool includeSkipped = false,
            bool includeDirectives = false,
            bool includeDocumentationComments = false)
        {
            var findSkippedToken = includeSkipped ? s_findSkippedTokenForward : ((l, p) => default);

            var token = GetInitialToken(root, position, includeSkipped, includeDirectives, includeDocumentationComments);

            if (position < token.SpanStart)
            {
                var skippedToken = findSkippedToken(token.LeadingTrivia, position);
                token = skippedToken.RawKind != 0 ? skippedToken : token;
            }
            else if (token.Span.End <= position)
            {
                do
                {
                    var skippedToken = findSkippedToken(token.TrailingTrivia, position);
                    token = skippedToken.RawKind != 0
                        ? skippedToken
                        : token.GetNextToken(includeZeroWidth: false, includeSkipped: includeSkipped, includeDirectives: includeDirectives, includeDocumentationComments: includeDocumentationComments);
                }
                while (token.RawKind != 0 && token.Span.End <= position && token.Span.End <= root.FullSpan.End);
            }

            if (token.Span.Length == 0)
            {
                token = token.GetNextToken();
            }

            return token;
        }

        /// <summary>
        /// If the position is inside of token, return that token; otherwise, return the token to the left.
        /// </summary>
        public static SyntaxToken FindTokenOnLeftOfPosition(
            this SyntaxNode root,
            int position,
            bool includeSkipped = false,
            bool includeDirectives = false,
            bool includeDocumentationComments = false)
        {
            var findSkippedToken = includeSkipped ? s_findSkippedTokenBackward : ((l, p) => default);

            var token = GetInitialToken(root, position, includeSkipped, includeDirectives, includeDocumentationComments);

            if (position <= token.SpanStart)
            {
                do
                {
                    var skippedToken = findSkippedToken(token.LeadingTrivia, position);
                    token = skippedToken.RawKind != 0
                        ? skippedToken
                        : token.GetPreviousToken(includeZeroWidth: false, includeSkipped: includeSkipped, includeDirectives: includeDirectives, includeDocumentationComments: includeDocumentationComments);
                }
                while (position <= token.SpanStart && root.FullSpan.Start < token.SpanStart);
            }
            else if (token.Span.End < position)
            {
                var skippedToken = findSkippedToken(token.TrailingTrivia, position);
                token = skippedToken.RawKind != 0 ? skippedToken : token;
            }

            if (token.Span.Length == 0)
            {
                token = token.GetPreviousToken();
            }

            return token;
        }

        public static T WithPrependedLeadingTrivia<T>(
            this T node,
            params SyntaxTrivia[] trivia) where T : SyntaxNode
        {
            if (trivia.Length == 0)
            {
                return node;
            }

            return node.WithPrependedLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);
        }

        public static T WithPrependedLeadingTrivia<T>(
            this T node,
            SyntaxTriviaList trivia) where T : SyntaxNode
        {
            if (trivia.Count == 0)
            {
                return node;
            }

            return node.WithLeadingTrivia(trivia.Concat(node.GetLeadingTrivia()));
        }

        public static T WithPrependedLeadingTrivia<T>(
            this T node,
            IEnumerable<SyntaxTrivia> trivia) where T : SyntaxNode
        {
            var list = new SyntaxTriviaList();
            list = list.AddRange(trivia);

            return node.WithPrependedLeadingTrivia(list);
        }

        public static T WithAppendedTrailingTrivia<T>(
            this T node,
            params SyntaxTrivia[] trivia) where T : SyntaxNode
        {
            if (trivia.Length == 0)
            {
                return node;
            }

            return node.WithAppendedTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);
        }

        public static T WithAppendedTrailingTrivia<T>(
            this T node,
            SyntaxTriviaList trivia) where T : SyntaxNode
        {
            if (trivia.Count == 0)
            {
                return node;
            }

            return node.WithTrailingTrivia(node.GetTrailingTrivia().Concat(trivia));
        }

        public static T WithAppendedTrailingTrivia<T>(
            this T node,
            IEnumerable<SyntaxTrivia> trivia) where T : SyntaxNode
        {
            var list = new SyntaxTriviaList();
            list = list.AddRange(trivia);

            return node.WithAppendedTrailingTrivia(list);
        }

        public static T With<T>(
            this T node,
            IEnumerable<SyntaxTrivia> leadingTrivia,
            IEnumerable<SyntaxTrivia> trailingTrivia) where T : SyntaxNode
        {
            return node.WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(trailingTrivia);
        }

        private static SyntaxNode GetParent(this SyntaxNode node)
        {
            return node is IStructuredTriviaSyntax trivia ? trivia.ParentTrivia.Token.Parent : node.Parent;
        }

        public static TNode FirstAncestorOrSelfUntil<TNode>(this SyntaxNode node, Func<SyntaxNode, bool> predicate)
            where TNode : SyntaxNode
        {
            for (var current = node; current != null; current = current.GetParent())
            {
                if (current is TNode tnode)
                {
                    return tnode;
                }

                if (predicate(current))
                {
                    break;
                }
            }

            return default;
        }
    }
}