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
    public class FRExperimentBase<T> : ExperimentBase<FRExperimentBase<T>> 
        where T : Word, new() 
    {
        protected override void AwakeOverride() { }

        protected void Start() {
            Run();
        }

        protected List<T> blankWords;
        protected int wordsPerList;
        protected FRSession<T> currentSession;

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
            await Fixation();
            await Encoding();
            await MathDistractor();
            await PauseBeforeRecall();
            await RecallPrompt();
            await FreeRecall();
            FinishPracticeTrial();
        }
        protected override async Task TrialStates() {
            StartTrial();
            await NextListPrompt();
            await CountdownVideo();
            await Fixation();
            await Encoding();
            await MathDistractor();
            await PauseBeforeRecall();
            await RecallPrompt();
            await FreeRecall();
            FinishTrial();
        }
        protected override void SendRamulatorStateMsg(HostPC.StateMsg state, bool stateToggle, Dictionary<string, object> extraData = null) {
            var dict = (extraData != null) ? new Dictionary<string, object>(extraData) : new();
            if (state != HostPC.StateMsg.WORD) {
                dict["phase_type"] = currentSession.GetState().encodingStim;
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
        protected async Task Fixation() {
            SendRamulatorStateMsg(HostPC.StateMsg.ORIENT, true);
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.ORIENT);

            int[] limits = Config.fixationDuration;
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

                var wordStim = currentSession.GetWord();
                currentSession.NextWord();
                Dictionary<string, object> data = new() {
                    { "word", wordStim.word },
                    { "serialpos", i },
                    { "stim", wordStim.stim },
                };

                eventReporter.LogTS("word stimulus info", data);
                manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.WORD, data);
                textDisplayer.Display("word stimulus", "", wordStim.word.ToDisplayString());
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
        protected async Task FreeRecall() {
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
            blankWords = new List<T>(Enumerable.Range(1, wordsPerList).Select(i => new T()).ToList());

            var sourceWords = ReadWordpool();
            var words = new WordRandomSubset<T>(sourceWords);

            // TODO: (feature) Load Session
            currentSession = GenerateSession(words);
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
        protected void WriteLstFile(StimWordList<T> list, int index) {
            // create .lst files for annotation scripts
            string lstfile = Path.Combine(manager.fileManager.SessionPath(), index.ToString() + ".lst");
            IList<string> noRepeats = new HashSet<string>(list.words.Select(wordStim => wordStim.word)).ToList();
            File.WriteAllLines(lstfile, noRepeats, System.Text.Encoding.UTF8);
        }

        // Word/Stim List Generation
        // TODO: JPB: (Noa) (feature) Change FRExperiment::ReadWordpool to be generic for reading from line
        protected virtual List<T> ReadWordpool() {
            // wordpool is a file with 'word' as a header and one word per line.
            // repeats are described in the config file with two matched arrays,
            // repeats and counts, which describe the number of presentations
            // words can have and the number of words that should be assigned to
            // each of those presentation categories.
            string sourceList = manager.fileManager.GetWordList();
            var sourceWords = new List<T>();

            //skip line for tsv header
            foreach (var line in File.ReadLines(sourceList).Skip(1)) {
                T item = (T)Activator.CreateInstance(typeof(T), line);
                sourceWords.Add(item);
            }

            // copy full wordpool to session directory
            string path = System.IO.Path.Combine(manager.fileManager.SessionPath(), "wordpool.txt");
            File.WriteAllText(path, String.Join("\n", sourceWords));

            return sourceWords;
        }
        protected virtual FRRun<T> MakeRun<U>(U randomSubset, bool encStim, bool recStim) 
                where U : WordRandomSubset<T>
        {
            var inputWords = randomSubset.Get(wordsPerList).ToList();
            var encList = GenOpenLoopStimList(inputWords, encStim);
            var recList = GenOpenLoopStimList(inputWords, recStim);
            return new FRRun<T>(encList, recList, encStim, recStim);
        }

        protected StimWordList<T> GenOpenLoopStimList(List<T> inputWords, bool stim) {
            if (stim) {
                // var halfNumWords = wordsPerList / 2;
                // var falses = Enumerable.Range(1, halfNumWords).Select(i => false).ToList();
                // var trues = Enumerable.Range(1, wordsPerList-halfNumWords).Select(i => true).ToList();
                // var stimList = falses.Concat(trues).ToList().Shuffle();
                var stimList = Enumerable.Range(1, wordsPerList)
                                .Select(i => InterfaceManager.rnd.Value.NextDouble() >= 0.5)
                                .ToList();
                return new StimWordList<T>(inputWords, stimList);
            } else {
                var stimList = Enumerable.Range(1, wordsPerList)
                                .Select(i => false)
                                .ToList();
                return new StimWordList<T>(inputWords, stimList);
            }
        }

        protected virtual FRSession<T> GenerateSession<U>(U randomSubset) 
                where U : WordRandomSubset<T>
        {
            var session = new FRSession<T>();

            for (int i = 0; i < Config.practiceLists; i++) {
                session.Add(MakeRun(randomSubset, false, false));
            }

            for (int i = 0; i < Config.preNoStimLists; i++) {
                session.Add(MakeRun(randomSubset, false, false));
            }

            var randomized_list = new FRSession<T>();

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