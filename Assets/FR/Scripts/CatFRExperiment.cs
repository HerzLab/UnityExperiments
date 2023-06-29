using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEPL;

public class CatFRExperiment : FRExperiment {
    protected override void SetupWordList() {
        var wordRepeats = Config.wordRepeats;
        if (wordRepeats.Count() != 1 && wordRepeats[0] != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's wordRepeats should only have one item with a value of 1"));
        }

        wordsPerList = Config.wordCounts[0];
        blankWords = new List<string>(Enumerable.Repeat(string.Empty, wordsPerList));

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
        string source_list = manager.fileManager.GetWordList();
        Debug.Log(source_list);
        var source_words = new List<CategorizedWord>();

        //skip line for csv header
        foreach (var line in File.ReadLines(source_list).Skip(1)) {
            string[] category_and_word = line.Split('\t');
            source_words.Add(new CategorizedWord(category_and_word[1], category_and_word[0]));
        }

        // copy wordpool to session directory
        string path = System.IO.Path.Combine(manager.fileManager.SessionPath(), "wordpool.txt");
        File.Copy(source_list, path, true);

        return source_words;
    }


}
