using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEPL {

    // Stores a word and whether or not it should be stimulated during encoding.
    public class WordStim {
        public string word;
        public bool stim;

        public WordStim(string new_word, bool new_stim = false) {
            word = new_word;
            stim = new_stim;
        }

        public override string ToString() {
            return String.Format("{0}:{1}", word, Convert.ToInt32(stim));
        }
    }

    // This class keeps a list of words associated with their stim states.
    public class StimWordList : Timeline<WordStim> {

        public override bool IsReadOnly { get { return false; } }
        protected List<string> words_;
        public IList<string> words {
            get { return words_.AsReadOnly(); }
        }
        protected List<bool> stims_;
        public IList<bool> stims {
            get { return stims_.AsReadOnly(); }
        }
        public override int Count {
            get { return words_.Count; }
        }
        protected double score_;
        public double score {
            get { return score_; }
        }

        public StimWordList() {
            words_ = new List<string>();
            stims_ = new List<bool>();
            score_ = Double.NaN;
        }

        public StimWordList(List<string> word_list, List<bool> stim_list = null, double score = Double.NaN) {
            words_ = new List<string>(word_list);
            stims_ = new List<bool>(stim_list ?? new List<bool>());
            score_ = score;

            // Force the two lists to be the same size.
            if (stims_.Count > words_.Count) {
                stims_.RemoveRange(words_.Count, 0);
            } else {
                while (stims_.Count < words_.Count) {
                    stims_.Add(false);
                }
            }
        }

        public StimWordList(List<WordStim> word_stim_list, double score = Double.NaN) {
            words_ = new List<string>();
            stims_ = new List<bool>();
            score_ = score;

            foreach (var ws in word_stim_list) {
                words_.Add(ws.word);
                stims_.Add(ws.stim);
            }
        }

        public void Add(string word, bool stim = false) {
            words_.Add(word);
            stims_.Add(stim);
        }

        public override void Add(WordStim word_stim) {
            Add(word_stim.word, word_stim.stim);
        }

        public void Insert(int index, string word, bool stim = false) {
            words_.Insert(index, word);
            stims_.Insert(index, stim);
        }

        public override void Insert(int index, WordStim word_stim) {
            Insert(index, word_stim.word, word_stim.stim);
        }

        public override IEnumerator<WordStim> GetEnumerator() {
            for (int i = 0; i < words_.Count; i++) {
                yield return new WordStim(words_[i], stims_[i]);
            }
        }

        // needed to allow writing to collection
        // when loading session in progress
        public override void Clear() {
            throw new NotSupportedException("method included only for compatibility");
        }

        public override bool Contains(WordStim item) {
            throw new NotSupportedException("method included only for compatibility");
        }

        public override void CopyTo(WordStim[] array, int arrayIndex) {
            if (array == null) throw new ArgumentNullException();

            if (arrayIndex < 0) throw new ArgumentOutOfRangeException();

            if (this.Count > array.Length - arrayIndex) throw new ArgumentException();

            GetEnumerator().ToEnumerable().ToArray().CopyTo(array, arrayIndex);
        }

        public override bool Remove(WordStim item) {
            throw new NotSupportedException("method included only for compatibility");
        }

        // Read-only indexed access.
        public override WordStim this[int i] {
            get { return new WordStim(words_[i], stims_[i]); }
        }

        public override string ToString() {
            string str = this[0].ToString();
            for (int i = 1; i < this.Count; i++) {
                str += String.Format(", {0}", this[i]);
            }
            return str;
        }

        public void SetStim(int index, bool state = true) {
            stims_[index] = state;
        }
    }

    // Generates well-spaced RepFR wordlists with open-loop stimulation assigned.
    public class WordGenerator {
        public static void AssignRandomStim(StimWordList rw) {
            for (int i = 0; i < rw.Count; i++) {
                bool stim = Convert.ToBoolean(InterfaceManager.rnd.Value.Next(2));
                rw.SetStim(i, stim);
            }
        }

        // TODO: JPB: (needed) Make suyre WordGenerator works
        public static StimWordList Generate(List<string> inputWords, bool doStim) {
            var wordList = new StimWordList();
            foreach (var word in inputWords) {
                wordList.Add(word);
            }

            if (doStim) {
                AssignRandomStim(wordList);                
            }

            return wordList;
        }
    }

}