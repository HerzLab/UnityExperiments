// Provides random subsets of a word pool without replacement.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEPL;

public class WordRandomSubset {
    protected List<Word> shuffled;
    protected int index;

    /// <summary>
    /// This is only used by inherited classes
    /// If this is used, make sure to implement handling for Config.splitWordsOverTwoSessions
    /// </summary>
    protected WordRandomSubset() {
        index = 0;
    }

    public WordRandomSubset(List<Word> sourceWords) {
        index = 0;
        
        // Only keep the words for that session
        if (Config.splitWordsOverTwoSessions) {
            int splitIndex = sourceWords.Count / 2;
            int lenRemove = sourceWords.Count - splitIndex;
            sourceWords
                .ShuffleInPlace(InterfaceManager.stableRnd.Value)
                .RemoveRange(splitIndex, lenRemove);
            //UnityEngine.Debug.Log(String.Join(", ", sourceWords.ConvertAll((x) => x.ToString())));
        }

        shuffled = sourceWords.Shuffle();
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
