// Provides random subsets of a word pool without replacement.
using System;
using System.Collections.Generic;
using UnityEPL;

public class WordRandomSubset {
    protected List<Word> shuffled;
    protected int index;

    protected WordRandomSubset() {
        index = 0;
    }

    public WordRandomSubset(List<Word> sourceWords) {
        shuffled = sourceWords.Shuffle();
        index = 0;
    }

    public virtual List<Word> Get(int amount) {
        if ((shuffled.Count - index) < amount) {
            throw new IndexOutOfRangeException("Word list too small for session");
        }
        int indexNow = index;
        index += amount;

        return shuffled.GetRange(indexNow, amount);
    }
}
