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

        protected List<string> source_words;
        protected List<string> blank_words;
        protected RepCounts rep_counts = null;
        protected int words_per_list;
        protected int unique_words_per_list;
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
            manager.hostPC?.SendStateMsg(HostPC.StateMsg.WAITING);

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
            manager.hostPC?.SendStateMsg(HostPC.StateMsg.FINAL_RECALL, new() { { "duration", Config.finalRecallDuration } });

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
            manager.hostPC?.SendStateMsg(HostPC.StateMsg.TRIAL, data);
        }
        protected async Task NextListPrompt() {
            await textDisplayer.PressAnyKey("pause before list", $"Press any key for trial {trialNum}.");
        }
        protected async Task NextPracticeListPrompt() {
            await textDisplayer.PressAnyKey("pause before list", "Press any key for practice trial.");
        }
        protected async Task CountdownVideo() {
            eventReporter.LogTS("countdown");
            manager.hostPC?.SendStateMsg(HostPC.StateMsg.COUNTDOWN);
            manager.videoControl.SetVideo(Config.countdownVideo);
            await manager.videoControl.PlayVideo();
        }
        protected async Task Orientation() {
            int[] limits = Config.orientationDuration;
            int duration = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
            textDisplayer.Display("orientation stimulus", "", "+");
            manager.hostPC?.SendStateMsg(HostPC.StateMsg.ORIENT);
            await InterfaceManager.Delay(duration);
        }
        protected async Task Encoding() {
            int[] isiLimits = Config.stimulusInterval;
 
            for (int i = 0; i < 12; ++i) {
                int isiDuration = InterfaceManager.rnd.Value.Next(isiLimits[0], isiLimits[1]);
                manager.hostPC?.SendStateMsg(HostPC.StateMsg.ISI, new() { { "duration", isiDuration } });
                await InterfaceManager.Delay(isiDuration);

                var word = currentSession.GetWord();
                currentSession.NextWord();
                Dictionary<string, object> data = new() {
                    { "word", word.word },
                    { "serialpos", i },
                    { "stim", word.stim },
                };

                eventReporter.LogTS("word stimulus info", data);
                manager.hostPC?.SendStateMsg(HostPC.StateMsg.WORD, data);
                textDisplayer.Display("word stimulus", "", word.word);
                await InterfaceManager.Delay(Config.stimulusDuration);
                eventReporter.LogTS("clear word stimulus", data);
                textDisplayer.Clear();
            }
        }
        protected async Task MathDistractor() {
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
                    manager.ramulator.SendMathMsg(problem, answer, responseTimeMs, correct);
                    manager.hostPC?.SendStateMsg(HostPC.StateMsg.MATH, dict);

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
            string wavPath = Path.Combine(manager.fileManager.SessionPath(), currentSession.GetListIndex() + ".wav");
            bool stim = currentSession.GetState().recallStim;

            manager.recorder.StartRecording(wavPath);
            eventReporter.LogTS("start recall period");
            manager.hostPC?.SendStateMsg(HostPC.StateMsg.RECALL, new() { { "duration", Config.recallDuration } });

            await InterfaceManager.Delay(Config.recallDuration);

            manager.recorder.StopRecording();
            manager.lowBeep.Play();
            eventReporter.LogTS("end recall period");

            if (stim) {
                RecallStim();
            }
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


        // Helper Functions
        protected void RecallStim() {
            // Uniform stim.
            int recstimInterval = Config.recStimulusInterval;
            int stimDuration = Config.stimulusDuration;
            int recPeriod = Config.recallDuration;
            uint stimReps = (uint)(recPeriod / (stimDuration + recstimInterval));

            int total_interval = stimDuration + recstimInterval;
            int stim_time = total_interval;

            DoRepeating(0, stim_time, stimReps, RecallStimHelper);
        }
        protected void RecallStimHelper() {
            eventReporter.LogTS("recall stimulus info");
            manager.hostPC?.SendStimMsg();
        }
        protected void SetupWordList() {
            // Repetition specification:
            int[] repeats = Config.wordRepeats;
            int[] counts = Config.wordCounts;

            if (repeats.Length != counts.Length) {
                throw new Exception("Word Repeats and Counts not aligned");
            }

            for (int i = 0; i < repeats.Length; i++) {
                if (rep_counts == null) {
                    rep_counts = new RepCounts(repeats[i], counts[i]);
                } else {
                    rep_counts = rep_counts.RepCnt(repeats[i], counts[i]);
                }
            }

            // boilerplate needed by RepWordGenerator
            words_per_list = rep_counts.TotalWords();
            unique_words_per_list = rep_counts.UniqueWords();
            blank_words = new List<string>(Enumerable.Repeat(string.Empty, words_per_list));
            source_words = ReadWordpool();

            // TODO: Load Session
            currentSession = GenerateSession();
        }
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
            manager.ramulator.SendStateMsg(state, stateToggle, dict);
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

        // Experiment Saving and Loading Logic
        public void WriteLstFile(StimWordList list, int index) {
            // create .lst files for annotation scripts
            string lstfile = Path.Combine(manager.fileManager.SessionPath(), index.ToString() + ".lst");
            IList<string> noRepeats = new HashSet<string>(list.words).ToList();
            File.WriteAllLines(lstfile, noRepeats, System.Text.Encoding.UTF8);
        }

        // Word/Stim List Generation
        public List<string> ReadWordpool() {
            // wordpool is a file with 'word' as a header and one word per line.
            // repeats are described in the config file with two matched arrays,
            // repeats and counts, which describe the number of presentations
            // words can have and the number of words that should be assigned to
            // each of those presentation categories.
            string source_list = manager.fileManager.GetWordList();
            source_words = new List<string>();

            //skip line for csv header
            foreach (var line in File.ReadLines(source_list).Skip(1)) {
                source_words.Add(line);
            }

            // copy wordpool to session directory
            string path = System.IO.Path.Combine(manager.fileManager.SessionPath(), "wordpool.txt");
            System.IO.File.Copy(source_list, path, true);

            return source_words;

        }
        public FRRun MakeRun(RandomSubset subsetGen, bool encStim, bool recStim) {
            var enclist = RepWordGenerator.Generate(rep_counts, subsetGen.Get(unique_words_per_list), encStim);
            var reclist = RepWordGenerator.Generate(rep_counts, blank_words, recStim);
            return new FRRun(enclist, reclist, encStim, recStim);
        } 
        public FRSession GenerateSession() {
            // Parameters retrieved from experiment config, given default
            // value if null.
            // Numbers of list types:
            int practiceLists = Config.practiceLists;
            int preNoStimLists = Config.preNoStimLists;
            int encodingOnlyLists = Config.encodingOnlyLists;
            int retrievalOnlyLists = Config.retrievalOnlyLists;
            int encodingAndRetrievalLists = Config.encodingAndRetrievalLists;
            int noStimLists = Config.noStimLists;

            RandomSubset subsetGen = new RandomSubset(source_words);

            var session = new FRSession();

            for (int i = 0; i < practiceLists; i++) {
                session.Add(MakeRun(subsetGen, false, false));
            }

            for (int i = 0; i < preNoStimLists; i++) {
                session.Add(MakeRun(subsetGen, false, false));
            }

            var randomized_list = new FRSession();

            for (int i = 0; i < encodingOnlyLists; i++) {
                randomized_list.Add(MakeRun(subsetGen, true, false));
            }

            for (int i = 0; i < retrievalOnlyLists; i++) {
                randomized_list.Add(MakeRun(subsetGen, false, true));
            }

            for (int i = 0; i < encodingAndRetrievalLists; i++) {
                randomized_list.Add(MakeRun(subsetGen, true, true));
            }

            for (int i = 0; i < noStimLists; i++) {
                randomized_list.Add(MakeRun(subsetGen, false, false));
            }

            // TODO: JPB: (needed) (bug) All shuffles in FRExperiment, RepWordGenerator, Timeline, and FRSession may need to be ShuffleInPlace
            session.AddRange(randomized_list.Shuffle());

            for (int i = 0; i < session.Count; i++) {
                WriteLstFile(session[i].encoding, i);
            }

            return session;
        }


    }

}