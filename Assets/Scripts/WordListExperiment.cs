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
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

using UnityEPL;
public class WordListExperiment<WordType, TrialType, SessionType> : ExperimentBase<WordListExperiment<WordType, TrialType, SessionType>, SessionType, TrialType> 
    where WordType : Word, new()
    where TrialType : FRTrial<WordType>
    where SessionType : WordListSession<WordType, TrialType>, new()
{
    protected override void AwakeOverride() { }

    protected void Start() {
        Run();
    }

    protected int wordsPerList;


    [SerializeField] protected MathDiplayer mathDiplayer;

    protected override async Task PreTrialStates() {
        await SetupWordList();

        if (!Config.skipIntros) {
            await QuitPrompt();
            await Introduction();
            await MicrophoneTest();
            await ConfirmStart();
        }
    }
    protected override async Task PostTrialStates() {
        await FinishExperiment();
    }
    protected override async Task PracticeTrialStates() {
        await StartTrial();
        await NextPracticeTrialPrompt();
        await CountdownVideo();
        await Fixation();
        await Encoding();
        await MathDistractor();
        await PauseBeforeRecall();
        await RecallOrientation();
        await FreeRecall();
    }
    protected override async Task TrialStates() {
        await StartTrial();
        await NextTrialPrompt();
        await CountdownVideo();
        await Fixation();
        await Encoding();
        await MathDistractor();
        await PauseBeforeRecall();
        await RecallOrientation();
        await FreeRecall();
    }
    protected override void SendRamulatorStateMsg(HostPcStateMsg state, bool stateToggle, Dictionary<string, object> extraData = null) {
        var dict = (extraData != null) ? new Dictionary<string, object>(extraData) : new();
        if (state != HostPcStateMsg.WORD()) {
            dict["phase_type"] = session?.Trial.encodingStim ?? false;
        }
        manager.ramulator?.SendStateMsg(state, stateToggle, dict);
    }


    // Post-Trial States
    protected async Task FinalRecall() {
        // TODO: JPB: (needed) (bug) Change final recall wav file name
        string wavPath = Path.Combine(manager.fileManager.SessionPath(),
                                            "final_recall" + ".wav");

        manager.recorder.StartRecording(wavPath);
        eventReporter.LogTS("start final recall period");
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.FINAL_RECALL(Config.finalRecallDuration));

        await Timing.Delay(Config.finalRecallDuration);

        eventReporter.LogTS("end final recall period");
        manager.recorder.StopRecording();
        manager.lowBeep.Play();
    }
    protected async Task FinishExperiment() {
        await textDisplayer.PressAnyKey("display end message", LangStrings.SessionEnd());
    }

    // Trial States
    protected async Task StartTrial() {
        int numPracticeTrials = Config.practiceLists;
        int numTrials = session.trials.Count;

        Func<Task> optionalPracticeTrialQuestion = async () => {
            var practiceQ = session.TrialNum == 1 ? LangStrings.DoPracticeQuestion() : LangStrings.RepeatPracticeQuestion();
            textDisplayer.Display("repeat practice question", LangStrings.Blank(), practiceQ);
            var keyCode = await inputManager.WaitForKey(new List<KeyCode>() { KeyCode.Y, KeyCode.N });
            if (keyCode == KeyCode.N) { EndCurrentSession(); }
        };

        // Control when the trials end
        if (!session.NextTrial()) {
            EndCurrentSession();
        }
        // TODO: JPB: (needed) (bug) OptionalExtraPracticeTrails fails hard with GenSessions....
        if (session.isPractice) {
            if (Config.optionalExtraPracticeTrials && Config.onlyPracticeOnFirstSession && Config.sessionNum > 1) {
                await optionalPracticeTrialQuestion();
            } else if (Config.onlyPracticeOnFirstSession && Config.sessionNum > 1) {
                EndCurrentSession();
            } else if (Config.optionalExtraPracticeTrials && session.TrialNum > numPracticeTrials) {
                await optionalPracticeTrialQuestion();
            } else if (session.TrialNum > numPracticeTrials) {
                EndCurrentSession();
            }
        } else if (session.TrialNum > numTrials) {
            EndCurrentSession();
        } 

        var stimList = session.Trial.GetStimValues();
        bool stim = stimList.Values.Aggregate((current, next) => current || next);
        Dictionary<string, object> data = new() {
            { "trial", session.TrialNum },
            { "stim", stim },
            { "stimList", stimList },
            { "practice", session.isPractice }
        };
        
        eventReporter.LogTS("start trial", data);
        manager.ramulator?.BeginNewTrial((int)session.TrialNum);
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.TRIAL(), data);
    }
    protected async Task NextTrialPrompt() {
        var key = await textDisplayer.PressAnyKey("trial prompt", LangStrings.TrialPrompt(session.TrialNum));
        if (key == KeyCode.D) { // D for Done
            EndCurrentSession();
        }
    }
    protected async Task NextPracticeTrialPrompt() {
        var key = await textDisplayer.PressAnyKey("practice trial prompt", LangStrings.PracticeTrialPrompt(session.TrialNum));
        if (key == KeyCode.D) { // D for Done
            EndCurrentSession();
        }
    }
    protected async Task CountdownVideo() {
        SendRamulatorStateMsg(HostPcStateMsg.COUNTDOWN(), true, new() { { "current_trial", session.TrialNum } });
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.COUNTDOWN(), new() { { "current_trial", session.TrialNum } });

        manager.videoControl.SetVideo(Config.countdownVideo);
        eventReporter.LogTS("countdown");
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.COUNTDOWN());
        await manager.videoControl.PlayVideo();

        SendRamulatorStateMsg(HostPcStateMsg.COUNTDOWN(), false, new() { { "current_trial", session.TrialNum } });
    }
    protected async Task Fixation() {
        SendRamulatorStateMsg(HostPcStateMsg.ORIENT(), true);
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ORIENT());

        int[] limits = Config.fixationDurationMs;
        int duration = UnityEPL.Random.Rnd.Next(limits[0], limits[1]);
        textDisplayer.Display("orientation stimulus", LangStrings.Blank(), LangStrings.GenForCurrLang("+"));
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ORIENT());
        await Timing.Delay(duration);
        textDisplayer.Clear();

        SendRamulatorStateMsg(HostPcStateMsg.ORIENT(), false);
    }
    protected async Task Orientation(int duration) {
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ORIENT());
        textDisplayer.Display("display recall text", LangStrings.Blank(), LangStrings.GenForCurrLang("*******"));
        manager.highBeep.Play();
        await Timing.Delay(duration);
        textDisplayer.Clear();
    }
    protected async Task Encoding() {
        SendRamulatorStateMsg(HostPcStateMsg.ENCODING(), true, new() { { "current_trial", session.TrialNum } });
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ENCODING(), new() { { "current_trial", session.TrialNum } });

        int[] isiLimits = Config.interStimulusDurationMs;
        var encStimWords = session.Trial.encoding;

        for (int i = 0; i < 12; ++i) {
            int isiDuration = UnityEPL.Random.Rnd.Next(isiLimits[0], isiLimits[1]);
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(isiDuration));
            await Timing.Delay(isiDuration);

            var wordStim = encStimWords[i];
            Dictionary<string, object> data = new() {
                { "word", wordStim.word },
                { "serialPos", i },
                { "stimWord", wordStim.stim },
            };

            eventReporter.LogTS("word stimulus info", data);
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.WORD(), data);
            textDisplayer.Display("word stimulus", LangStrings.Blank(), LangStrings.GenForCurrLang(wordStim.word.ToDisplayString()));
            await Timing.Delay(Config.stimulusDurationMs);
            eventReporter.LogTS("clear word stimulus", data);
            textDisplayer.Clear();
        }

        SendRamulatorStateMsg(HostPcStateMsg.ENCODING(), false, new() { { "current_trial", session.TrialNum } });
    }
    protected async Task FixationDistractor() {
        SendRamulatorStateMsg(HostPcStateMsg.DISTRACT(), true, new() { { "current_trial", session.TrialNum } });
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.DISTRACT(), new() { { "current_trial", session.TrialNum } });

        textDisplayer.Display("display distractor fixation cross", LangStrings.Blank(), LangStrings.GenForCurrLang("+"));
        await Timing.Delay(Config.distractorDurationMs);
        textDisplayer.Clear();

        SendRamulatorStateMsg(HostPcStateMsg.DISTRACT(), false, new() { { "current_trial", session.TrialNum } });
    }
    protected async Task MathDistractor() {
        SendRamulatorStateMsg(HostPcStateMsg.DISTRACT(), true, new() { { "current_trial", session.TrialNum } });
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.DISTRACT(), new() { { "current_trial", session.TrialNum } });

        const int numValuesInEquation = 3;
        mathDiplayer.SetNewRandomEquation(numValuesInEquation);

        var startTime = Clock.UtcNow;
        while (true) {
            var keyCode = await inputManager.WaitForKey();
            var key = keyCode.ToString();

            // Enter only numbers
            if (IsNumericKeyCode(keyCode)) {
                key = key[key.Length - 1].ToString(); // Unity gives numbers as Alpha# or Keypad#
                mathDiplayer.AddDigitToAnswer(int.Parse(key));
            }
            // Delete key removes last character from answer
            else if (keyCode == KeyCode.Backspace || keyCode == KeyCode.Delete) {
                mathDiplayer.RemoveDigitFromAnswer();
            }
            // Submit answer
            else if (keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter) {
                (bool correct, int responseTimeMs) = mathDiplayer.SubmitAnswer();

                // Play tone depending on right or wrong answer
                if (correct) {
                    manager.lowBeep.Play();
                } else {
                    manager.lowerBeep.Play();
                }

                // Report results
                manager.ramulator?.SendMathMsg(mathDiplayer.Problem, mathDiplayer.Answer, responseTimeMs, correct);

                // End distractor or setup next math problem
                if ((Clock.UtcNow - startTime).TotalMilliseconds > Config.distractorDurationMs) {
                    mathDiplayer.TurnOff();
                    break;
                } else {
                    mathDiplayer.SetNewRandomEquation(numValuesInEquation);
                }
            }
        }

        SendRamulatorStateMsg(HostPcStateMsg.DISTRACT(), false, new() { { "current_trial", session.TrialNum } });;
    }
    protected async Task PauseBeforeRecall() {
        int[] limits = Config.recallDelayMs;
        int interval = UnityEPL.Random.Rnd.Next(limits[0], limits[1]);
        await Timing.Delay(interval);
    }
    protected async Task RecallOrientation() {
        await Orientation(Config.recallOrientationDurationMs);
    }
    protected async Task FreeRecall() {
        SendRamulatorStateMsg(HostPcStateMsg.RETRIEVAL(), true, new() { { "current_trial", session.TrialNum } });
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.RETRIEVAL(), new() { { "current_trial", session.TrialNum } });

        string wavPath = Path.Combine(manager.fileManager.SessionPath(), session.TrialNum + ".wav");
        bool stim = session.Trial.recallStim;

        if (stim) {
            RecallStim();
        }

        manager.recorder.StartRecording(wavPath);
        eventReporter.LogTS("start recall period");
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.RECALL(Config.recallDurationMs));

        await Timing.Delay(Config.recallDurationMs);

        manager.recorder.StopRecording();
        manager.lowBeep.Play();
        eventReporter.LogTS("end recall period");

        SendRamulatorStateMsg(HostPcStateMsg.RETRIEVAL(), false, new() { { "current_trial", session.TrialNum } });
    }

    // Setup Functions
    protected virtual Task SetupWordList() {
        // Validate word repeats and counts
        var wordRepeats = Config.wordRepeats;
        var wordCounts = Config.wordCounts;
        if (wordRepeats.Count() != 1 && wordRepeats[0] != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's wordRepeats should only have one item with a value of 1"));
        } else if (wordCounts.Count() != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's wordCounts should only have one item in it"));
        }

        // Set member variables
        wordsPerList = wordCounts[0];

        // Read words and generate the random subset needed
        var sourceWords = ReadWordpool<WordType>(manager.fileManager.GetWordList(), "wordpool");
        var words = new WordRandomSubset<WordType>(sourceWords);

        // TODO: (feature) Load Session
        session = GenerateSession(words);

        return Task.CompletedTask;
    }

    // Helper Functions
    protected void RecallStim() {
        // Uniform stim.
        int recStimInterval = Config.recallStimIntervalMs;
        int stimDuration = Config.recallStimDurationMs;
        int recPeriod = Config.recallDurationMs;

        uint stimReps = (uint)(recPeriod / (stimDuration + recStimInterval));

        int total_interval = stimDuration + recStimInterval;
        int stim_time = total_interval;

        DoRepeating(0, stim_time, stimReps, RecallStimHelper);
    }
    protected void RecallStimHelper() {
        eventReporter.LogTS("recall stimulus info");
        manager.hostPC?.SendStimMsgTS();
    }

    // Experiment Saving and Loading Logic
    protected void WriteLstFile(StimWordList<WordType> list, int index, bool practice) {
        // create .lst files for annotation scripts
        string practiceStr = practice ? "practice_" : "";
        string fileName = practiceStr + index.ToString() + ".lst";
        string lstfile = Path.Combine(manager.fileManager.SessionPath(), fileName);
        IList<string> noRepeats = new HashSet<string>(list.words.Select(wordStim => wordStim.word)).ToList();
        File.WriteAllLines(lstfile, noRepeats, System.Text.Encoding.UTF8);
    }

    // Word/Stim List Generation
    protected virtual List<T> ReadWordpool<T>(string wordpoolPath, string copyPathName = null) 
        where T : Word, new() 
    {
        // wordpool is a file with 'word' as a header and one word per line.
        // repeats are described in the config file with two matched arrays,
        // repeats and counts, which describe the number of presentations
        // words can have and the number of words that should be assigned to
        // each of those presentation categories.
        var sourceWords = new List<T>();

        //skip line for tsv header
        foreach (var line in File.ReadLines(wordpoolPath).Skip(1)) {
            T item = (T)Activator.CreateInstance(typeof(T), line);
            sourceWords.Add(item);
        }

        // copy full wordpool to session directory
        if (copyPathName != null) {
            string copyPath = Path.Combine(manager.fileManager.SessionPath(), copyPathName + ".tsv");
            File.Copy(wordpoolPath, copyPath, true);
        }

        return sourceWords;
    }
    protected virtual TrialType MakeRun<T>(T randomSubset, bool encStim, bool recallStim) 
        where T : WordRandomSubset<WordType>
    {
        var inputWords = randomSubset.Get(wordsPerList).ToList();
        var encList = GenOpenLoopStimList(inputWords, encStim);
        var recallList = GenOpenLoopStimList(inputWords, recallStim);

        return (TrialType)(new FRTrial<WordType>(encList, recallList, encStim, recallStim));
    }

    protected StimWordList<WordType> GenOpenLoopStimList(List<WordType> inputWords, bool stim) {
        if (stim) {
            // var halfNumWords = wordsPerList / 2;
            // var falses = Enumerable.Range(1, halfNumWords).Select(i => false).ToList();
            // var trues = Enumerable.Range(1, wordsPerList-halfNumWords).Select(i => true).ToList();
            // var stimList = falses.Concat(trues).ToList().Shuffle();
            var stimList = Enumerable.Range(1, wordsPerList)
                            .Select(i => UnityEPL.Random.Rnd.NextDouble() >= 0.5)
                            .ToList();
            return new StimWordList<WordType>(inputWords, stimList);
        } else {
            var stimList = Enumerable.Range(1, wordsPerList)
                            .Select(i => false)
                            .ToList();
            return new StimWordList<WordType>(inputWords, stimList);
        }
    }
    protected virtual SessionType GenerateSession<T>(T randomSubset) 
            where T : WordRandomSubset<WordType>
    {
        var session = new SessionType();

        for (int i = 0; i < Config.practiceLists; i++) {
            session.AddTrial(MakeRun(randomSubset, false, false));
        }

        for (int i = 0; i < Config.preNoStimLists; i++) {
            session.AddTrial(MakeRun(randomSubset, false, false));
        }

        var randomized_list = new SessionType();

        for (int i = 0; i < Config.encodingOnlyLists; i++) {
            randomized_list.AddTrial(MakeRun(randomSubset, true, false));
        }

        for (int i = 0; i < Config.retrievalOnlyLists; i++) {
            randomized_list.AddTrial(MakeRun(randomSubset, false, true));
        }

        for (int i = 0; i < Config.encodingAndRetrievalLists; i++) {
            randomized_list.AddTrial(MakeRun(randomSubset, true, true));
        }

        for (int i = 0; i < Config.noStimLists; i++) {
            randomized_list.AddTrial(MakeRun(randomSubset, false, false));
        }

        // TODO: JPB: (needed) (bug) All shuffles in FRExperiment, RepWordGenerator, Timeline, and FRSession may need to be ShuffleInPlace
        session.AddTrials(randomized_list.trials.Shuffle());

        for (int i = 0; i < session.trials.Count; i++) {
            WriteLstFile(session.trials[i].encoding, i, false);
        }

        session.PrintAllWordsToDebugLog();

        return session;
    }
}