//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections.Generic;
using UnityEPL;

namespace UnityEPL {

    public class MemMapTrial<T> : FRRun<T> 
        where T : PairedWord
    {
        public StimWordList<T> recognition;
        public bool recognitionStim;

        public override Dictionary<string, bool> GetStimValues() {
            var stimValues = base.GetStimValues();
            stimValues.Add("recogStim", recognitionStim);
            return stimValues;
        }

        public MemMapTrial(StimWordList<T> encodingList, StimWordList<T> recallList, StimWordList<T> recogList,
            bool setEncodingStim = false, bool setRecallStim = false, bool setRecogStim = false) :
            base(encodingList, recallList, setEncodingStim, setRecallStim)
        {
            recognition = recogList;
            recognitionStim = setRecogStim;
        }
    }

    [Serializable]
    public class MemMapSession<T> : FRSessionBase<T, MemMapTrial<T>> 
        where T : PairedWord 
    {
        public MemMapTrial<T> GetTrial() {
            return GetState();
        }

        private static string StimWordToWord(T sw) {
            return sw.word;
        }

        public void DebugPrintAll() {
            string output = "";
            foreach (var trial in this) {
                // Print the stim type
                if (trial.encodingStim && trial.recallStim) { output += "B"; }
                else if (trial.encodingStim) { output += "E"; }
                else if (trial.recallStim) { output += "R"; }
                else { output += "N"; }
                output += " - ";

                // Print the paired word order
                List<string> recallWords = new();
                foreach (var stimWord in trial.recall) {
                    recallWords.Add(stimWord.word.word);
                }
                for (int i = 0; i < trial.encoding.Count; ++i) {
                    output += recallWords.Contains(trial.encoding[i].word.word) ? "T" : "F";
                }
                output += " - ";

                // Print the recog order
                for (int i = 0; i < trial.recognition.Count; ++i) {
                    var recogWord = trial.recognition[i].word;
                    for (int j = 0; j < trial.encoding.Count; ++j) {
                        if (recogWord.word == trial.encoding[j].word.word || recogWord.word == trial.encoding[j].word.pairedWord) {
                            output += i + ",";
                        }
                    }
                }
                output = output.Remove(output.Length-1);
                output += " - ";

                // Print all the words for encoding, recall, and recognition
                output += "[";
                foreach (var stimWord in trial.encoding) {
                    output += $"({stimWord.word.word}, {stimWord.word.pairedWord}) ";
                }
                output = output.Remove(output.Length-1);
                output += "] - [";
                foreach (var stimWord in trial.recall) {
                    output += $"{stimWord.word.word} ";
                }
                output = output.Remove(output.Length-1);
                output += "] - [";
                foreach (var stimWord in trial.recognition) {
                    output += $"{stimWord.word.word} ";
                }
                output = output.Remove(output.Length-1);
                output += "]";

                output += "\n";
            }
            UnityEngine.Debug.Log(output);
        }
    }

}
