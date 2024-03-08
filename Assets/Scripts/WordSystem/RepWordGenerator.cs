using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEPL {

    // Stores the number of times to repeat a word, and the count of how many
    // words should be repeated that many times.
    public class RepCnt {
        public int rep;
        public int count;

        public RepCnt(int newRep, int newCount) {
            rep = newRep;
            count = newCount;
        }
    }

    // e.g. new RepCounts(3,6).RepCnt(2,3).RepCnt(1,3);
    // Specifies 3 repeats of 6 words, 2 repeats of 3 words, 1 instance of 3 words.
    public class RepCounts : List<RepCnt> {
        public RepCounts() { }

        public RepCounts(int rep, int count) {
            RepCnt(rep, count);
        }

        public RepCounts RepCnt(int rep, int count) {
            Add(new RepCnt(rep, count));
            return this;
        }

        public int TotalWords() {
            int total = 0;
            foreach (var r in this) {
                total += r.rep * r.count;
            }
            return total;
        }

        public int UniqueWords() {
            int total = 0;
            foreach (var r in this) {
                total += r.count;
            }
            return total;
        }
    }

    // A list of words which will each be repeated the specified number of times.
    public class RepWordList : StimWordList<Word> {
        public int repeats;

        public RepWordList(int repeats_ = 1) {
            repeats = repeats_;
        }

        public RepWordList(List<Word> wordList, int repeats_ = 1,
            List<bool> stimList = null)
            : base(wordList, stimList) {
            repeats = repeats_;
        }
    }

    // Generates well-spaced RepFR wordlists with open-loop stimulation assigned.
    public class RepWordGenerator {
        // perm is the permutation to be assigned to the specified repword_lists,
        // interpreted in order.  If the first word in the first RepWordList is to
        // be repeated 3 times, the first three indices in perm are its locations
        // in the final list.  The score is a sum of the inverse
        // distances-minus-one between all neighboring repeats of each word.  Word
        // lists with repeats spaced farther receive the lowest scores, and word
        // lists with adjacent repeats receive a score of infinity.
        public static double SpacingScore(List<int> perm,
            List<RepWordList> repwordLists) {
            var split = new List<List<int>>();
            int offset = 0;
            foreach (var wl in repwordLists) {
                for (int w = 0; w < wl.Count; w++) {
                    var row = new List<int>();
                    for (int r = 0; r < wl.repeats; r++) {
                        row.Add(perm[w * wl.repeats + r + offset]);
                    }
                    split.Add(row);
                }
                offset += wl.Count * wl.repeats;
            }

            double score = 0;

            foreach (var s in split) {
                s.Sort();

                for (int i = 0; i < s.Count - 1; i++) {
                    double dist = s[i + 1] - s[i];
                    score += 1.0 / (dist - 1);
                    // score += (Math.Abs(dist) > 1) ? 0 : Double.PositiveInfinity;
                }
            }

            return score;
        }

        // Prepares a list of repeated words with better than random spacing,
        // while keeping the repeats associated with their stim state.
        public static StimWordList<Word> SpreadWords(
                List<RepWordList> repwordLists,
                double topPercentSpaced = 0.2) {

            int wordLen = 0;
            foreach (var wl in repwordLists) {
                wordLen += wl.Count * wl.repeats;
            }

            var arrangements = new List<Tuple<double, List<int>>>();

            int iterations = Convert.ToInt32(100 / topPercentSpaced);

            for (int i = 0; i < iterations; i++) {
                double score = 1.0 / 0;
                int giveUp = 20;
                var perm = new List<int>();
                while (giveUp > 0 && double.IsInfinity(score)) {
                    var range = Enumerable.Range(0, wordLen).ToList();
                    perm = range.Shuffle();

                    score = SpacingScore(perm, repwordLists);
                    giveUp--;
                }
                arrangements.Add(new Tuple<double, List<int>>(score, perm));
            }

            arrangements.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            var wordlst = new List<WordStim<Word>>();
            foreach (var wl in repwordLists) {
                foreach (var wordStim in wl) {
                    for (int i = 0; i < wl.repeats; i++) {
                        wordlst.Add(wordStim);
                    }
                }
            }

            var wordsSpread = new List<WordStim<Word>>(wordlst);

            for (int i = 0; i < wordlst.Count; i++) {
                wordsSpread[arrangements[0].Item2[i]] = wordlst[i];
            }

            return new StimWordList<Word>(wordsSpread, score: arrangements[0].Item1);
        }

        public static void AssignRandomStim(RepWordList rw) {
            for (int i = 0; i < rw.Count; i++) {
                bool stim = Convert.ToBoolean(InterfaceManager.rnd.Value.Next(2));
                rw.SetStim(i, stim);
            }
        }

        // Create a RepFR open-stim word list from specified lists of words to be
        // repeated and list of words to use once.
        public static StimWordList<Word> Generate(
            List<RepWordList> repeats,
            RepWordList singles,
            bool doStim,
            double topPercentSpaced = 0.2) {

            if (doStim) {
                // Open-loop stim assigned here.
                foreach (var rw in repeats) {
                    AssignRandomStim(rw);
                }
                AssignRandomStim(singles);
            }

            StimWordList<Word> preparedWords = SpreadWords(repeats, topPercentSpaced);

            foreach (var word_stim in singles) {
                int insert_at = InterfaceManager.rnd.Value.Next(preparedWords.Count + 1);
                preparedWords.Insert(insert_at, word_stim);
            }

            return preparedWords;
        }

        // Create a RepFR open-stim word list from a list of repetitions and counts,
        // and a list of candidate words.
        public static StimWordList<Word> Generate(
            RepCounts repCounts,
            List<Word> inputWords,
            bool doStim,
            double topPercentSpaced = 0.2) {

            var shuffled = inputWords.Shuffle();

            var repeats = new List<RepWordList>();
            var singles = new RepWordList();

            var shuf = new BoundedInt(shuffled.Count,
                "Words required exceeded input word list size.");
            foreach (var rc in repCounts) {
                if (rc.rep == 1) {
                    for (int i = 0; i < rc.count; i++) {
                        singles.Add(shuffled[shuf.i++]);
                    }
                } else if (rc.rep > 1 && rc.count > 0) {
                    var repWords = new RepWordList(rc.rep);
                    for (int i = 0; i < rc.count; i++) {
                        repWords.Add(shuffled[shuf.i++]);
                    }
                    repeats.Add(repWords);
                }
            }

            return Generate(repeats, singles, doStim, topPercentSpaced);
        }
    }

}
