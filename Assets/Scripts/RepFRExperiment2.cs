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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEPL {

    public class RepFRExperiment2 : FRExperimentBase<Word, FRRun<Word>, FRSession<Word>> {
        protected RepCounts repCounts = null;
        protected int uniqueWordsPerList;

        protected override async Task PreTrialStates() {
            SetupWordList();

            await QuitPrompt();
            await Introduction();
            await MicrophoneTest();
            await ConfirmStart();
        }
        protected override async Task PostTrialStates() {
            await FinishExperiment();
        }
        protected override async Task PracticeTrialStates() {
            StartTrial();
            await NextPracticeListPrompt();
            await CountdownVideo();
            await Orientation();
            await Encoding();
            await MathDistractor();
            await PauseBeforeRecall();
            await RecallPrompt();
            await FreeRecall();
            FinishTrial();
        }
        protected override async Task TrialStates() {
            StartTrial();
            await NextListPrompt();
            await CountdownVideo();
            await Orientation();
            await Encoding();
            await MathDistractor();
            await PauseBeforeRecall();
            await RecallPrompt();
            await FreeRecall();
            FinishTrial();
        }

        // Pre-Trial States
        protected override async Task Introduction() {
            await RepeatUntilNo(async () => {
                await textDisplayer.PressAnyKey("show instruction video", "Press any key to show instruction video");

                manager.videoControl.SetVideo(Config.introductionVideo, true);
                SendRamulatorStateMsg(HostPcStateMsg.INSTRUCT(), true);
                manager.hostPC?.SendStateMsgTS(HostPcStateMsg.INSTRUCT());
                await manager.videoControl.PlayVideo();
                SendRamulatorStateMsg(HostPcStateMsg.INSTRUCT(), false);
            }, "repeat introduction video", "Press Y to continue to practice list, \n Press N to replay instructional video.");
        }

        // Trial States
        protected async Task Orientation() {
            SendRamulatorStateMsg(HostPcStateMsg.ORIENT(), true);
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ORIENT());

            int[] limits = Config.fixationDuration;
            int duration = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
            textDisplayer.Display("orientation stimulus", "", "+");
            
            await InterfaceManager.Delay(duration);

            SendRamulatorStateMsg(HostPcStateMsg.ORIENT(), false);
        }
        protected async Task RecallPrompt() {
            manager.highBeep.Play();
            textDisplayer.Display("display recall text", "", "*******");
            await InterfaceManager.Delay(Config.recallOrientationDuration);
        }

        // Setup Functions
        protected void SetupRepetitions() {
            // Repetition specification
            int[] repeats = Config.wordRepeats;
            int[] counts = Config.wordCounts;

            if (repeats.Length != counts.Length) {
                throw new Exception("Word Repeats and Counts not aligned");
            }

            for (int i = 0; i < repeats.Length; i++) {
                if (repCounts == null) {
                    repCounts = new RepCounts(repeats[i], counts[i]);
                } else {
                    repCounts = repCounts.RepCnt(repeats[i], counts[i]);
                }
            }
        }
        protected override void SetupWordList() {
            SetupRepetitions();

            wordsPerList = repCounts.TotalWords();
            uniqueWordsPerList = repCounts.UniqueWords();

            var sourceWords = ReadWordpool<Word>(manager.fileManager.GetWordList());
            var words = new WordRandomSubset<Word>(sourceWords);

            // TODO: (feature) Load Session
            currentSession = GenerateSession(words);
        }

        // Word/Stim List Generation
        protected override FRRun<Word> MakeRun<U>(U randomSubset, bool encStim, bool recStim) {
            var inputWords = randomSubset.Get(uniqueWordsPerList).ToList();
            var blankWords = new List<Word>(Enumerable.Range(1, wordsPerList).Select(i => new Word("")).ToList());
            var encList = RepWordGenerator.Generate(repCounts, inputWords, encStim);
            var recList = RepWordGenerator.Generate(repCounts, blankWords, recStim);
            return new FRRun<Word>(encList, recList, encStim, recStim);
        }
    }

}