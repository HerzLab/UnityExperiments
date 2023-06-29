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

    public class RepFRExperiment2 : FRExperiment {
        protected RepCounts repCounts = null;
        protected int uniqueWordsPerList;
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
                SendRamulatorStateMsg(HostPC.StateMsg.INSTRUCT, true);
                manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.INSTRUCT);
                await manager.videoControl.PlayVideo();
                SendRamulatorStateMsg(HostPC.StateMsg.INSTRUCT, false);
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
            eventReporter.LogTS("countdown");
            SendRamulatorStateMsg(HostPC.StateMsg.COUNTDOWN, true);
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.COUNTDOWN);
            manager.videoControl.SetVideo(Config.countdownVideo);
            await manager.videoControl.PlayVideo();
            SendRamulatorStateMsg(HostPC.StateMsg.COUNTDOWN, false);
        }
        protected async Task Orientation() {
            SendRamulatorStateMsg(HostPC.StateMsg.ORIENT, true);
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.ORIENT);

            int[] limits = Config.orientationDuration;
            int duration = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
            textDisplayer.Display("orientation stimulus", "", "+");
            
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

            SendRamulatorStateMsg(HostPC.StateMsg.DISTRACT, false, new() { { "current_trial", trialNum } });
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
            blankWords = new List<string>(Enumerable.Repeat(string.Empty, wordsPerList));

            var sourceWords = ReadWordpool();
            var words = new WordRandomSubset(sourceWords);

            // TODO: (feature) Load Session
            currentSession = GenerateSession(words);
        }

        // Word/Stim List Generation
        protected override FRRun MakeRun<T>(T subsetGen, bool encStim, bool recStim) {
            var inputWords = subsetGen.Get(uniqueWordsPerList).Select(x => x.word).ToList();
            var encList = RepWordGenerator.Generate(repCounts, inputWords, encStim);
            var recList = RepWordGenerator.Generate(repCounts, blankWords, recStim);
            return new FRRun(encList, recList, encStim, recStim);
        }
    }

}