/*==============================================================================
(c) 2021 BlindEye Softworks. All rights reserved.

This is free software, you can redistribute it and/or modify it under the terms
of the GNU GPL License v3.0 (GPLv3).

You should have received a copy of the GPLv3 license with this source code. If
not, visit https://www.gnu.org/licenses/gpl-3.0.en.html.

If you have any questions in regards to this license you may contact:
licensing@blindeyesoftworks.com
===============================================================================*/

using System;
using System.Text;

namespace BlindEye.NaturalLanguageGeneration
{
    /// <summary>
    /// Class containing methods for performing natural language generation.
    /// </summary>
    public static class NaturalLanguageGenerator
    {
        #region Fields
        /* Not to be confused with the duodecimal number system, this is simply
           a derivation. */
        private static readonly string[][] baseLabels = new string[][]
        {             // Base:      Affix:  Modifier:
            new string[] {"zero",   null,   null},
            new string[] {"one",    null,   null},
            new string[] {"two",    "twen", null},
            new string[] {"three",  "thir", null},
            new string[] {"four",   null,   "for"},
            new string[] {"five",   "fif",  null},
            new string[] {"six",    null,   null},
            new string[] {"seven",  null,   null},
            new string[] {"eight",  "eigh", null},
            new string[] {"nine",   null,   null},
            new string[] {"ten",    null,   null},
            new string[] {"eleven", null,   null},
            new string[] {"twelve", null,   null}
        };

        /* ---------------------------------------------------------------------
           Below is a zero-based chart of the range of indicies for each
           significant grouping named using the short scale naming system. Not
           to be confused with FIGURE 1A below.
           ---------------------------------------------------------------------
           FIGURE 1A:
           1,000,000 | POS: | Group:
           | ||| |||__ 0      Hundreds
           | ||| ||___ 1      Hundreds
           | ||| |____ 2      Hundreds
           | |||______ 3      Thousands
           | ||_______ 4      Thousands
           | |________ 5      Thousands
           |__________ 6      Millions
           ---------------------------------------------------------------------
           | SCIENTIFIC NOTATION:                                              |
           ---------------------------------------------------------------------
           The chart can also be used to easily map each name to its short scale
           and long scale scientific notation representation. To convert to
           short scale scientific notation the expression would be 10^x.
           ---------------------------------------------------------------------
           EXAMPLE:
           Trillion = 10^x
           WHERE:
           x = MIN
           SUBSTITUTED:
           Trillion = 10^12
           ---------------------------------------------------------------------
           To convert to long scale scientific notation the expression would be
           10^6x, 10^6x+3.
           ---------------------------------------------------------------------
           EXAMPLE:
           Octillion = 10^6x+3
           WHERE:
           x = POS-1
           POS = Index
           SUBSTITUTED:
           Octillion = 10^48
           ---------------------------------------------------------------------
           To convert to E-notation the the expression would be E+MIN, or
           E+(MIN + 1) if MIN is equal to zero.
           ---------------------------------------------------------------------
           EXAMPLE:
           Million = 10E+MIN
           WHERE:
           MIN = 6
           SUBSTITUTED:
           Million = 10E+6
           OR:
           Billion = 1000x10E+6
           ---------------------------------------------------------------------
           | GROUPINGS:                                                        |
           ---------------------------------------------------------------------
           Furthermore we can also detect the grouping name from a digits
           placement using the floor function (POS / 3) - ((POS / 3) mod 1) to
           retrieve a proper quotient rounded down to the nearest whole number.
           ---------------------------------------------------------------------
           EXAMPLE:
           1,000,000
              |_____ (POS / 3) - ((POS / 3) mod 1)
           WHERE:
           POS = The zero-based index of the digit. See FIGURE 1A.
           SOLVED:
           POS = 1
           var group = shortScale[1] + "s";
           Debug.WriteLine(group);
           OUTPUT:
           Thousands
           -------------------------------------------------------------------*/
        private static readonly string[] groupLabels = new string[]
        {                            // | MIN | MAX |
                                     // |-----------|
            "hundred",               // | 000 - 002 |
            "thousand",              // | 003 - 005 |
            "million",               // | 006 - 008 |
            "billion",               // | 009 - 011 |
            "trillion",              // | 012 - 014 |
            "quadrillion",           // | 015 - 017 |
            "quintillion",           // | 018 - 020 |
            "sextillion",            // | 021 - 023 |
            "septillion",            // | 024 - 026 |
            "octillion",             // | 027 - 029 |
            "nonillion",             // | 030 - 032 |
            "decillion",             // | 033 - 035 |
            "undecillion",           // | 036 - 038 |
            "duodecillion",          // | 039 - 041 |
            "tredecillion",          // | 042 - 044 |
            "quattuordecillion",     // | 045 - 047 |
            "quindecillion",         // | 048 - 050 |
            "sexdecillion",          // | 051 - 053 |
            "septendecillion",       // | 054 - 056 |
            "octodecillion",         // | 057 - 059 |
            "novemdecillion",        // | 060 - 062 |
            "vigintillion",          // | 063 - 065 |
            "unvigintillion",        // | 066 - 068 |
            "duovigintillion",       // | 069 - 071 |
            "trevigintillion",       // | 072 - 074 |
            "quattuorvigintillion",  // | 075 - 077 |
            "quinvigintillion",      // | 078 - 080 |
            "sexvigintillion",       // | 081 - 083 |
            "septenvigintillion",    // | 084 - 086 |
            "octovigintillion",      // | 087 - 089 |
            "novemvigintillion",     // | 090 - 092 |
            "trigintillion",         // | 093 - 095 |
            "untrigintillion",       // | 096 - 098 |
            "duotrigintillion",      // | 099 - 101 |
            "googol",                // | 102 - 104 |
            "tretrigintillion",      // | 105 - 107 |
            "quattuortrigintillion", // | 108 - 110 |
            "quintrigintillion",     // | 111 - 113 |
            "sextrigintillion",      // | 114 - 116 |
            "septentrigintillion",   // | 117 - 119 |
            "octotrigintillion",     // | 120 - 122 |
            "novemtrigintillion",    // | 123 - 125 |
            "centillion"             // | 126 - 128 |
        };

        /* Derivational bound morphemes to assist in augmentative derivative
           generation. */
        private static readonly string[] baseSuffixes = new string[]
        {
            "teen", /* Translates to "combining form of ten" in Old English.
                       Used to form cardinal numbers from 13-19. */
            "ty"    // Translates to "multiples of ten" in Middle English.
        };
        #endregion

        /// <summary>
        /// Returns a string containing the text-based representation of
        /// <paramref name="source"/>.
        /// </summary>
        /// <param name="source">
        /// The source text to translate.
        /// </param>
        /// <returns>
        /// A string containing the text-based representation of
        /// <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is null, empty, or
        /// only consisting of whitespace characters.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown when <paramref name="source"/> has an incorrect numerical
        /// structure.
        /// </exception>
        public static string GenerateText(string source)
        {
            // Best done outside of the preprocessor to avoid generic messages.
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException(
                    message: "Value cannot be null or empty.",
                    paramName: nameof(source));

            if (!CanGenerate(ref source, out StringBuilder buffer))
                throw new FormatException();

            NumericalRealization(source, ref buffer);

            return buffer.ToString();
        }

        /// <summary>
        /// Returns a value indicating whether text can be generated.
        /// </summary>
        /// <param name="source">
        /// The source text to analyze.
        /// </param>
        /// <returns>
        /// True if <paramref name="source"/> is of a proper format;
        /// otherwise, false.
        /// </returns>
        public static bool CanGenerate(string source)
            => CanGenerate(ref source, out _);

        #region Generation Methods
        private static bool CanGenerate(ref string source, out StringBuilder buffer)
        {
            Preprocess(ref source, out buffer);

            /* Faster and more memory efficient to check for conformancy AOT
               instead of having the parser check for illegal characters while
               writing to the buffer. Not much of a penalty since the max
               iterations possible is 130 when the sequence is signed. */
            return !IsMalformed(source);
        }

        private static void NumericalRealization(string source, ref StringBuilder buffer)
        {
            // Scanning from LTR yields a reduced cyclomatic complexity.
            for (int i = 0; i < source.Length; i++)
            {
                /* Typical lexical analysis is better with a state machine so
                   we can peek, look behind, and get the next character but
                   the problem is storing static states reduces thread safety so
                   here we will emulate those missing features. */
                char previous = i - 1 >= 0 ? source[i - 1] : char.MinValue,
                     mostPrevious = i - 2 >= 0 ? source[i - 2] : char.MinValue;
                int group = GetGroup(i, source.Length),
                    placement = GetPlacement(i, source.Length);
                string baseLabel = ResolveBase(previous, source[i], placement);

                if (baseLabel != string.Empty)
                    if (placement == 1)
                        baseLabel = MorphologizeMostSignificant(baseLabel, i);
                    else if (placement == 2)
                        baseLabel = MorphologizeMidpoint(baseLabel, previous);
                    else
                        baseLabel = MorphologizeLeastSignificant(baseLabel, mostPrevious,
                            previous, source[i]);

                if (group != 0 && placement == 3 && !IsEmptyGroup(mostPrevious, previous,
                    source[i]))
                    buffer.Append(baseLabel + ' ' + GetGroupLabel(group));
                else if (buffer.Length != 0 && (baseLabel != string.Empty &&
                    baseLabel[0] != '-'))
                    buffer.Append(' ' + baseLabel);
                else
                    buffer.Append(baseLabel);
            }
        }

        private static string MorphologizeMostSignificant(string baseLabel, int index)
            => baseLabel + ' ' + GetGroupLabel(0);

        private static string MorphologizeMidpoint(string baseLabel, char previous)
            => previous != char.MinValue ? "and " + baseLabel : baseLabel;

        private static string MorphologizeLeastSignificant(string baseLabel,
            char mostPrevious, char previous, char current)
        {
            int previousDigit = CharToInt(previous),
                currentDigit = CharToInt(current);

            if (previous != char.MinValue)
                if ((previousDigit == 1 || previousDigit == 0)
                    && mostPrevious != char.MinValue)
                    return "and " + baseLabel;
                else if (previousDigit > 1)
                    return '-' + baseLabel;

            return baseLabel;
        }

        private static string ResolveBase(char previous, char current, int placement)
        {
            int integer = CharToInt(current);

            if (placement == 1)
                return LexicalizeMostSignificant(integer);

            if (placement == 2)
                return LexicalizeMidpoint(integer);

            return LexicalizeLeastSignificant(previous, integer);
        }

        private static string LexicalizeMostSignificant(int integer)
            => integer != 0 ? GetBaseLabel(integer) : string.Empty;

        private static string LexicalizeMidpoint(int integer)
        {
            /* Base resolution will be determined by the least significant
               digit. */
            if (integer < 2)
                return string.Empty;

            string baseLabel;

            if (HasBaseModifier(integer))
                baseLabel = GetBaseModifier(integer);
            else if (HasBaseAffix(integer))
                baseLabel = GetBaseAffix(integer);
            else
                baseLabel = GetBaseLabel(integer);

            return baseLabel + baseSuffixes[1];
        }

        private static string LexicalizeLeastSignificant(char previous, int integer)
        {
            int previousDigit = CharToInt(previous);

            if (previous != char.MinValue && previousDigit != 1 && integer == 0)
                return string.Empty;

            if (previousDigit < 1 || previousDigit > 1)
                return GetBaseLabel(integer);

            if (integer < 3)
                return GetBaseLabel(10 + integer);

            string baseLabel = HasBaseAffix(integer) ?
                GetBaseAffix(integer) : GetBaseLabel(integer);

            return baseLabel + baseSuffixes[0];
        }
        #endregion

        #region Preprocessing Methods
        private static void Preprocess(ref string source, out StringBuilder buffer)
        {
            /* This method serves as the primary language preprocessor. Not the
               best setup but helps mitigate additional class dependencies. */
            source = source.Trim();
            IdentifySign(ref source, out buffer);
            RemoveLeadingZeros(ref source);
        }

        private static void IdentifySign(ref string source, out StringBuilder buffer)
        {
            buffer = new StringBuilder(groupLabels.Length * 3);

            /* SNAFU, later preprocessing should be responsible for detecting
               character sequences that are FUBAR. */
            if (source[0] == '-')
                buffer.Append("negative");
            else if (source[0] == '+')
                buffer.Append("positive");

            if (buffer.Length > 0)
                source = source.Remove(0, 1);
        }

        private static void RemoveLeadingZeros(ref string source)
        {
            int endPos = 0;

            while (endPos < source.Length - 1 && source[endPos] == '0')
                endPos++;

            if (endPos != 0)
                source = source.Remove(0, endPos);
        }

        private static bool IsMalformed(string source)
        {
            // This guard clause should always remain penultimate.
            if (source.Length > groupLabels.Length * 3)
                return true;

            for (int i = 0; i < source.Length; i++)
                if (!char.IsDigit(source[i]))
                    return true;

            return false;
        }
        #endregion

        #region Helper Methods
        private static int InvertIndex(int index, int length)
            => (length - 1) - index;

        private static int CharToInt(char c)
            => c - 48;

        private static int GetGroup(int index, int length)
            => InvertIndex(index, length) / 3;

        private static string GetGroupLabel(int group)
            => groupLabels[group];

        private static string GetBaseLabel(int integer)
            => baseLabels[integer][0];

        private static string GetBaseAffix(int integer)
            => baseLabels[integer][1];

        private static string GetBaseModifier(int integer)
            => baseLabels[integer][2];

        private static bool HasBaseAffix(int integer)
            => baseLabels[integer][1] != null;

        private static bool HasBaseModifier(int integer)
            => baseLabels[integer][2] != null;

        private static int GetPlacement(int index, int length)
        {
            // Returns the digit placement relative to its group.
            if ((length - index) % 3 == 0)
                return 1; // Most significant.
            else if (((length - index) + 1) % 3 == 0)
                return 2; // Mid-point.

            return 3; // Least significant.
        }

        private static bool IsEmptyGroup(char a, char b, char c)
            => a == 48 && b == a && c == b;
        #endregion
    }
}
