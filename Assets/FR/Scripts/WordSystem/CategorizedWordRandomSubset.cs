using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEPL;

public class CategorizedWordRandomSubset : WordRandomSubset {
    protected new Dictionary<string, List<Word>> shuffled = new();

    public CategorizedWordRandomSubset(List<CategorizedWord> sourceWords) {
        Dictionary<string, List<Word>> catWords = new();

        foreach (var word in sourceWords) {
            List<Word> temp;
            catWords.TryGetValue(word.category, out temp);
            temp = temp != null ? temp : new();
            temp.Add(word);
            catWords[word.category] = temp; // TODO: JPB: Is this line needed?
        }

        foreach (var words in catWords) {
            shuffled[words.Key] = words.Value.Shuffle();
        }

        if (Config.splitWordsOverTwoSessions) {
            var stableShuffledCategories = shuffled.ToList();

            if (stableShuffledCategories.Count % 2 != 0) {
                ErrorNotifier.ErrorTS(new Exception($"There are an odd number of categories ({stableShuffledCategories.Count}), even though the config says to splitWordsOverTwoSessions"));
            }

            stableShuffledCategories.Sort((x, y) => {
                return x.Key.CompareTo(y.Key);
            });
            stableShuffledCategories.ShuffleInPlace(InterfaceManager.stableRnd.Value);
            int count = stableShuffledCategories.Count / 2;
            if (Config.sessionNum % 2 == 0) {
                shuffled = stableShuffledCategories.Take(count).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            } else {
                shuffled = stableShuffledCategories.TakeLast(count).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            UnityEngine.Debug.Log(String.Join(", ", shuffled.ToList().ConvertAll(x => $"{x.Key}")));
        }
    }

    /// <summary>
    /// Get list of categorized words with 2 pairs of each category,
    /// where each category should have a pair in the first half of the list
    /// and a pair in the second half of the list.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="IndexOutOfRangeException"></exception>
    /// This can throw an exception even if your word list has enough words.
    /// This is because the randomization process could end up with one category having
    /// two lists worth and not enough other categories to complete the list.
    /// If you are worried about this, run it in a while loop for like 1000 iterations.
    /// It should probably work by then.
    public override List<Word> Get(int amount) {
        var numWordsPerCategory = 4;
        var numCategoriesPerList = amount / numWordsPerCategory;

        var justOneCategories = shuffled
            .Where(x => (x.Value.Count() > 0) && (x.Value.Count() <= numWordsPerCategory))
            .ToList().ShuffleInPlace();
        var moreThanOneCategories = shuffled
            .Where(x => x.Value.Count() > numWordsPerCategory)
            .ToList().ShuffleInPlace();

        // This saves numCategoriesPerList categories for the end
        // to avoid the situation where there are more than one sets of one category, but
        // not enough unique categories to get one each
        // Ex: amount = 12, numWordsPerCategory = 4, numCategoriesPerList = 3
        //     On the last iteration: there are 2 categories, one has 8 words, the other has 4
        //     This wouldn't work because even though there are enough words left, there are not
        //     enough unique categories.
        var categoryLists = justOneCategories;
        if (moreThanOneCategories.Count > 0) {
            var numMoreThanOneCategories = moreThanOneCategories.Count;
            // Extra (not holding to avoid error) categories with only one list worth of words
            var numExtraJustOneCategories = Math.Max(0, justOneCategories.Count - numCategoriesPerList);
            // Needed just one categories, in case of amount = 12, numCategoriesPerList = 3, numWordsPerCategory = 4
            // moreThanOneCategories counts = [8], and justOneCategories counts = [4, 4, 4, 4, 4]
            var numNeededJustOneCategories = Math.Max(numExtraJustOneCategories, numCategoriesPerList - numMoreThanOneCategories);
            // The justOneCategories that will be used
            var neededJustOneCategories = justOneCategories.Take(numNeededJustOneCategories);
            categoryLists = moreThanOneCategories.Concat(neededJustOneCategories).ToList().ShuffleInPlace();
        }
        //UnityEngine.Debug.Log("----------------------------------------");
        //UnityEngine.Debug.Log(shuffled.Values.ToList().ConvertAll(x => x.Count).Sum());
        //UnityEngine.Debug.Log(String.Join(", ", shuffled.Where(x => x.Value.Count != 0).ToList().ConvertAll(x => $"{x.Key} {x.Value.Count}")));
        //UnityEngine.Debug.Log(String.Join(", ", justOneCategories.ConvertAll(x => $"{x.Key} {x.Value.Count}")));
        //UnityEngine.Debug.Log(String.Join(", ", moreThanOneCategories.ConvertAll(x => $"{x.Key} {x.Value.Count}")));
        //UnityEngine.Debug.Log(String.Join(", ", categoryLists.ConvertAll(x => $"{x.Key} {x.Value.Count}")));
        //UnityEngine.Debug.Log(String.Join("\n", categoryLists.ConvertAll(x => String.Join(", ", x.Value))));

        if (amount % numWordsPerCategory != 0) {
            throw new ArgumentException($"The amount ({amount}) argument must be divisble by {numWordsPerCategory}");
        } else if (numCategoriesPerList > categoryLists.Count()) {
            throw new IndexOutOfRangeException("Word list too small for session");
        }

        // Get the words from each category
        List<List<Word>> wordLists = new();
        for (int i = 0; i < shuffled.Count; ++i) {
            wordLists.Add(new());
        }
        for (int i = 0; i < numCategoriesPerList; ++i) {
            var category = categoryLists[i].Key;
            var words = categoryLists[i].Value;
            //UnityEngine.Debug.Log($"{category}: {String.Join(", ", words)}");

            wordLists[i].AddRange(words.Take(numWordsPerCategory));
            shuffled[category].RemoveRange(0, numWordsPerCategory);
        }

        // The lists are composed of 2 pairs of each category,
        // where each category should have a pair in the first half of the list
        // and a pair in the second half of the list.
        // However, there should never be pairs of the same category next to each other.
        // So we make sure the last pair of the first half is not at the beginning of the second half
        var groups = Enumerable.Range(0, numCategoriesPerList).ToList().ShuffleInPlace();
        var firstHalfLastItem = groups.Last();

        var groupsSecondHalf = Enumerable.Range(0, numCategoriesPerList).ToList().ShuffleInPlace();
        groupsSecondHalf.Remove(firstHalfLastItem);
        groupsSecondHalf.Insert(InterfaceManager.rnd.Value.Next(1, numCategoriesPerList), firstHalfLastItem);
        groups.Add(groupsSecondHalf);

        // Make the final word list
        List<Word> finalWordList = new();
        foreach (var i in groups) {
            finalWordList.Add(wordLists[i][0]);
            finalWordList.Add(wordLists[i][1]);
            wordLists[i].RemoveRange(0, 2);
        }

        //UnityEngine.Debug.Log(String.Join(", ", finalWordList));

        return finalWordList;
    }
}
