//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEPL.Extensions;

public class UniqueCategorizedWordRandomSubset : CategorizedWordRandomSubset {

    public UniqueCategorizedWordRandomSubset(List<CategorizedWord> sourceWords) : base(sourceWords) { }

    // Get one word from each category
    public override List<CategorizedWord> Get(int amount) {
        var remainingCategories = shuffled
            .Where(x => x.Value.Count() > 0)
            .ToList().Shuffle();

        if (amount > remainingCategories.Count()) {
            throw new IndexOutOfRangeException("Word list too small for session");
        }

        // Make sure to use the categories with more items first to balance item usage
        remainingCategories.Sort((x, y) => y.Value.Count().CompareTo(x.Value.Count()));

        var words = new List<CategorizedWord>();
        for (int i = 0; i < amount; ++i) {
            var catWords = remainingCategories[i];
            words.Add(catWords.Value.Last());
            shuffled[catWords.Key].RemoveAt(catWords.Value.Count - 1);
        }

        return words;
    }
}
