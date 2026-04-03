// Created by: Julian Noel
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IDEK.Tools.ShocktroopExtensions
{
    public static class StringExtensions
    {
        const string NULL_FILLER = "<Null>";
        const string EMPTY_FILLER = "<NONE>";

        /// <summary>
        /// Returns true if this string is null, empty, or contains only whitespace.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <returns><c>true</c> if this string is null, empty, or contains only whitespace; otherwise, <c>false</c>.</returns>
        public static bool IsNullOrWhitespace(this string str)
        {
            if (string.IsNullOrEmpty(str)) return true;
            
            for (int index = 0; index < str.Length; ++index)
            {
                if (!char.IsWhiteSpace(str[index]))
                    return false;
            }

            return true;
        }
        
        /// <summary>
        /// Replaces the string if null or empty. Defaults to "&lt;Null&gt;" and "&lt;NONE&gt;".
        /// </summary>
        /// <param name="text"></param>
        /// <param name="emptyText"></param>
        /// <param name="nullText"></param>
        /// <returns></returns>
        public static string BackFillString(this string text, string emptyText=EMPTY_FILLER, string nullText= NULL_FILLER)
        {
            if(text == null)
            {
                return nullText;
            }
            else if(text.NullIfEmpty() == null)
            {
                return emptyText;
            }
            else
            {
                return text;
            }
        }

        public static string SplitCamelCase(this string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        public static string NullIfEmpty(this string s)
        {
            if(s == string.Empty)
            {
                return null;
            }
            else
            {
                return s;
            }
        }

        public static string ToHexString(this byte b) => b.ToString("X2");
        public static string ToHexString(this short b) => b.ToString("X4");
        public static string ToHexString(this int b) => b.ToString("X8");
        public static string ToHexString(this float b) => b.ToString("X8");
        public static string ToHexString(this long b) => b.ToString("X16");
        public static string ToHexString(this double b) => b.ToString("X16");

        public static string ToEnumeratedString<T>(this IEnumerable<T> container, Func<T, string> tostringCallback, int elementsPerLine = 1, bool forceOneLine = false, string indentationString = "", bool recursive = false)
        {
            bool isOneLine = forceOneLine || container.Count() <= elementsPerLine;
            string startPrefix = isOneLine ? "" : indentationString;
            string fullContentsString = startPrefix + container.ToString() + ": [";

            int currentItem = 0;
            //not all IEnumerables are indexable, so using a for-loop would be awkward.
            foreach(T item in container)
            {
                string conditionalNewLine = (!isOneLine && (elementsPerLine < 1 || currentItem % elementsPerLine == 0)) ?
                    ("\n" + indentationString + "\t")
                    : "";

                string elementString;
                if(recursive && item is IEnumerable e && item is not string)
                {
                    elementString = e.Cast<object>().ToEnumeratedString(elementsPerLine, forceOneLine, indentationString + "\t", recursive);
                }
                else
                {
                    elementString = tostringCallback?.Invoke(item) ?? "NULL CALLBACK";
                }

                fullContentsString += conditionalNewLine + elementString + ", ";

                currentItem++;
            }

            string endPrefix = isOneLine ? "" : "\n" + indentationString;
            string endSuffix = isOneLine ? "" : "\n";
            fullContentsString += endPrefix + "]" + endSuffix;

            return fullContentsString;
        }


        public static string ToEnumeratedString<T>(this IEnumerable<T> container, int elementsPerLine = 1, bool forceOneLine = false, string indentationString = "", bool recursive = false)
        {
            return container?.ToEnumeratedString(item => item?.ToString() ?? "NULL", elementsPerLine, forceOneLine, indentationString, recursive) ?? "NULL";
        }

        public static string FormatAsElementOfEnglishList(string elementName, int listSize, int currentIndex, string finalPunctuation=".")
        {
            bool isCommaSeparatedList = listSize >= 3; //grammar 101: only use commas for lists with 3 or more elements
            bool moreThanOneListElement = listSize > 1;
            bool isLastIndex = currentIndex >= listSize - 1; // last index of a list of elements is handled different

            //if the last element in a list (so there's more than one button), start with the final "and "
            // as in A, B, and C
            string startOfPhrase = (isLastIndex && moreThanOneListElement) ? "and " : "";

            //end of list is the final punctuation (currently finalPunctuation). 
            //
            //Per English grammer, elements before the last only use commas 
            //if the list contains 3 or more elements: "A, B, and C!" 
            //(for the sake of code simplicity we are observing the Oxford Comma)
            //
            //A list of length two is just: "A and B!"
            string endOfPhrase = isLastIndex ? 
                finalPunctuation 
                : isCommaSeparatedList ? 
                    ", " : " ";

            return startOfPhrase + elementName + endOfPhrase;
        }

        //TODO: add flag for Oxford Comma
        /// <summary>
        /// Generates a string listing the enumerable's elements in grammatically-correct English.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns>the complete list phrase.</returns>
        public static string ToEnglishListString<T>(this IEnumerable<T> elements, Func<T, string> elementPreFormatter)
        {
            var iterator = elements.GetEnumerator();
            int count = elements.Count();
            string sentence = "";

            for(int i = 0; iterator.MoveNext(); i++)
            {
                var currentElement = elementPreFormatter != null ?
                    elementPreFormatter(iterator.Current)
                    : iterator.Current.ToString();

                sentence += FormatAsElementOfEnglishList(currentElement, count, i);
            }

            return sentence;
        }
    }
}
