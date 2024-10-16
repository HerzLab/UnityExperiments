//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

using UnityEPL;

public class CatFRExperiment : WordListExperimentBase<CatFRExperiment, FRSession<CategorizedWord>, FRTrial<CategorizedWord>, FRConstants, CategorizedWord> {
    protected override Task SetupWordList() {
        var wordRepeats = Config.wordRepeats;
        if (wordRepeats.Count() != 1 && wordRepeats[0] != 1) {
            throw new Exception("Config's wordRepeats should only have one item with a value of 1");
        }

        wordsPerList = Config.wordCounts[0];

        var sourceWords = ReadWordpool<CategorizedWord>(FileManager.GetWordList(), "wordpool");
        var words = new CategorizedWordRandomSubset(sourceWords, CONSTANTS.splitWordsOverTwoSessions);

        // TODO: (feature) Load Session
        session = GenerateSession(words);

        return Task.CompletedTask;
    }
}
