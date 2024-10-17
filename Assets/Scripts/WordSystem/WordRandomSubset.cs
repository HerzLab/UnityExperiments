//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;

using PsyForge;
using PsyForge.Extensions;

// Provides random subsets of a word pool without replacement.
public class WordRandomSubset<T> 
    where T : Word
{
    protected List<T> shuffled;
    protected int index;
    protected string usedWordsPath;

    public int Count => shuffled.Count;
    public int Index => index;

    /// <summary>
    /// This is only used by inherited classes
    /// If this is used, make sure to implement handling for Config.splitWordsOverTwoSessions
    /// </summary>
    protected WordRandomSubset() {
        index = 0;
    }

    public WordRandomSubset(List<T> sourceWords, bool splitWordsOverTwoSessions = false, string usedWordsPath = null) {
        index = 0;
        this.usedWordsPath = usedWordsPath;

        // Only keep the words for that session
        if (splitWordsOverTwoSessions) {
            int splitIndex = sourceWords.Count / 2;
            int lenRemove = sourceWords.Count - splitIndex;
            sourceWords.ShuffleInPlace(PsyForge.Utilities.Random.StableRnd);
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
            throw new WordListTooSmallException($"Word list ({shuffled.Count}) too small for session (> {index+amount})");
        }
        int indexNow = index;
        index += amount;

        var words = shuffled.GetRange(indexNow, amount);
        if (usedWordsPath != null) {
            File.AppendAllLines(usedWordsPath, words.ConvertAll(w => w.ToTSV()));
        }

        return words;
    }
}

public class WordListTooSmallException : SystemException {
    public WordListTooSmallException() : base() { }
    public WordListTooSmallException(string message) : base(message) { }
    public WordListTooSmallException(string message, Exception inner) : base(message, inner) { }
}