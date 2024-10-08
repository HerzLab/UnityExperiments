//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using UnityEPL;
using UnityEPL.Extensions;
using UnityEPL.ExternalDevices;

public class WordDisplayer : MonoBehaviour {
    [SerializeField] protected TextMeshProUGUI singleWord;
    [SerializeField] protected TextMeshProUGUI pairedWord1;
    [SerializeField] protected TextMeshProUGUI pairedWord2;

    protected Dictionary<string, object> wordStimulusData = null;

    public void Awake() {
        TurnOff();
    }

    public void TurnOff() {
        gameObject.SetActive(false);
    }

    public void SetWordSize(List<Word> words) {
        gameObject.SetActive(true);
        var strList = words.Select(x => x.word).ToList();
        int fontSize = (int)pairedWord1.FindMaxFittingFontSize(strList);
        gameObject.SetActive(false);

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

    public void DisplayWord(string word, Dictionary<string, object> data = null) {
        gameObject.SetActive(true);
        wordStimulusData = data ?? new();
        wordStimulusData["words"] = new string[1] { word };
        
        MainManager.Instance.hostPC?.SendStateMsgTS(HostPcStateMsg.WORD(), data);
        EventReporter.Instance.LogTS("word stimulus", wordStimulusData);
        singleWord.text = word;
    }

    public void DisplayPairedWord(string word1, string word2, Dictionary<string, object> data = null) {
        gameObject.SetActive(true);
        wordStimulusData = data ?? new();
        wordStimulusData["words"] = new string[2] { word1, word2 };

        MainManager.Instance.hostPC?.SendStateMsgTS(HostPcStateMsg.WORD(), data);
        EventReporter.Instance.LogTS("word stimulus", wordStimulusData);
        pairedWord1.text = word1;
        pairedWord2.text = word2;
        
    }

    public void ClearWords() {
        EventReporter.Instance.LogTS("clear word stimulus", wordStimulusData);
        wordStimulusData = null;
        singleWord.text = "";
        pairedWord1.text = "";
        pairedWord2.text = "";
    }
}