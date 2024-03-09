//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEPL {

    // Stores a word and whether or not it should be stimulated during encoding.
    public class WordStim<T>
        where T : Word
    {
        public T word;
        public bool stim;

        public WordStim(T new_word, bool new_stim = false) {
            word = new_word;
            stim = new_stim;
        }

        public override string ToString() {
            return String.Format("{0}:{1}", word, Convert.ToInt32(stim));
        }
    }

    // This class keeps a list of words associated with their stim states.
    public class StimWordList<T> : Timeline<WordStim<T>> 
        where T : Word
    {
        public override bool IsReadOnly { get { return false; } }
        protected List<T> words_;
        public IList<T> words {
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
            words_ = new List<T>();
            stims_ = new List<bool>();
            score_ = Double.NaN;
        }

        public StimWordList(List<T> word_list, List<bool> stim_list = null, double score = Double.NaN) {
            words_ = new List<T>(word_list);
            score_ = score;
            if (stims_ == null) {
                stims_ = new();
                for (int i = 0; i < words_.Count; i++) {
                    stims_.Add(false);
                }
            }

            if (words_.Count != stims_.Count) {
                ErrorNotifier.ErrorTS(new
                    ArgumentException("word_list and stim_list must be the same length"));
            }
        }

        public StimWordList(List<WordStim<T>> word_stim_list, double score = Double.NaN) {
            words_ = new List<T>();
            stims_ = new List<bool>();
            score_ = score;

            foreach (var ws in word_stim_list) {
                words_.Add(ws.word);
                stims_.Add(ws.stim);
            }
        }

        public void Add(T word, bool stim = false) {
            words_.Add(word);
            stims_.Add(stim);
        }

        public override void Add(WordStim<T> word_stim) {
            Add(word_stim.word, word_stim.stim);
        }

        public void Insert(int index, T word, bool stim = false) {
            words_.Insert(index, word);
            stims_.Insert(index, stim);
        }

        public override void Insert(int index, WordStim<T> word_stim) {
            Insert(index, word_stim.word, word_stim.stim);
        }

        public override IEnumerator<WordStim<T>> GetEnumerator() {
            for (int i = 0; i < words_.Count; i++) {
                yield return new WordStim<T>(words_[i], stims_[i]);
            }
        }

        // needed to allow writing to collection
        // when loading session in progress
        public override void Clear() {
            throw new NotSupportedException("method included only for compatibility");
        }

        public override bool Contains(WordStim<T> item) {
            throw new NotSupportedException("method included only for compatibility");
        }

        public override void CopyTo(WordStim<T>[] array, int arrayIndex) {
            if (array == null) throw new ArgumentNullException();

            if (arrayIndex < 0) throw new ArgumentOutOfRangeException();

            if (this.Count > array.Length - arrayIndex) throw new ArgumentException();

            GetEnumerator().ToEnumerable().ToArray().CopyTo(array, arrayIndex);
        }

        public override bool Remove(WordStim<T> item) {
            throw new NotSupportedException("method included only for compatibility");
        }

        // Read-only indexed access.
        public override WordStim<T> this[int i] {
            get { return new WordStim<T>(words_[i], stims_[i]); }
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

}