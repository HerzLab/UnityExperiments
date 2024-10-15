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
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

using UnityEPL;
using UnityEPL.Utilities;
using UnityEPL.ExternalDevices;

public class RepFRExperiment2 : WordListExperimentBase<RepFRExperiment2, FRSession<Word>, FRTrial<Word>, RepFRConstants, Word> {
    protected RepCounts repCounts = null;
    protected int uniqueWordsPerList;

    protected override async Task PreTrialStates() {
        await SetupWordList();

        await QuitPrompt();
        await Introduction();
        await MicrophoneTest();
        await ConfirmStart();
    }
    protected override async Task PostTrialStates() {
        await FinishExperiment();
    }
    protected override async Task PracticeTrialStates() {
        await StartTrial();
        await NextPracticeTrialPrompt();
        await CountdownVideo();
        await Orientation();
        await Encoding();
        await MathDistractor();
        await PauseBeforeRecall();
        await RecallPrompt();
        await FreeRecall();
    }
    protected override async Task TrialStates() {
        await StartTrial();
        await NextTrialPrompt();
        await CountdownVideo();
        await Orientation();
        await Encoding();
        await MathDistractor();
        await PauseBeforeRecall();
        await RecallPrompt();
        await FreeRecall();
    }

    // Pre-Trial States
    protected override async Task Introduction() {
        SendRamulatorStateMsg(HostPcStatusMsg.INSTRUCT(), true);
        await SetExperimentStatus(HostPcStatusMsg.INSTRUCT());
        await RepeatUntilYes(async (CancellationToken ct) => {
            await PressAnyKey("show instruction video", LangStrings.ShowInstructionVideo());

            manager.videoControl.SetVideo(Config.introductionVideo, true);
            await manager.videoControl.PlayVideo();
        }, "repeat introduction video", LangStrings.RepeatIntroductionVideo(), new());
        SendRamulatorStateMsg(HostPcStatusMsg.INSTRUCT(), false);
    }

    // Trial States
    protected async Task Orientation() {
        SendRamulatorStateMsg(HostPcStatusMsg.ORIENT(), true);
        await SetExperimentStatus(HostPcStatusMsg.ORIENT());

        int[] limits = CONSTANTS.fixationDurationMs;
        int duration = UnityEPL.Utilities.Random.Rnd.Next(limits[0], limits[1]);
        textDisplayer.Display("orientation stimulus", LangStrings.Blank(), LangStrings.GenForCurrLang("+"));
        
        await manager.Delay(duration);

        SendRamulatorStateMsg(HostPcStatusMsg.ORIENT(), false);
    }
    protected async Task RecallPrompt() {
        manager.highBeep.Play();
        textDisplayer.Display("display recall text", LangStrings.Blank(), LangStrings.GenForCurrLang("*******"));
        await manager.Delay(CONSTANTS.recallOrientationDurationMs);
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
    protected override Task SetupWordList() {
        SetupRepetitions();

        wordsPerList = repCounts.TotalWords();
        uniqueWordsPerList = repCounts.UniqueWords();

        var sourceWords = ReadWordpool<Word>(FileManager.GetWordList(), "wordpool");
        var words = new WordRandomSubset<Word>(sourceWords, CONSTANTS.splitWordsOverTwoSessions);

        // TODO: (feature) Load Session
        session = GenerateSession(words);

        return Task.CompletedTask;
    }

    // Word/Stim List Generation
    protected override FRTrial<Word> MakeRun<U>(U randomSubset, bool encStim, bool recStim) {
        var inputWords = randomSubset.Get(uniqueWordsPerList).ToList();
        var blankWords = new List<Word>(Enumerable.Range(1, wordsPerList).Select(i => new Word("")).ToList());
        var encList = RepWordGenerator.Generate(repCounts, inputWords, encStim);
        var recList = RepWordGenerator.Generate(repCounts, blankWords, recStim);
        return new FRTrial<Word>(encList, recList, encStim, recStim);
    }
}
