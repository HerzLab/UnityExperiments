using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEPL;

public class WordDisplayer : MonoBehaviour {
    public TextMeshProUGUI singleWord;
    public TextMeshProUGUI pairedWord1;
    public TextMeshProUGUI pairedWord2;

    public void SetWordSize(List<Word> words) {
        var strList = words.Select(x => x.word).ToList();
        int fontSize = (int)UnityUtilities.FindMaxFittingFontSize(strList, pairedWord1);

        singleWord.enableAutoSizing = false;
        singleWord.fontSizeMax = fontSize;
        singleWord.fontSize = fontSize;

        pairedWord1.enableAutoSizing = false;
        pairedWord1.fontSizeMax = fontSize;
        pairedWord1.fontSize = fontSize;

        pairedWord2.enableAutoSizing = false;
        pairedWord2.fontSizeMax = fontSize;
        pairedWord2.fontSize = fontSize;
    }

    public void DisplayWord(string word) {
        Dictionary<string, object> dataDict = new() {
            { "words", new string[1] { word } },
        };
        EventReporter.Instance.LogTS("word stimulus", dataDict);
        singleWord.text = word;
    }

    public void DisplayPairedWord(string word1, string word2) {
        Dictionary<string, object> dataDict = new() {
            { "words", new string[2] { word1, word2 } },
        };
        EventReporter.Instance.LogTS("word stimulus", dataDict);
        pairedWord1.text = word1;
        pairedWord2.text = word2;
    }

    public void ClearWords() {
        EventReporter.Instance.LogTS("clear word stimulus");
        singleWord.text = "";
        pairedWord1.text = "";
        pairedWord2.text = "";
    }
}