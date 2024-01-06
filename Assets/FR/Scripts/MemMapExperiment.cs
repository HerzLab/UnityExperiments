using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEPL;

public class MemMapExperiment : FRExperimentBase<PairedWord> {
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
        await NextListPrompt();
        await CountdownVideo();
        await Fixation();
        await Encoding();
        await MathDistractor();
        await PauseBeforeRecall();
        await RecallOrientation();
        await CuedRecall();
        await PauseBeforeRecall();
        await RecallOrientation();
        await Recognition();
        FinishTrial();
    }
    protected override async Task TrialStates() {
        StartTrial();
        await NextListPrompt();
        await CountdownVideo();
        await Fixation();
        await Encoding();
        await MathDistractor();
        await PauseBeforeRecall();
        await RecallOrientation();
        await CuedRecall();
        await PauseBeforeRecall();
        await RecallOrientation();
        await Recognition();
        FinishTrial();
    }


    protected new async Task Fixation() {
            SendRamulatorStateMsg(HostPC.StateMsg.ORIENT, true);
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.ORIENT);

            int[] limits = Config.fixationDuration;
            int duration = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
            textDisplayer.Display("orientation stimulus", "", "+");
            manager.hostPC?.SendStateMsgTS(HostPC.StateMsg.ORIENT);
            await InterfaceManager.Delay(duration);

            textDisplayer.Clear();
            limits = Config.postFixationDelay;
            duration = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
            await InterfaceManager.Delay(duration);

            SendRamulatorStateMsg(HostPC.StateMsg.ORIENT, false);
        }

    protected new async Task Encoding() {
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

    protected async Task PauseBeforeRecog() {
        int[] limits = Config.recallDelay;
        int interval = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
        await InterfaceManager.Delay(interval);
    }

    protected async Task CuedRecall() {
        // TODO: JPB: (Noa) (needed) CuedRecall is not implemented
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
    }

    protected async Task Recognition() {

    }

    protected override void SetupWordList() {
        var wordRepeats = Config.wordRepeats;
        if (wordRepeats.Count() != 1 && wordRepeats[0] != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's wordRepeats should only have one item with a value of 1"));
        }

        wordsPerList = Config.wordCounts[0];
        blankWords = new List<PairedWord>(Enumerable.Range(1, wordsPerList).Select(i => new PairedWord("", "")).ToList());

        var sourceWords = ReadWordpool();
        var words = new WordRandomSubset<PairedWord>(sourceWords);

        // TODO: (feature) Load Session
        currentSession = GenerateSession(words);
    }

    protected override FRRun<PairedWord> MakeRun<U>(U randomSubset, bool encStim, bool recStim) {
        var inputWords = randomSubset.Get(wordsPerList).ToList();
        var encList = GenStimList(inputWords, encStim);
        var recList = GenStimList(inputWords, recStim);
        return new FRRun<PairedWord>(encList, recList, encStim, recStim);
    }

    protected StimWordList<PairedWord> GenStimList(List<PairedWord> inputWords, bool stim) {
        var stimList = Enumerable.Range(1, wordsPerList).Select(i => stim).ToList();
        return new StimWordList<PairedWord>(inputWords, stimList);
    }

    protected override FRSession<PairedWord> GenerateSession<U>(U randomSubset) {
        var session = new FRSession<PairedWord>();

        for (int i = 0; i < Config.practiceLists; i++) {
            session.Add(MakeRun(randomSubset, false, false));
        }

        for (int i = 0; i < Config.preNoStimLists; i++) {
            session.Add(MakeRun(randomSubset, false, false));
        }

        if (Config.encodingAndRetrievalLists != 0) {
            ErrorNotifier.ErrorTS(new Exception("Config's encodingAndRetrievalLists should be 0 in Config (unsupported)"));
        }

        int numEncLists = Config.encodingOnlyLists;
        int numRetLists = Config.retrievalOnlyLists;
        int numEncAndRetLists = Config.encodingAndRetrievalLists;
        int numNoStimLists = Config.noStimLists;

        int maxNumListsPerStimType = Math.Max(numEncLists, Math.Max(numRetLists, Math.Max(numEncAndRetLists, numNoStimLists)));
        int numStimMethods = (new List<int> {numEncLists, numRetLists, numEncAndRetLists, numNoStimLists}).Count(x => x > 0);
        int numListsPerBlock = Math.Min(maxNumListsPerStimType, numStimMethods);
        int numBlocks = (int)Math.Ceiling((double)maxNumListsPerStimType / numListsPerBlock);

        // This example assumes numEncLists = 4, numRetLists = 4, numNoStimLists = 4, numEncAndRetLists = 0
        // For each block, do the following loop

        // This part below shows the first iteration of the loop

            // Make the word orders for each list in a block
            // Each word order has the same number of trues and falses (or 1 off if odd number of words)
            // wordOrders =  [[TTF], [TFT], [TFF]]

            // Create a list, for each stim type, of FRRuns with the wordOrders
            // This also adds the words to each FRRun
            // encLists =    [FRRun(TTF, E), FRRun(TFT, E), FRRun(TFF, E)]
            // retLists =    [FRRun(TTF, R), FRRun(TFT, R), FRRun(TFF, R)]
            // noStimLists = [FRRun(TTF, N), FRRun(TFT, N), FRRun(TFF, N)]

            // Randomize the order of each stim type's list (of wordOrders)
            // encLists =    [FRRun(TFT, E), FRRun(TFF, E), FRRun(TTF, E)]  // shuffled
            // retLists =    [FRRun(TFF, R), FRRun(TTF, R), FRRun(TFT, R)]  // shuffled
            // noStimLists = [FRRun(TTF, N), FRRun(TFT, N), FRRun(TFF, N)]  // shuffled

            // Only use the correct amount of lists for each stim type
            // This does nothing on the first iteration
            // encLists =    [FRRun(TFT, E), FRRun(TFF, E), FRRun(TTF, E)]
            // retLists =    [FRRun(TFF, R), FRRun(TTF, R), FRRun(TFT, R)]
            // noStimLists = [FRRun(TTF, N), FRRun(TFT, N), FRRun(TFF, N)]

                // Create a list using the first of each stim type, randomize it, and add the elements of that list to the session
                // randomizedList = [[FRRun(TFT, E), FRRun(TFF, R), FRRun(TTF, N)]]
                // randomizedList = [[FRRun(TFT, N), FRRun(TTF, R), FRRun(TFF, E)]] // shuffled
                // session =        [FRRun(TFT, N), FRRun(TTF, R), FRRun(TFF, E)]

                // Create a list using the second of each stim type, randomize it, and add the elements of that list to the session
                // randomizedList = [[FRRun(TFF, E), FRRun(TTF, R), FRRun(TFT, N)]]
                // randomizedList = [[FRRun(TFF, N), FRRun(TFT, E), FRRun(TTF, R)]] // shuffled
                // session =        [FRRun(TFT, N), FRRun(TTF, R), FRRun(TFF, E),
                //                   FRRun(TFF, N), FRRun(TFT, E), FRRun(TTF, R)]

                // Create a list using the third of each stim type, randomize it, and add the elements of that list to the session
                // randomizedList = [[FRRun(TTF, E), FRRun(TFT, R), FRRun(TFF, N)]]
                // randomizedList = [[FRRun(TFF, R), FRRun(TTF, N), FRRun(TFT, E)]] // shuffled
                // session =        [FRRun(TFT, N), FRRun(TTF, R), FRRun(TFF, E),
                //                   FRRun(TFF, N), FRRun(TFT, E), FRRun(TTF, R),
                //                   FRRun(TFF, R), FRRun(TTF, N), FRRun(TFT, E)]

        // This part shows the second iteration of the loop
            
            // wordOrders =  [[FFT], [TFT], [TTF]]

            // encLists =    [FRRun(FFT, E), FRRun(TFT, E), FRRun(TTF, E)]
            // retLists =    [FRRun(FFT, R), FRRun(TFT, R), FRRun(TTF, R)]
            // noStimLists = [FRRun(FFT, N), FRRun(TFT, N), FRRun(TTF, N)]

            // encLists =    [FRRun(TFT, E), FRRun(TTF, E), FRRun(FFT, E)]  // shuffled
            // retLists =    [FRRun(FFT, R), FRRun(TFT, R), FRRun(TTF, R)]  // shuffled
            // noStimLists = [FRRun(TFT, N), FRRun(FFT, N), FRRun(TTF, N)]  // shuffled

            // encLists =    [FRRun(TFT, E)]
            // retLists =    [FRRun(FFT, R)]
            // noStimLists = [FRRun(TFT, N)]

                // randomizedList = [[FRRun(TFT, E), FRRun(FFT, R), FRRun(TFT, N)]]
                // randomizedList = [[FRRun(TFT, N), FRRun(TFT, E), FRRun(FFT, R)]] // shuffled
                // session =        [FRRun(TFT, N), FRRun(TTF, R), FRRun(TFF, E),
                //                   FRRun(TFF, N), FRRun(TFT, E), FRRun(TTF, R),
                //                   FRRun(TFF, R), FRRun(TTF, N), FRRun(TFT, E),
                //                   FRRun(TFT, N), FRRun(TFT, E), FRRun(FFT, R)]

        // This is the final result
        // session = [FRRun(TFT, N), FRRun(TTF, R), FRRun(TFF, E),
        //            FRRun(TFF, N), FRRun(TFT, E), FRRun(TTF, R),
        //            FRRun(TFF, R), FRRun(TTF, N), FRRun(TFT, E),
        //            FRRun(TFT, N), FRRun(TFT, E), FRRun(FFT, R)]

        // For each block, do the following loop
        for (int blockNum = 0; blockNum < numBlocks; ++blockNum) {
            // Make the word orders for each list in a block
            // Each word order has the same number of trues and falses (or 1 off if odd number of words)
            var wordOrders = GenerateUniqueBoolLists(numListsPerBlock, numStimMethods);

            // Create a list, for the 'encoding' stim type, of FRRuns with the wordOrders
            var encLists = new List<FRRun<PairedWord>>();
            var numEncListsInBlock = Math.Min(numEncLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numListsPerBlock; i++) {
                encLists.Add(MakeRun(randomSubset, true, false));
                for (int j=0; j < encLists[i].encoding.Count; ++j) {
                    encLists[i].encoding[j].word.setCuedWord(wordOrders[i][j]);
                }
                for (int j=0; j < encLists[i].recall.Count; ++j) {
                    encLists[i].recall[j].word.setCuedWord(wordOrders[i][j]);
                }
            }

            // Create a list, for the 'retrieval' stim type, of FRRuns with the wordOrders
            var retLists = new List<FRRun<PairedWord>>();
            var numRetListsInBlock = Math.Min(numRetLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numListsPerBlock; i++) {
                retLists.Add(MakeRun(randomSubset, false, true));
                for (int j=0; j < retLists[i].encoding.Count; ++j) {
                    retLists[i].encoding[j].word.setCuedWord(wordOrders[i][j]);
                }
                for (int j=0; j < retLists[i].recall.Count; ++j) {
                    retLists[i].recall[j].word.setCuedWord(wordOrders[i][j]);
                }
            }

            // Create a list, for the 'encoding and retrieval' stim type, of FRRuns with the wordOrders
            var encAndRetLists = new List<FRRun<PairedWord>>();
            var numEncAndRetListsInBlock = Math.Min(numEncAndRetLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numListsPerBlock; i++) {
                encAndRetLists.Add(MakeRun(randomSubset, true, true));
                for (int j=0; j < encAndRetLists[i].encoding.Count; ++j) {
                    encAndRetLists[i].encoding[j].word.setCuedWord(wordOrders[i][j]);
                }
                for (int j=0; j < encAndRetLists[i].recall.Count; ++j) {
                    encAndRetLists[i].recall[j].word.setCuedWord(wordOrders[i][j]);
                }
            }

            // Create a list, for the 'no stim' stim type, of FRRuns with the wordOrders
            var noStimLists = new List<FRRun<PairedWord>>();
            var numNoStimListsInBlock = Math.Min(numNoStimLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numListsPerBlock; i++) {
                noStimLists.Add(MakeRun(randomSubset, false, false));
                for (int j=0; j < noStimLists[i].encoding.Count; ++j) {
                    noStimLists[i].encoding[j].word.setCuedWord(wordOrders[i][j]);
                }
                for (int j=0; j < noStimLists[i].recall.Count; ++j) {
                    noStimLists[i].recall[j].word.setCuedWord(wordOrders[i][j]);
                }
            }
            
            // Randomize the order of each stim type's list (of wordOrders)
            encLists.ShuffleInPlace();
            retLists.ShuffleInPlace();
            encAndRetLists.ShuffleInPlace();
            noStimLists.ShuffleInPlace();

            // Only use the correct amount of lists for each stim type
            encLists = encLists.Take(numEncListsInBlock).ToList();
            retLists = retLists.Take(numRetListsInBlock).ToList();
            encAndRetLists = encAndRetLists.Take(numEncAndRetListsInBlock).ToList();
            noStimLists = noStimLists.Take(numNoStimListsInBlock).ToList();

            // For each potential list index:
            //      Create a list of FRRun's from that index of each stim type
            //      Randomize this new list
            //      Add the elements of that list to the session
            for (int i=0; i < numListsPerBlock; ++i) {
                var randomizedList = new FRSession<PairedWord>();
                if (i < numEncListsInBlock) {
                    randomizedList.Add(encLists[i]);
                }
                if (i < numRetListsInBlock) {
                    randomizedList.Add(retLists[i]);
                }
                if (i < numEncAndRetListsInBlock) {
                    randomizedList.Add(encAndRetLists[i]);
                }
                if (i < numNoStimListsInBlock) {
                    randomizedList.Add(noStimLists[i]);
                }
                session.AddRange(randomizedList.Shuffle());
            }
        }

        for (int i = 0; i < session.Count; i++) {
            WriteLstFile(session[i].encoding, i);
        }

        session.PrintAllWordsToDebugLog();

        return session;
    }

    protected static List<List<bool>> GenerateUniqueBoolLists(int numLists, int numElements) {
        var allCombinations = new List<List<bool>>();
        GenerateCombinations(new List<bool>(), numElements, ref allCombinations);
        return allCombinations.ShuffleInPlace().Take(numLists).ToList();
    }
    protected static void GenerateCombinations(List<bool> current, int numElements, ref List<List<bool>> allCombinations) {
        if (current.Count == numElements)
        {
            allCombinations.Add(new List<bool>(current));
            return;
        }

        // Count trues and falses
        int countTrue = current.Count(x => x);
        int countFalse = current.Count(x => !x);

        // Add true if possible
        if (countTrue < numElements / 2 || (numElements % 2 != 0 && countTrue <= numElements / 2))
        {
            current.Add(true);
            GenerateCombinations(current, numElements, ref allCombinations);
            current.RemoveAt(current.Count - 1);
        }

        // Add false if possible
        if (countFalse < numElements / 2 || (numElements % 2 != 0 && countFalse <= numElements / 2))
        {
            current.Add(false);
            GenerateCombinations(current, numElements, ref allCombinations);
            current.RemoveAt(current.Count - 1);
        }
    }

}
