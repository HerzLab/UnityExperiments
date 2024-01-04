using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEPL;

public class CatFRExperiment : FRExperimentBase<CategorizedWord> {
    protected override void SetupWordList() {
        var wordRepeats = Config.wordRepeats;
        if (wordRepeats.Count() != 1 && wordRepeats[0] != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's wordRepeats should only have one item with a value of 1"));
        }

        wordsPerList = Config.wordCounts[0];
        blankWords = new List<CategorizedWord>(Enumerable.Range(1, wordsPerList).Select(i => new CategorizedWord("", "")).ToList());

        var sourceWords = ReadCategorizedWordpool();
        var words = new CategorizedWordRandomSubset(sourceWords);

        // TODO: (feature) Load Session
        currentSession = GenerateSession(words);
    }

    protected List<CategorizedWord> ReadCategorizedWordpool() {
        // wordpool is a file with 'category\tword' as a header
        // with one category and one word per line.
        // repeats are described in the config file with two matched arrays,
        // repeats and counts, which describe the number of presentations
        // words can have and the number of words that should be assigned to
        // each of those presentation categories.
        string sourceList = manager.fileManager.GetWordList();
        Debug.Log(sourceList);
        var sourceWords = new List<CategorizedWord>();

        //skip line for csv header
        foreach (var line in File.ReadLines(sourceList).Skip(1)) {
            string[] categoryAndWord = line.Split('\t');
            sourceWords.Add(new CategorizedWord(categoryAndWord[1], categoryAndWord[0]));
        }

        // copy full wordpool to session directory
        string path = System.IO.Path.Combine(manager.fileManager.SessionPath(), "wordpool.txt");
        File.WriteAllText(path, String.Join("\n", sourceWords));

        // copy full categorized wordpool to session directory
        string catPath = System.IO.Path.Combine(manager.fileManager.SessionPath(), "categorized_wordpool.txt");
        var catList = sourceWords.ConvertAll(x => $"{x.category}\t{x.word}");
        File.WriteAllText(catPath, String.Join("\n", catList));

        return sourceWords;
    }


}
