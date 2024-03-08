using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEPL;

public class CatFRExperiment : FRExperimentBase<CategorizedWord, FRRun<CategorizedWord>, FRSession<CategorizedWord>> {
    protected override void SetupWordList() {
        var wordRepeats = Config.wordRepeats;
        if (wordRepeats.Count() != 1 && wordRepeats[0] != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's wordRepeats should only have one item with a value of 1"));
        }

        wordsPerList = Config.wordCounts[0];

        var sourceWords = ReadWordpool<CategorizedWord>(manager.fileManager.GetWordList());
        var words = new CategorizedWordRandomSubset(sourceWords);

        // TODO: (feature) Load Session
        currentSession = GenerateSession(words);
    }
}
