// Provides random subsets of a word pool without replacement.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEPL;

public class WordRandomSubset<T> 
    where T : Word
{
    protected List<T> shuffled;
    protected int index;

    public int Count => shuffled.Count;
    public int Index => index;

    /// <summary>
    /// This is only used by inherited classes
    /// If this is used, make sure to implement handling for Config.splitWordsOverTwoSessions
    /// </summary>
    protected WordRandomSubset() {
        index = 0;
    }

    public WordRandomSubset(List<T> sourceWords, bool ignoreSplit = false) {
        index = 0;

        // Only keep the words for that session
        if (Config.splitWordsOverTwoSessions && !ignoreSplit) {
            int splitIndex = sourceWords.Count / 2;
            int lenRemove = sourceWords.Count - splitIndex;
            sourceWords.ShuffleInPlace(InterfaceManager.stableRnd.Value);
            if (Config.sessionNum % 2 == 0) {
                sourceWords.RemoveRange(splitIndex, lenRemove);
            } else {
                sourceWords.RemoveRange(0, splitIndex);
            }
                
            //UnityEngine.Debug.Log(String.Join(", ", sourceWords.ConvertAll((x) => x.ToString())));
        }

        shuffled = sourceWords.Shuffle();
    }

    public virtual List<T> Get(int amount) {
        if ((shuffled.Count - index) < amount) {
            throw new IndexOutOfRangeException($"Word list ({shuffled.Count}) too small for session (> {index+amount})");
        }
        int indexNow = index;
        index += amount;

        return shuffled.GetRange(indexNow, amount);
    }
}