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

    public class FRExperiment : ExperimentBase<FRExperiment> {
        protected override void AwakeOverride() { }

        protected void Start() {
            Run();
        }

        protected List<string> blankWords;
        protected int wordsPerList;
        protected FRSession currentSession;

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
            await Recall();
            FinishPracticeTrial();
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
            await Recall();
            FinishTrial();
        }

        // Pre-Trial States
        protected async Task Introduction() {
            await RepeatUntilNo(async () => {
                await textDisplayer.PressAnyKey("show instruction video", "Press any key to show instruction video");

                manager.videoControl.SetVideo(Config.introductionVideo, true);
                await manager.videoControl.PlayVideo();
            }, "repeat introduction video", "Press Y to continue to practice list, \n Press N to replay instructional video.");
        }
        protected async Task MicrophoneTest() {
            await RepeatUntilNo(async () => {
                await textDisplayer.PressAnyKey("microphone test prompt", "Microphone Test", "Press any key to record a sound after the beep.");

                string wavPath = System.IO.Path.Combine(manager.fileManager.SessionPath(), "microphone_test_"
                        + Clock.UtcNow.ToString("yyyy-MM-dd_HH_mm_ss") + ".wav");

                manager.lowBeep.Play();
                await DoWaitWhile(() => manager.lowBeep.isPlaying);
                await InterfaceManager.Delay(100); // This is needed so you don't hear the end of the beep

                manager.recorder.StartRecording(wavPath);
                manager.textDisplayer.DisplayText("microphone test recording", "<color=red>Recording...</color>");
                await InterfaceManager.Delay(Config.micTestDuration);
                var clip = manager.recorder.StopRecording();

                manager.textDisplayer.DisplayText("microphone test playing", "<color=green>Playing...</color>");
                manager.playback.Play(clip);
                await InterfaceManager.Delay(Config.micTestDuration);
            }, "repeat mic test", "Did you hear the recording ? \n(Y = Continue / N = Try Again).");
        }
        protected async Task QuitPrompt() {
            SendRamulatorStateMsg(HostPC.StateMsg.WAITING, true);
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.WAITING);

            textDisplayer.Display("subject/session confirmation", "",
                $"Running {Config.subject} in session {Config.sessionNum} of {Config.experimentName}." +
                "\nPress Y to continue, N to quit.");
            var keyCode = await inputManager.GetKeyTS(new() { KeyCode.Y, KeyCode.N });

            SendRamulatorStateMsg(HostPC.StateMsg.WAITING, false);

            if (keyCode == KeyCode.N) {
                manager.QuitTS();
            }
        }
        protected async Task ConfirmStart() {
            await textDisplayer.PressAnyKey("confirm start",
                "Please let the experimenter know if you have any questions about what you just did.\n\n" +
                "If you think you understand, please explain the task to the experimenter in your own words.\n\n" +
                "Press any key to continue to the first list.");
        }

        // Post-Trial States
        protected async Task FinalRecall() {
            // TODO: JPB: (needed) (bug) Change final recall wav file name
            string wavPath = Path.Combine(manager.fileManager.SessionPath(),
                                                "final_recall" + ".wav");

            manager.recorder.StartRecording(wavPath);
            eventReporter.LogTS("start final recall period");
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.FINAL_RECALL, new() { { "duration", Config.finalRecallDuration } });

            await InterfaceManager.Delay(Config.finalRecallDuration);

            eventReporter.LogTS("end final recall period");
            manager.recorder.StopRecording();
            manager.lowBeep.Play();
        }
        protected async Task FinishExperiment() {
            textDisplayer.Display("session end", "", "Yay! Session Complete.");
            await InterfaceManager.Delay(10000);
        }

        // Trial States
        protected void StartTrial() {
            // TODO: JPB: (needed) (bug) Change stim value to a real value
            Dictionary<string, object> data = new() {
                { "trial", trialNum },
                { "stim", false }
            };
            
            eventReporter.LogTS("start trial", data);
            manager.ramulator?.BeginNewTrial((int)trialNum);
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.TRIAL, data);
        }
        protected async Task NextListPrompt() {
            await textDisplayer.PressAnyKey("pause before list", $"Press any key for trial {trialNum}.");
        }
        protected async Task NextPracticeListPrompt() {
            await textDisplayer.PressAnyKey("pause before list", "Press any key for practice trial.");
        }
        protected async Task CountdownVideo() {
            SendRamulatorStateMsg(HostPC.StateMsg.COUNTDOWN, true, new() { { "current_trial", trialNum } });
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.COUNTDOWN, new() { { "current_trial", trialNum } });

            eventReporter.LogTS("countdown");
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.COUNTDOWN);
            manager.videoControl.SetVideo(Config.countdownVideo);
            await manager.videoControl.PlayVideo();

            SendRamulatorStateMsg(HostPC.StateMsg.COUNTDOWN, false, new() { { "current_trial", trialNum } });
        }
        protected async Task Orientation() {
            SendRamulatorStateMsg(HostPC.StateMsg.ORIENT, true);
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.ORIENT);

            int[] limits = Config.orientationDuration;
            int duration = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
            textDisplayer.Display("orientation stimulus", "", "+");
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.ORIENT);
            await InterfaceManager.Delay(duration);

            SendRamulatorStateMsg(HostPC.StateMsg.ORIENT, false);
        }
        protected async Task Encoding() {
            SendRamulatorStateMsg(HostPC.StateMsg.ENCODING, true, new() { { "current_trial", trialNum } });
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.ENCODING, new() { { "current_trial", trialNum } });

            int[] isiLimits = Config.interStimulusDuration;
 
            for (int i = 0; i < 12; ++i) {
                int isiDuration = InterfaceManager.rnd.Value.Next(isiLimits[0], isiLimits[1]);
                manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.ISI, new() { { "duration", isiDuration } });
                await InterfaceManager.Delay(isiDuration);

                var word = currentSession.GetWord();
                currentSession.NextWord();
                Dictionary<string, object> data = new() {
                    { "word", word.word },
                    { "serialpos", i },
                    { "stim", word.stim },
                };

                eventReporter.LogTS("word stimulus info", data);
                manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.WORD, data);
                textDisplayer.Display("word stimulus", "", word.word);
                await InterfaceManager.Delay(Config.stimulusDuration);
                eventReporter.LogTS("clear word stimulus", data);
                textDisplayer.Clear();
            }

            SendRamulatorStateMsg(HostPC.StateMsg.ENCODING, false, new() { { "current_trial", trialNum } });
        }
        protected async Task FixationDistractor() {
            SendRamulatorStateMsg(HostPC.StateMsg.DISTRACT, true, new() { { "current_trial", trialNum } });
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.DISTRACT, new() { { "current_trial", trialNum } });

            textDisplayer.Display("display distractor fixation cross", "", "+");
            await InterfaceManager.Delay(Config.distractorDuration);
            textDisplayer.Clear();

            SendRamulatorStateMsg(HostPC.StateMsg.DISTRACT, false, new() { { "current_trial", trialNum } });
        }
        protected async Task MathDistractor() {
            SendRamulatorStateMsg(HostPC.StateMsg.DISTRACT, true, new() { { "current_trial", trialNum } });
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.DISTRACT, new() { { "current_trial", trialNum } });

            int[] nums = new int[] {
                InterfaceManager.rnd.Value.Next(1, 10),
                InterfaceManager.rnd.Value.Next(1, 10),
                InterfaceManager.rnd.Value.Next(1, 10) };
            string message = "display distractor problem";
            string problem = nums[0].ToString() + " + " +
                             nums[1].ToString() + " + " +
                             nums[2].ToString() + " = ";
            string answer = "";

            var startTime = Clock.UtcNow;
            var displayTime = startTime;
            while (true) {
                textDisplayer.Display(message, "", problem + answer);
                var keyCode = await inputManager.GetKeyTS();
                var key = keyCode.ToString();

                // Enter only numbers
                if (IsNumericKeyCode(keyCode)) {
                    key = key[key.Length - 1].ToString(); // Unity gives numbers as Alpha# or Keypad#
                    if (answer.Length < 3) {
                        answer += key;
                    }
                    message = "modify distractor answer";
                }
                // Delete key removes last character from answer
                else if (keyCode == KeyCode.Backspace || keyCode == KeyCode.Delete) {
                    if (answer != "") {
                        answer = answer.Substring(0, answer.Length - 1);
                    }
                    message = "modify distractor answer";
                }
                // Submit answer
                else if (keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter) {
                    bool correct = int.Parse(answer) == nums.Sum();

                    // Play tone depending on right or wrong answer
                    if (correct) {
                        manager.lowBeep.Play();
                    } else {
                        manager.lowerBeep.Play();
                    }

                    // Report results
                    message = "distractor answered";
                    textDisplayer.Display(message, "", problem + answer);
                    int responseTimeMs = (int)(Clock.UtcNow - displayTime).TotalMilliseconds;
                    Dictionary<string, object> dict = new() {
                        { "correct", correct },
                        { "problem", problem },
                        { "answer", answer },
                        { "responseTime", responseTimeMs }
                    };
                    eventReporter.LogTS(message, dict);
                    manager.ramulator?.SendMathMsg(problem, answer, responseTimeMs, correct);
                    manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.MATH, dict);

                    // End distractor or setup next math problem
                    if ((Clock.UtcNow - startTime).TotalMilliseconds > Config.distractorDuration) {
                        textDisplayer.Clear();
                        break;
                    } else {
                        nums = new int[] { InterfaceManager.rnd.Value.Next(1, 10),
                                           InterfaceManager.rnd.Value.Next(1, 10),
                                           InterfaceManager.rnd.Value.Next(1, 10) };
                        message = "display distractor problem";
                        problem = nums[0].ToString() + " + " +
                                  nums[1].ToString() + " + " +
                                  nums[2].ToString() + " = ";
                        answer = "";
                        textDisplayer.Display(message, "", problem + answer);
                        displayTime = Clock.UtcNow;
                    }
                }
            }

            SendRamulatorStateMsg(HostPC.StateMsg.DISTRACT, false, new() { { "current_trial", trialNum } });;
        }
        protected async Task PauseBeforeRecall() {
            int[] limits = Config.recallDelay;
            int interval = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
            await InterfaceManager.Delay(interval);
        }
        protected async Task RecallPrompt() {
            manager.highBeep.Play();
            textDisplayer.Display("display recall text", "", "*******");
            await InterfaceManager.Delay(Config.recallPromptDuration);
        }
        protected async Task Recall() {
            SendRamulatorStateMsg(HostPC.StateMsg.RETRIEVAL, true, new() { { "current_trial", trialNum } });
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.RETRIEVAL, new() { { "current_trial", trialNum } });

            string wavPath = Path.Combine(manager.fileManager.SessionPath(), currentSession.GetListIndex() + ".wav");
            bool stim = currentSession.GetState().recallStim;

            manager.recorder.StartRecording(wavPath);
            eventReporter.LogTS("start recall period");
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.RECALL, new() { { "duration", Config.recallDuration } });

            await InterfaceManager.Delay(Config.recallDuration);

            manager.recorder.StopRecording();
            manager.lowBeep.Play();
            eventReporter.LogTS("end recall period");

            if (stim) {
                RecallStim();
            }

            SendRamulatorStateMsg(HostPC.StateMsg.RETRIEVAL, false, new() { { "current_trial", trialNum } });
        }
        protected void FinishTrial() {
            if(!currentSession.NextList()) {
                EndTrials();
            }
        }
        protected void FinishPracticeTrial() {
            if (!currentSession.NextList()) {
                EndPracticeTrials();
                EndTrials();
            }
            if (practiceTrialNum > Config.practiceLists) {
                EndPracticeTrials();
            }
        }

        // Setup Functions
        protected virtual void SetupWordList() {
            var wordRepeats = Config.wordRepeats;
            if (wordRepeats.Count() != 1 && wordRepeats[0] != 1) {
                ErrorNotifier.ErrorTS(new Exception("Config's wordRepeats should only have one item with a value of 1"));
            }

            wordsPerList = Config.wordCounts[0];
            blankWords = new List<string>(Enumerable.Repeat(string.Empty, wordsPerList));

            var sourceWords = ReadWordpool();
            var words = new WordRandomSubset(sourceWords);

            // TODO: (feature) Load Session
            currentSession = GenerateSession(words);
        }

        // Wrapper/Replacement Functions
        protected bool IsNumericKeyCode(KeyCode keyCode) {
            bool isAlphaNum = keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9;
            bool isKeypadNum = keyCode >= KeyCode.Keypad0 && keyCode <= KeyCode.Keypad9;
            return isAlphaNum || isKeypadNum;
        }
        protected void SendRamulatorStateMsg(HostPC.StateMsg state, bool stateToggle, Dictionary<string, object> extraData = null) {
            var dict = (extraData != null) ? new Dictionary<string, object>(extraData) : new();
            if (state != HostPC.StateMsg.WORD) {
                dict["phase_type"] = currentSession.GetState().encodingStim;
            }
            manager.ramulator?.SendStateMsg(state, stateToggle, dict);
        }
        protected new async Task RepeatUntilNo(Func<Task> func, string description, string displayText) {
            var repeat = true;
            while (repeat) {
                await func();

                SendRamulatorStateMsg(HostPC.StateMsg.WAITING, true);
                textDisplayer.Display(description, "", displayText);
                var keyCode = await inputManager.GetKeyTS(new() { KeyCode.Y, KeyCode.N });
                repeat = keyCode == KeyCode.N;
                SendRamulatorStateMsg(HostPC.StateMsg.WAITING, false);
            }
        }
        protected new async Task RepeatUntilYes(Func<Task> func, string description, string displayText) {
            var repeat = true;
            while (repeat) {
                await func();

                SendRamulatorStateMsg(HostPC.StateMsg.WAITING, true);
                textDisplayer.Display(description, "", displayText);
                var keyCode = await inputManager.GetKeyTS(new() { KeyCode.Y, KeyCode.N });
                repeat = keyCode == KeyCode.Y;
                SendRamulatorStateMsg(HostPC.StateMsg.WAITING, false);
            }
        }

        // Helper Functions
        protected void RecallStim() {
            // Uniform stim.
            int recStimInterval = Config.recallStimInterval;
            int stimDuration = Config.recallStimDuration;
            int recPeriod = Config.recallDuration;

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
        protected void WriteLstFile(StimWordList list, int index) {
            // create .lst files for annotation scripts
            string lstfile = Path.Combine(manager.fileManager.SessionPath(), index.ToString() + ".lst");
            IList<string> noRepeats = new HashSet<string>(list.words).ToList();
            File.WriteAllLines(lstfile, noRepeats, System.Text.Encoding.UTF8);
        }

        // Word/Stim List Generation
        protected virtual List<Word> ReadWordpool() {
            // wordpool is a file with 'word' as a header and one word per line.
            // repeats are described in the config file with two matched arrays,
            // repeats and counts, which describe the number of presentations
            // words can have and the number of words that should be assigned to
            // each of those presentation categories.
            string source_list = manager.fileManager.GetWordList();
            var source_words = new List<Word>();

            //skip line for csv header
            foreach (var line in File.ReadLines(source_list).Skip(1)) {
                source_words.Add(new Word(line));
            }

            // copy wordpool to session directory
            string path = System.IO.Path.Combine(manager.fileManager.SessionPath(), "wordpool.txt");
            File.Copy(source_list, path, true);

            return source_words;
        }
        protected virtual FRRun MakeRun<T>(T subsetGen, bool encStim, bool recStim) where T : WordRandomSubset {
            var inputWords = subsetGen.Get(wordsPerList).Select(x => x.word).ToList();
            var encList = WordGenerator.Generate(inputWords, encStim);
            var recList = WordGenerator.Generate(blankWords, recStim);
            return new FRRun(encList, recList, encStim, recStim);
        }
        protected virtual FRSession GenerateSession<T>(T randomSubset) where T : WordRandomSubset {
            var session = new FRSession();

            for (int i = 0; i < Config.practiceLists; i++) {
                session.Add(MakeRun(randomSubset, false, false));
            }

            for (int i = 0; i < Config.preNoStimLists; i++) {
                session.Add(MakeRun(randomSubset, false, false));
            }

            var randomized_list = new FRSession();

            for (int i = 0; i < Config.encodingOnlyLists; i++) {
                randomized_list.Add(MakeRun(randomSubset, true, false));
            }

            for (int i = 0; i < Config.retrievalOnlyLists; i++) {
                randomized_list.Add(MakeRun(randomSubset, false, true));
            }

            for (int i = 0; i < Config.encodingAndRetrievalLists; i++) {
                randomized_list.Add(MakeRun(randomSubset, true, true));
            }

            for (int i = 0; i < Config.noStimLists; i++) {
                randomized_list.Add(MakeRun(randomSubset, false, false));
            }

            // TODO: JPB: (needed) (bug) All shuffles in FRExperiment, RepWordGenerator, Timeline, and FRSession may need to be ShuffleInPlace
            session.AddRange(randomized_list.Shuffle());

            for (int i = 0; i < session.Count; i++) {
                WriteLstFile(session[i].encoding, i);
            }

            session.PrintAllWordsToDebugLog();

            return session;
        }
    }

}