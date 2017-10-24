using System;
using System.Collections.Generic;
using System.Text;

namespace SentencesFuzzyComparison {
    public class FuzzyComparer {

        public double ThresholdSentence { get; private set; }

        public double ThresholdWord { get; private set; }

        public int MinWordLength { get; private set; }

        public int SubtokenLength { get; private set; }

        public FuzzyComparer(double aThresholdSentence = 0.25, double aThresholdWord = 0.45, int aMinWordLength = 3, int aSubtokenLength = 2) {
            if (aThresholdSentence <= 0) {
                throw new ArgumentException("A threshhold for sentence can not be less than or equal to 0.");
            }

            if (aMinWordLength <= 0) {
                throw new ArgumentException("A word length can not be less than or equal to 0.");
            }

            if (aSubtokenLength <= 0) {
                throw new ArgumentException("A subtoken length can not be less than or equal to 0.");
            }

            if (aSubtokenLength > aMinWordLength) {
                throw new ArgumentException("A subtoken length can not be larger than MinWordLength.");
            }
            
            ThresholdSentence = aThresholdSentence;
            ThresholdWord = aThresholdWord;
            MinWordLength = aMinWordLength;
            SubtokenLength = aSubtokenLength;
        }

        public bool IsFuzzyEqual(string first, string second) {
            return ThresholdSentence <= CalculateFuzzyEqualValue(first, second);
        }

        public double CalculateFuzzyEqualValue(string first, string second) {
            if (string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(second)) {
                return 1.0;
            }

            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second)) {
                return 0.0;
            }

            var normalizedFirst = NormalizeSentence(first);
            var normalizedSecond = NormalizeSentence(second);

            var tokensFirst = GetTokens(normalizedFirst);
            var tokensSecond = GetTokens(normalizedSecond);

            var fuzzyEqualsTokens = GetFuzzyEqualsTokens(tokensFirst, tokensSecond);

            var equalsCount = fuzzyEqualsTokens.Length;
            var firstCount = tokensFirst.Length;
            var secondCount = tokensSecond.Length;

            var resultValue = (1.0 * equalsCount) / (firstCount + secondCount - equalsCount);

            return resultValue;
        }

        private string[] GetFuzzyEqualsTokens(string[] tokensFirst, string[] tokensSecond) {
            var equalsTokens = new List<string>();
            var usedToken = new bool[tokensSecond.Length];
            for (var i = 0; i < tokensFirst.Length; ++i) {
                for (var j = 0; j < tokensSecond.Length; ++j) {
                    if (!usedToken[j]) {
                        if (IsTokensFuzzyEqual(tokensFirst[i], tokensSecond[j])) {
                            equalsTokens.Add(tokensFirst[i]);
                            usedToken[j] = true;
                            break;
                        }
                    }
                }
            }

            return equalsTokens.ToArray();
        }

        private bool IsTokensFuzzyEqual(string firstToken, string secondToken) {
            var equalSubtokensCount = 0;
            var usedTokens = new bool[secondToken.Length - SubtokenLength + 1];
            for (var i = 0; i < firstToken.Length - SubtokenLength + 1; ++i) {
                var subtokenFirst = firstToken.Substring(i, SubtokenLength);
                for (var j = 0; j < secondToken.Length - SubtokenLength + 1; ++j) {
                    if (!usedTokens[j]) {                        
                        var subtokenSecond = secondToken.Substring(j, SubtokenLength);
                        if (subtokenFirst.Equals(subtokenSecond)) {
                            equalSubtokensCount++;
                            usedTokens[j] = true;
                            break;
                        }
                    }                    
                }
            }

            var subtokenFirstCount = firstToken.Length - SubtokenLength + 1;
            var subtokenSecondCount = secondToken.Length - SubtokenLength + 1;

            var tanimoto = (1.0 * equalSubtokensCount) / (subtokenFirstCount + subtokenSecondCount - equalSubtokensCount);

            return ThresholdWord <= tanimoto; 
        }

        private string[] GetTokens(string sentence) {
            var tokens = new List<string>();
            var words = sentence.Split(' ');
            foreach (var word in words) {
                if (word.Length >= MinWordLength) {
                    tokens.Add(word);
                }
            }

            return tokens.ToArray();
        }

        private string NormalizeSentence(string sentence) {
            var resultContainer = new StringBuilder(100);
            var lowerSentece = sentence.ToLower();
            foreach (var c in lowerSentece) {
                if (IsNormalChar(c)) {
                    resultContainer.Append(c);
                }
            }

            return resultContainer.ToString();
        }

        private bool IsNormalChar(char c) {
            return char.IsLetterOrDigit(c) || c == ' ';
        }
    }
}
