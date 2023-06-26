using System;
using System.Collections.Generic;
using System.Linq;
using UnityEPL;

public class UniqueCategorizedWordRandomSubset : CategorizedWordRandomSubset {

    public UniqueCategorizedWordRandomSubset(List<CategorizedWord> sourceWords) : base(sourceWords) { }

    // Get one word from each category
    public override List<Word> Get(int amount) {
        var remainingCategories = shuffled
            .Where(x => x.Value.Count() > 0)
            .ToList().Shuffle();

        if (amount > remainingCategories.Count()) {
            throw new IndexOutOfRangeException("Word list too small for session");
        }

        // Make sure to use the categories with more items first to balance item usage
        remainingCategories.Sort((x, y) => y.Value.Count().CompareTo(x.Value.Count()));

        var words = new List<Word>();
        for (int i = 0; i < amount; ++i) {
            var catWords = remainingCategories[i];
            words.Add(catWords.Value.Last());
            shuffled[catWords.Key].RemoveAt(catWords.Value.Count - 1);
        }

        return words;
    }
}
