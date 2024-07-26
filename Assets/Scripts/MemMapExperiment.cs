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
using System.Threading.Tasks;
using UnityEngine;
using UnityEPL;

public class MemMapExperiment : WordListExperiment<PairedWord, MemMapTrial<PairedWord>, MemMapSession<PairedWord>> {
    protected readonly List<KeyCode> skipKeys = new List<KeyCode> {KeyCode.Space};
    protected readonly List<KeyCode> ynKeyCodes = new List<KeyCode> {KeyCode.Y, KeyCode.N};

    protected int lureWordsPerList;
    protected bool newIsLeftShift;

    [SerializeField] protected WordDisplayer wordDisplayer;
    [SerializeField] protected OldNewKeys oldNewKeys;

    protected override async Task PreTrialStates() {
        await SetupWordList();

        if (!Config.skipIntros) {
            await QuitPrompt();
            await MicrophoneTest();
            await Introduction();
            await ConfirmStart();
        }
    }
    protected override async Task PostTrialStates() {
        await Questioneer();
        await FinishExperiment();
    }
    protected override async Task PracticeTrialStates() {
        await StartTrial();
        await NextPracticeTrialPrompt();
        await CountdownVideo();
        await Encoding();
        await MathDistractor();
        await PauseBeforeRecall();
        await RecallOrientation();
        await CuedRecall();
        await RecogInstructions();
        await PauseBeforeRecall();
        await RecallOrientation();
        await Recognition();
    }
    protected override async Task TrialStates() {
        await StartTrial();
        await NextTrialPrompt();
        await CountdownVideo();
        await Encoding();
        await MathDistractor();
        await PauseBeforeRecall();
        await RecallOrientation();
        await CuedRecall();
        await RecogInstructions();
        await PauseBeforeRecall();
        await RecallOrientation();
        await Recognition();
    }

    protected async Task RecogInstructions() {
        await textDisplayer.PressAnyKey("recog instructions", LangStrings.RecognitionInstructions());
    }

    protected new async Task Fixation() {
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ORIENT());

        // Delay for random time within fixation duration limits and show orientation stimulus
        int[] limits = Config.fixationDurationMs;
        int duration = UnityEPL.Random.Rnd.Next(limits[0], limits[1]);
        textDisplayer.Display("orientation stimulus", LangStrings.Blank(), LangStrings.GenForCurrLang("+"));
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ORIENT());
        await Timing.Delay(duration);

        // Delay for random time within post-fixation delay limits
        textDisplayer.Clear();
        limits = Config.postFixationDelayMs;
        duration = UnityEPL.Random.Rnd.Next(limits[0], limits[1]);
        await Timing.Delay(duration);
    }

    protected new async Task Encoding() {
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ENCODING(), new() { { "current_trial", session.TrialNum } });

        // Get encoding state variables
        int[] isiLimits = Config.interStimulusDurationMs;
        int[] stimEarlyOnsetMsLimits = Config.stimEarlyOnsetMs;
        var encStimWords = session.Trial.encoding;

        for (int i = 0; i < encStimWords.Count; ++i) {
            var wordStim = encStimWords[i];

            // Determine the Inter Stimulus Interval (ISI), when stim should start, and adjust ISI
            int isiDuration = UnityEPL.Random.Rnd.Next(isiLimits[0], isiLimits[1]);
            int stimEarlyDuration = UnityEPL.Random.Rnd.Next(stimEarlyOnsetMsLimits[0], stimEarlyOnsetMsLimits[1]);
            isiDuration -= stimEarlyDuration;

            // Do the ISI
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(isiDuration));
            await Timing.Delay(isiDuration);

            // Do the stim and wait the rest of the ISI
            if (wordStim.stim) { manager.hostPC?.SendStimMsgTS(); }
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(stimEarlyDuration));
            await Timing.Delay(stimEarlyDuration);

            // Do the encoding and log it
            Dictionary<string, object> data = new() {
                { "word", wordStim.word },
                { "serialpos", i },
                { "stimWord", wordStim.stim },
            };
            eventReporter.LogTS("word stimulus info", data);
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.WORD(), data);
            wordDisplayer.DisplayPairedWord(wordStim.word.word, wordStim.word.pairedWord);
            await Timing.Delay(Config.stimulusDurationMs);
            wordDisplayer.ClearWords();

            // manager.lowBeep.Play();
        }
        wordDisplayer.TurnOff();
    }

    protected async Task PauseBeforeRecog() {
        int[] limits = Config.recallDelayMs;
        int interval = UnityEPL.Random.Rnd.Next(limits[0], limits[1]);
        await Timing.Delay(interval);
    }

    protected async Task CuedRecall() {
        // Get cued recall state variables
        int[] isiLimits = Config.interStimulusDurationMs;
        int[] stimEarlyOnsetMsLimits = Config.stimEarlyOnsetMs;
        var recallStimWords = session.Trial.recall;

        for (int i = 0; i < recallStimWords.Count; ++i) {
            var wordStim = recallStimWords[i];

            // Determine the Inter Stimulus Interval (ISI), when stim should start, and adjust ISI
            int isiDuration = UnityEPL.Random.Rnd.Next(isiLimits[0], isiLimits[1]);
            int stimEarlyDuration = UnityEPL.Random.Rnd.Next(stimEarlyOnsetMsLimits[0], stimEarlyOnsetMsLimits[1]);
            isiDuration -= stimEarlyDuration;

            // Do the ISI
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(isiDuration));
            await Timing.Delay(isiDuration);

            // Do the stim and wait the rest of the ISI
            if (wordStim.stim) { manager.hostPC?.SendStimMsgTS(); }
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(stimEarlyDuration));
            await Timing.Delay(stimEarlyDuration);

            // Start recording for the cued recall
            string wavPath = Path.Combine(manager.fileManager.SessionPath(), 
                "cuedRecall_" + session.TrialNum + "_" + i +".wav");
            manager.recorder.StartRecording(wavPath);
            eventReporter.LogTS("start recall period");
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.RECALL(Config.stimulusDurationMs+Config.recallDurationMs));

            // Do the cued recall and log it
            Dictionary<string, object> data = new() {
                { "word", wordStim.word.word },
                { "serialpos", i },
                { "stimWord", wordStim.stim },
            };
            eventReporter.LogTS("word stimulus info", data);
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.WORD(), data);
            wordDisplayer.DisplayWord(wordStim.word.word);
            await Timing.Delay(Config.stimulusDurationMs);

            // Clear the word and wait for the rest of the recall duration
            wordDisplayer.ClearWords();
            await Timing.Delay(Config.recallDurationMs);
            // try { await inputManager.WaitForKey(skipKeys, false, Config.recallDurationMs); }
            // catch (TimeoutException) {}

            // Stop recording and recall period
            manager.recorder.StopRecording();
            eventReporter.LogTS("stop recall period");
            manager.lowBeep.Play();
        }
        wordDisplayer.TurnOff();
    }

    protected async Task Recognition() {
        // Get recognition state variables
        int[] isiLimits = Config.interStimulusDurationMs;
        int[] stimEarlyOnsetMsLimits = Config.stimEarlyOnsetMs;
        var recogStimWords = session.Trial.recognition;

        for (int i = 0; i < recogStimWords.Count; ++i) {
            var wordStim = recogStimWords[i];
            
            // Determine the Inter Stimulus Interval (ISI), when stim should start, and adjust ISI
            int isiDuration = UnityEPL.Random.Rnd.Next(isiLimits[0], isiLimits[1]);
            int stimEarlyDuration = UnityEPL.Random.Rnd.Next(stimEarlyOnsetMsLimits[0], stimEarlyOnsetMsLimits[1]);
            isiDuration -= stimEarlyDuration;

            // Do the ISI
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(isiDuration));
            await Timing.Delay(isiDuration);

            // Do the stim and wait the rest of the ISI
            if (wordStim.stim) { manager.hostPC?.SendStimMsgTS(); }
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(stimEarlyDuration));
            await Timing.Delay(stimEarlyDuration);

            // Start recording for the cued recall
            string wavPath = Path.Combine(manager.fileManager.SessionPath(), 
                "recognitionRecall_" + session.TrialNum + "_" + i +".wav");
            manager.recorder.StartRecording(wavPath);
            eventReporter.LogTS("start recall period");
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.RECALL(Config.stimulusDurationMs+Config.recogDurationMs));

            // Do the recognition and log it
            Dictionary<string, object> data = new() {
                { "word", wordStim.word.word },
                { "serialpos", i },
                { "stimWord", wordStim.stim },
            };
            eventReporter.LogTS("word stimulus info", data);
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.WORD(), data);
            oldNewKeys.TurnOn();
            wordDisplayer.DisplayWord(wordStim.word.word);
            await Timing.Delay(Config.stimulusDurationMs);

            // Clear the word and wait for the rest of the recognition duration
            wordDisplayer.ClearWords();
            await Timing.Delay(Config.recogDurationMs);
            // try { await inputManager.WaitForKey(skipKeys, false, Config.recogDurationMs); }
            // catch (TimeoutException) {}

            // Stop recording and recognition period
            manager.recorder.StopRecording();
            eventReporter.LogTS("stop recall period");
            oldNewKeys.TurnOff();
            manager.lowBeep.Play();
        }
        wordDisplayer.TurnOff();
    }

    protected async Task Questioneer() {
        var ynKeyCodes = new List<KeyCode> {KeyCode.Y, KeyCode.N};

        // Question 1
        textDisplayer.Display("Question 1", LangStrings.Blank(), LangStrings.QuestioneerQ1());
        KeyCode q1Resp = await inputManager.WaitForKey(ynKeyCodes);
        textDisplayer.Clear();

        // Question 1 sub questions
        if (q1Resp == KeyCode.Y) {
            await textDisplayer.PressAnyKey("Question 1a", LangStrings.Blank(), LangStrings.QuestioneerQ1a());

            string wavPath = Path.Combine(manager.fileManager.SessionPath(), "q1a.wav");
            manager.recorder.StartRecording(wavPath);
            textDisplayer.Display("Question 1a recording", LangStrings.Blank(), LangStrings.QuestioneerQ1b());
            await inputManager.WaitForKey();
            manager.recorder.StopRecording();
        }
    }

    // Word/Stim List Generation
    protected override async Task SetupWordList() {
        // Validate word repeats and counts
        var wordRepeats = Config.wordRepeats;
        var wordCounts = Config.wordCounts;
        if (wordRepeats.Count() != 1 && wordRepeats[0] != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's wordRepeats should only have one item with a value of 1"));
        } else if (wordCounts.Count() != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's wordCounts should only have one item in it"));
        }

        // Validate lure word repeats and counts
        var lureWordRepeats = Config.lureWordRepeats;
        var lureWordCounts = Config.lureWordCounts;
        if (lureWordRepeats.Count() != 1 && lureWordRepeats[0] != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's lureWordRepeats should only have one item with a value of 1"));
        } else if (lureWordCounts.Count() != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's lureWordCounts should only have one item in it"));
        }

        // Set member variables
        wordsPerList = wordCounts[0];
        lureWordsPerList = lureWordCounts[0];

        // Set the OldNewKeys sizes
        oldNewKeys.SetKeySize();
        oldNewKeys.SetupKeyPositions();

        // Read practice words and generate practice session
        var sourcePracticeWords = ReadWordpool<Word>(manager.fileManager.GetPracticeWordList(), "practice_wordpool");
        var practiceWords = new WordRandomSubset<Word>(sourcePracticeWords, true);
        practiceSession = GeneratePracticeSession(practiceWords);

        // Read words, save the original words, and set the WordDisplay sizes
        var sourceWords = ReadWordpool<Word>(manager.fileManager.GetWordList(), "wordpool");
        wordDisplayer.SetWordSize(sourceWords);

        // Allow for splitting over multiple sessions
        var usedWordsPath = Path.Combine(manager.fileManager.SessionPath(), "usedWords.tsv");
        var unusedWords = new List<Word>(sourceWords);
        var priorSessionPath = manager.fileManager.PriorSessionPath();
        if (priorSessionPath != null) {
            var priorUsedWordsPath = Path.Combine(priorSessionPath, "usedWords.tsv");
            File.Copy(priorUsedWordsPath, usedWordsPath, true);
            var usedWords = ReadWordpool<Word>(priorUsedWordsPath);
            unusedWords.RemoveAll(uw => usedWords.Any(w => w.word == uw.word));
        } else {
            var titleLine = File.ReadLines(manager.fileManager.GetWordList()).First();
            File.WriteAllLines(usedWordsPath, new string[1]{titleLine});
        }


        // Generate the random subset needed
        // If the number of words is less than the words needed, then ask the user if they want to reset word usage and resuse words, and do it again
        // TODO: JPB: (feature) Move the GenerateSession word reuse to WordRandomSubset (or something)
        // TODO: JPB: (feature) Load Session
        var words = new WordRandomSubset<Word>(unusedWords, usedWordsPath: usedWordsPath);
        try {
            normalSession = GenerateSession(words);
        } catch (Exception e) {
            textDisplayer.Display("ran out of words", LangStrings.Blank(), LangStrings.RanOutOfUnusedWords());
            KeyCode keyCode = await inputManager.WaitForKey(ynKeyCodes);
            textDisplayer.Clear();
            if (keyCode == KeyCode.Y) {
                // Reset the used words
                File.Delete(usedWordsPath);
                unusedWords = new List<Word>(sourceWords);
            } else {
                ErrorNotifier.ErrorTS(new Exception($"The number of unused words {unusedWords.Count}/{sourceWords.Count} is less than the words needed", e));
            }
            practiceWords = new WordRandomSubset<Word>(sourcePracticeWords, true);
            words = new WordRandomSubset<Word>(unusedWords, usedWordsPath: usedWordsPath);
            normalSession = GenerateSession(words);
        }
    }

    protected MemMapTrial<PairedWord> MakeTrial<U>(U randomSubset, bool encStim, bool recallStim, bool recogStim, List<bool> wordOrders, List<int> recallOrders, List<int> recogOrders) 
        where U : WordRandomSubset<Word>
    {
        // Make paired words
        var randomWords = randomSubset.Get(wordsPerList*2).ToList();
        var pairedWords = new List<PairedWord>();
        for (int i = 0; i < randomWords.Count - 1; i += 2) {
            pairedWords.Add(new(randomWords[i].word, randomWords[i+1].word));
        }

        // Make lure words
        randomWords = randomSubset.Get(lureWordsPerList).ToList();
        var lureWords = new List<PairedWord>();
        for (int i = 0; i < randomWords.Count; ++i) {
            lureWords.Add(new(randomWords[i].word, ""));
        }

        // Make encoding, recall, and recognition word lists
        var encWords = new List<PairedWord>(pairedWords);
        var recallWords = new List<PairedWord>(pairedWords);
        var recogWords = pairedWords.Concat(lureWords).ToList();

        // Swap the word order for recall and recog words as needed
        if (wordOrders.Count != wordsPerList) {
            ErrorNotifier.ErrorTS(new Exception($"The number of word orders {wordOrders.Count} does not equal the number of words per list {wordsPerList}"));
        }
        for (int i = 0; i < wordOrders.Count; ++i) {
            if (wordOrders[i]) {
                recallWords[i] = new PairedWord(recallWords[i].pairedWord, recallWords[i].word);
            } else {
                recogWords[i] = new PairedWord(recogWords[i].pairedWord, recogWords[i].word);
            }
        }

        // Reassign the order of the recall word list
        if (recallOrders.Count != recallWords.Count) {
            ErrorNotifier.ErrorTS(new Exception($"The number of recall orders {recallOrders.Count} does not equal the number of words per list {recallWords.Count}"));
        }
        var oldRecallWords = new List<PairedWord>(recallWords);
        for (int i = 0; i < recallOrders.Count; ++i) {
            recallWords[i] = oldRecallWords[recallOrders[i]];
        }

        // Reassign the order of the recognition word list
        if (recogOrders.Count != recogWords.Count) {
            ErrorNotifier.ErrorTS(new Exception($"The number of recog orders {recogOrders.Count} does not equal the number of words per list {recogWords.Count}"));
        }
        var oldRecogWords = new List<PairedWord>(recogWords);
        for (int i = 0; i < recogOrders.Count; ++i) {
            recogWords[i] = oldRecogWords[recogOrders[i]];
        }

        // Create the StimWord Lists
        var encStimList = GenStimList(encWords, encStim);
        var recallStimList = GenStimList(recallWords, recallStim);
        var recogStimList = GenStimList(recogWords, recogStim);

        return new MemMapTrial<PairedWord>(encStimList, recallStimList, recogStimList, encStim, recallStim, recogStim);
    }

    protected StimWordList<PairedWord> GenStimList(List<PairedWord> inputWords, bool stim) {
        var stimList = Enumerable.Range(1, inputWords.Count).Select(i => stim).ToList();
        return new StimWordList<PairedWord>(inputWords, stimList);
    }

    
    protected MemMapSession<PairedWord> GeneratePracticeSession<T>(T randomSubset) 
        where T : WordRandomSubset<Word>
    {
        var session = new MemMapSession<PairedWord> {
            isPractice = true
        };

        // Practice Lists
        bool noPractice = Config.onlyPracticeOnFirstSession && Config.sessionNum > 1;
        int numPracticeLists = noPractice ? 0 : Config.practiceLists;
        for (int i = 0; i < numPracticeLists; i++) {
            var wordOrders = Enumerable.Range(0, wordsPerList).Select(i => i % 2 == 0).ToList();
            var recallOrders = Enumerable.Range(0, wordsPerList).ToList().Shuffle(UnityEPL.Random.StableRnd);
            var recogOrders = GenZigZagList(wordsPerList, lureWordsPerList);
            session.AddTrial(MakeTrial(randomSubset, false, false, false, wordOrders, recallOrders, recogOrders));
        }

        // Write the lists to file
        for (int i = 0; i < session.trials.Count; i++) {
            WriteLstFile(session.trials[i].encoding, i, session.isPractice);
        }

        // Debugging
        session.DebugPrintAll();
        session.PrintAllWordsToDebugLog();

        return session;
    }

    protected new MemMapSession<PairedWord> GenerateSession<T>(T randomSubset) 
        where T : WordRandomSubset<Word>
    {
        var session = new MemMapSession<PairedWord>();

        // Pre No-Stim Lists
        for (int i = 0; i < Config.preNoStimLists; i++) {
            var wordOrders = Enumerable.Range(0, wordsPerList).Select(i => i % 2 == 0).ToList();
            var recallOrders = Enumerable.Range(0, wordsPerList).ToList().Shuffle();
            var recogOrders = Enumerable.Range(0, wordsPerList+lureWordsPerList).ToList().Shuffle();
            session.AddTrial(MakeTrial(randomSubset, false, false, false, wordOrders, recallOrders, recogOrders));
        }

        // Check for invalid list types
        if (Config.encodingAndRetrievalLists != 0) {
            ErrorNotifier.ErrorTS(new Exception("Config's encodingAndRetrievalLists should be 0 in Config"));
        }

        int numEncLists = Config.encodingOnlyLists;
        int numRetLists = Config.retrievalOnlyLists;
        int numEncAndRetLists = Config.encodingAndRetrievalLists;
        int numNoStimLists = Config.noStimLists;

        // Generate the unique bool lists for the paired words
        // 1000 is a random number to limit it if the word lists get too long
        List<List<bool>> uniqueBoolLists = GenerateUniqueBoolLists(wordsPerList, 100);

        // TODO: JPB: (noa) (feature) Make config value that limits the number of lists per block
        //            This could be important if patients don't finish the task consistently
        int maxNumListsPerStimType = Math.Max(numEncLists, Math.Max(numRetLists, Math.Max(numEncAndRetLists, numNoStimLists)));
        int numListsPerBlock = Math.Min(maxNumListsPerStimType, uniqueBoolLists.Count);
        int numBlocks = (int)Math.Ceiling((double)maxNumListsPerStimType / numListsPerBlock);

        // This example assumes wordsPerList = 3, numEncLists = 4, numRetLists = 4, numNoStimLists = 4, and numEncAndRetLists = 0

        // This part below shows the first iteration of the loop (which is for the first block)
        // There is only one iteration in this example
            
            // Make the word orders for each stim type
            // The word orders are which word is the shown word and which is the paired word for recall and recognition
            // Each word order has the same number of trues and falses (or 1 off if odd number of words)
            // Each stim type must have the same word orders in a unique ordering
            // [[[TTF], [TFT], [TFF], [FTF]],  -> Encoding
            //  [[TFF], [TTF], [FTF], [TFT]],  -> Retrieval
            //  [[TTF], [TFF], [TFT], [FTF]],  -> Encoding and Retrieval
            //  [[FTF], [TTF], [TFF], [TFT]]]  -> No stim

            // Make the recall orders for each stim type
            // The recall orders are how to shuffle the recall words
            // Each stim type must have the same word orders in a unique ordering
            // [[[2,1,3], [1,2,3], [3,2,1], [1,3,2]],  -> Encoding
            //  [[1,3,2], [1,2,3], [3,2,1], [2,1,3]],  -> Retrieval
            //  [[1,2,3], [2,1,3], [1,3,2], [3,2,1]],  -> Encoding and Retrieval
            //  [[3,2,1], [2,1,3], [1,3,2], [1,2,3]]]  -> No stim

            // Make the recognition orders for each stim type
            // The recognition orders are how to shuffle the recognition words (including the lures)
            // Each stim type must have the same word orders in a unique ordering
            // [[[1,5,6,2,4,3], [4,3,6,2,1,5], [5,2,1,4,6,3], [3,2,6,1,4,5]],  -> Encoding
            //  [[5,2,1,4,6,3], [1,5,6,2,4,3], [3,2,6,1,4,5], [4,3,6,2,1,5]],  -> Retrieval
            //  [[1,5,6,2,4,3], [4,3,6,2,1,5], [3,2,6,1,4,5], [5,2,1,4,6,3]],  -> Encoding and Retrieval
            //  [[3,2,6,1,4,5], [1,5,6,2,4,3], [4,3,6,2,1,5], [5,2,1,4,6,3]]]  -> No stim

            // Create groupings of each stim type as a Trial (for as many as there are), shuffle it, and add them all to the session
            // The shuffled order does not have to be unique for each iteration
            // You will notice that there are no "Encoding and Retrieval" lists added because numEncAndRetLists = 0
            // [MemMapTrial(E,[TTF], [2,1,3], [1,5,6,2,4,3]), MemMapTrial(R, [TFF], [1,3,2], [1,5,6,2,4,3]), MemMapTrial(N, [FTF], [3,2,1], [3,2,6,1,4,5])]
            // [MemMapTrial(R, [TFF], [1,3,2], [1,5,6,2,4,3]), MemMapTrial(E,[TTF], [2,1,3], [1,5,6,2,4,3]), MemMapTrial(N, [FTF], [3,2,1], [3,2,6,1,4,5])]
            // session = [MemMapTrial(R, [TFF], [1,3,2], [1,5,6,2,4,3]), MemMapTrial(E,[TTF], [2,1,3], [1,5,6,2,4,3]), MemMapTrial(N, [FTF], [3,2,1], [3,2,6,1,4,5])]
            // ... (skipping the next 2 lists)
            // [MemMapTrial(E, [FTF], [1,3,2], [3,2,6,1,4,5]), MemMapTrial(R, [TFT], [2,1,3], [4,3,6,2,1,5]), MemMapTrial(N, [TFT], [1,2,3], [5,2,1,4,6,3])]
            // [MemMapTrial(N, [TFT], [1,2,3], [5,2,1,4,6,3]), MemMapTrial(R, [TFT], [2,1,3], [4,3,6,2,1,5]), MemMapTrial(E, [FTF], [1,3,2], [3,2,6,1,4,5])]
            // session = [MemMapTrial(R, [TFF], [1,3,2], [1,5,6,2,4,3]), MemMapTrial(E,[TTF], [2,1,3], [1,5,6,2,4,3]), MemMapTrial(N, [FTF], [3,2,1], [3,2,6,1,4,5]),
            //            MemMapTrial(E, [TFT], [1,2,3], [4,3,6,2,1,5]), MemMapTrial(R, [TTF], [1,2,3], [1,5,6,2,4,3]), MemMapTrial(N, [TTF], [2,1,3], [1,5,6,2,4,3]),
            //            MemMapTrial(R, [FTF], [3,2,1], [3,2,6,1,4,5]), MemMapTrial(N, [TFF], [1,3,2], [4,3,6,2,1,5]), MemMapTrial(E, [TFF], [3,2,1], [5,2,1,4,6,3]),
            //            MemMapTrial(N, [TFT], [1,2,3], [5,2,1,4,6,3]), MemMapTrial(R, [TFT], [2,1,3], [4,3,6,2,1,5]), MemMapTrial(E, [FTF], [1,3,2], [3,2,6,1,4,5])]
            
        // If there were to be more iterations, you would do the same thing as the first iteration
        // The only difference would be that you add the new trials to the already populated session

        // For each block, do the following loop
        for (int blockNum = 0; blockNum < numBlocks; ++blockNum) {
            // Make the word orders for each list in a block
            // Each word order has the same number of trues and falses (or 1 off if odd number of words)
            var wordOrderOptions = uniqueBoolLists.Shuffle().Take(numListsPerBlock).ToList();
            // Make a unique random ordering for each stim type
            List<List<List<bool>>> wordOrders = GenUniqueRandomLists<List<List<bool>>, List<bool>>(4, wordOrderOptions);

            // Make the recall orders for each list in a block
            var wordsPerRecallList = wordsPerList;
            var recallOrderOptions = GenUniqueOrdersLists(wordsPerRecallList, numListsPerBlock);
            // Make a unique random ordering for each stim type
            List<List<List<int>>> recallOrders = GenUniqueRandomLists<List<List<int>>, List<int>>(4, recallOrderOptions);

            // Make the recog orders for each list in a block
            var wordsPerRecogList = wordsPerList + lureWordsPerList;
            var recogOrderOptions = GenUniqueOrdersLists(wordsPerRecogList, numListsPerBlock);
            // Make a unique random ordering for each stim type
            List<List<List<int>>> recogOrders = GenUniqueRandomLists<List<List<int>>, List<int>>(4, recogOrderOptions);

            // Create a list, for the 'encoding' stim type, of MemMapTrials with the wordOrders
            var encLists = new List<MemMapTrial<PairedWord>>();
            var numEncListsInBlock = Math.Min(numEncLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numEncListsInBlock; i++) {
                encLists.Add(MakeTrial(randomSubset, true, false, false, wordOrders[0][i], recallOrders[0][i], recogOrders[0][i]));
            }

            // Create a list, for the 'retrieval' stim type, of MemMapTrials with the wordOrders
            var retLists = new List<MemMapTrial<PairedWord>>();
            var numRetListsInBlock = Math.Min(numRetLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numRetListsInBlock; i++) {
                retLists.Add(MakeTrial(randomSubset, false, true, true, wordOrders[1][i], recallOrders[1][i], recogOrders[1][i]));
            }

            // Create a list, for the 'encoding and retrieval' stim type, of MemMapTrials with the wordOrders
            var encAndRetLists = new List<MemMapTrial<PairedWord>>();
            var numEncAndRetListsInBlock = Math.Min(numEncAndRetLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numEncAndRetListsInBlock; i++) {
                encAndRetLists.Add(MakeTrial(randomSubset, true, true, true, wordOrders[2][i], recallOrders[2][i], recogOrders[2][i]));
            }

            // Create a list, for the 'no stim' stim type, of MemMapTrials with the wordOrders
            var noStimLists = new List<MemMapTrial<PairedWord>>();
            var numNoStimListsInBlock = Math.Min(numNoStimLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numNoStimListsInBlock; i++) {
                noStimLists.Add(MakeTrial(randomSubset, false, false, false, wordOrders[3][i], recallOrders[3][i], recogOrders[3][i]));
            }

            // For each potential list index:
            //      Create a list of MemMapTrial's from that index of each stim type
            //      Randomize this new list
            //      Add the elements of that list to the session
            for (int i=0; i < numListsPerBlock; ++i) {
                var randomizedList = new MemMapSession<PairedWord>();
                if (i < numEncListsInBlock) {
                    randomizedList.AddTrial(encLists[i]);
                }
                if (i < numRetListsInBlock) {
                    randomizedList.AddTrial(retLists[i]);
                }
                if (i < numEncAndRetListsInBlock) {
                    randomizedList.AddTrial(encAndRetLists[i]);
                }
                if (i < numNoStimListsInBlock) {
                    randomizedList.AddTrial(noStimLists[i]);
                }
                session.AddTrials(randomizedList.trials.Shuffle());
            }
        }

        // Write the lists to file
        for (int i = 0; i < session.NumTrials; i++) {
            WriteLstFile(session.trials[i].encoding, i, session.isPractice);
        }

        // Debugging
        session.DebugPrintAll();
        session.PrintAllWordsToDebugLog();

        return session;
    }

    protected static List<List<bool>> GenerateUniqueBoolLists(int numElements, int? maxLists = null) {
        var allCombinations = new List<List<bool>>();
        GenerateUniqueBoolListsHelper(new List<bool>(), numElements, ref allCombinations, maxLists);
        return allCombinations;
    }
    protected static void GenerateUniqueBoolListsHelper(List<bool> current, int numElements, ref List<List<bool>> allCombinations, int? maxLists) {
        // This function generates every combination of an even number of trues and falses for numElements
        // If numElements is odd, then it's every combination that is up to 1 different
        // Ex: numElements = 3 : TTF, TFT, FTT, TFF, FTF, FFT
        // Ex: numElements = 4 : TTFF, TFTF, FTTF, FFTT, FTFT, TFFT 

        if (allCombinations.Count == maxLists) {
            return;
        } else if (current.Count == numElements) {
            allCombinations.Add(new List<bool>(current));
            return;
        }

        // Count trues and falses
        int countTrue = current.Count(x => x);
        int countFalse = current.Count(x => !x);

        // Add true if possible
        if (countTrue < numElements / 2 || (numElements % 2 != 0 && countTrue <= numElements / 2)) {
            GenerateUniqueBoolListsHelper(current.Append(true).ToList(), numElements, ref allCombinations, maxLists);
        }

        // Add false if possible
        if (countFalse < numElements / 2 || (numElements % 2 != 0 && countFalse <= numElements / 2)) {
            GenerateUniqueBoolListsHelper(current.Append(false).ToList(), numElements, ref allCombinations, maxLists);
        }
    }

    /// <summary>
    /// Generates a list of zigzagged indices by interleaving the indices of two already concatenated lists.
    /// If there are any leftover indices in the longer list, they are appended to the end of the new list.
    /// </summary>
    /// <param name="numList1">The number of elements in the first list (pre-concatenation).</param>
    /// <param name="numList2">The number of elements in the second list (pre-concatenation).</param>
    /// <returns>A list containing the indices of the original list interleaved in a zigzag pattern.</returns>
    /// ex: GenZigZagList(2, 2) -> [0, 2, 1, 3]
    /// ex: GenZigZagList(2, 4) -> [0, 2, 1, 3, 4, 5]
    /// ex: GenZigZagList(4, 2) -> [0, 4, 1, 5, 2, 3]
    protected List<int> GenZigZagList(int numList1, int numList2) {
        var list1 = Enumerable.Range(0, numList1).ToList();
        var list2 = Enumerable.Range(numList1, numList2).ToList();
        var retList = new List<int>();
        for (int i=0; i < Math.Max(numList1, numList2); ++i) {
            if (i < list1.Count) { retList.Add(list1[i]); }
            if (i < list2.Count) { retList.Add(list2[i]); }
        }
        return retList;
    }

    protected static List<List<int>> GenUniqueOrdersLists(int numElements, int numLists) {
        var inputList = Enumerable.Range(0, numElements).ToList();
        return GenUniqueRandomLists<List<int>, int>(numLists, inputList);
    }

    
    /// <summary>
    /// Generates a specified number of unique random lists from the input list.
    /// </summary>
    /// <typeparam name="T">The type of the input list.</typeparam>
    /// <typeparam name="U">The type of the elements in the input list.</typeparam>
    /// <param name="numCombos">The number of unique random lists to generate.</param>
    /// <param name="inputList">The input list from which to generate the unique random lists.</param>
    /// <returns>A list of unique random lists.</returns>
    /// <exception cref="System.Exception">Thrown when the number of requested combinations is greater than the number of possible permutations.</exception>
    protected static List<T> GenUniqueRandomLists<T, U>(int numCombos, T inputList) 
        where T : List<U>
    {
        var uniqueCombinations = new HashSet<int>();
        var result = new List<T>();

        int attempts = 0;
        // The math for figuring out how many permutations should exists (due to duplicates) is annoying
        // https://math.stackexchange.com/questions/4251947/permutations-of-elements-where-some-elements-are-of-the-same-kind
        // if (numCombos > Statistics.Permutation(inputList.Count, inputList.Count)) {
        //     throw new Exception("Tried to generate unique random lists, but there are less permutations possible than the number requested");
        // }

        while (result.Count < numCombos && attempts < numCombos * 10) {
            T combination = (T) inputList.Shuffle();
            int hash = combination.GetSequenceHashCode();

            if (!uniqueCombinations.Contains(hash)) {
                uniqueCombinations.Add(hash);
                result.Add(combination);
            }
            ++attempts;
        }

        if (result.Count < numCombos) {
            throw new Exception("Tried to generate unique random lists, but there are less permutations possible than the number requested (or you are REALLY unlucky).");
        }

        return result;
    }

}
