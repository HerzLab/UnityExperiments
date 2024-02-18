using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEPL;
using System.Collections.Immutable;

public class MemMapExperiment : FRExperimentBase<PairedWord, MemMapTrial<PairedWord>, MemMapSession<PairedWord>> {
    protected readonly List<KeyCode> skipKeys = new List<KeyCode> {KeyCode.Space};
    protected readonly List<KeyCode> ynKeyCodes = new List<KeyCode> {KeyCode.Y, KeyCode.N};
    //protected const ImmutableList<KeyCode> ynKeys = ImmutableList<KeyCode>.Create(KeyCode.Y, KeyCode.N);

    protected int lureWordsPerList;

    protected override async Task PreTrialStates() {
        SetupWordList();

        // await QuitPrompt();
        // await Introduction();
        // await MicrophoneTest();
        await ConfirmStart();
    }
    protected override async Task PostTrialStates() {
        await Questioneer();
        await FinishExperiment();
    }
    protected override async Task PracticeTrialStates() {
        StartTrial();
        await NextPracticeListPrompt();
        await Encoding();
        await CuedRecall();
        await Recognition();

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
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ORIENT());

            int[] limits = Config.fixationDuration;
            int duration = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
            textDisplayer.Display("orientation stimulus", "", "+");
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ORIENT());
            await InterfaceManager.Delay(duration);

            textDisplayer.Clear();
            limits = Config.postFixationDelay;
            duration = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
            await InterfaceManager.Delay(duration);
        }

    protected new async Task Encoding() {
        manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ENCODING(), new() { { "current_trial", trialNum } });

        int[] isiLimits = Config.interStimulusDuration;
        int[] stimEarlyOnsetMsLimits = Config.stimEarlyOnsetMs;
        var encStimWords = currentSession.GetState().encoding;

        for (int i = 0; i < encStimWords.Count; ++i) {
            int isiDuration = InterfaceManager.rnd.Value.Next(isiLimits[0], isiLimits[1]);
            int stimEarlyDuration = InterfaceManager.rnd.Value.Next(stimEarlyOnsetMsLimits[0], stimEarlyOnsetMsLimits[1]);
            isiDuration -= stimEarlyDuration;

            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(isiDuration));
            await InterfaceManager.Delay(isiDuration);

            manager.hostPC?.SendStimMsgTS();
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(stimEarlyDuration));
            await InterfaceManager.Delay(stimEarlyDuration);

            var wordStim = currentSession.GetEncWord();
            currentSession.NextWord();
            Dictionary<string, object> data = new() {
                { "word", wordStim.word },
                { "serialpos", i },
                { "stim", wordStim.stim },
            };

            eventReporter.LogTS("word stimulus info", data);
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.WORD(), data);
            textDisplayer.Display("word stimulus", "", wordStim.word.ToDisplayString());
            await InterfaceManager.Delay(Config.stimulusDuration);
            eventReporter.LogTS("clear word stimulus", data);
            textDisplayer.Clear();
        }
    }

    protected async Task PauseBeforeRecog() {
        int[] limits = Config.recallDelay;
        int interval = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
        await InterfaceManager.Delay(interval);
    }

    protected async Task CuedRecall() {
        int[] isiLimits = Config.interStimulusDuration;
        int[] stimEarlyOnsetMsLimits = Config.stimEarlyOnsetMs;
        var recallStimWords = currentSession.GetState().recall;

        for (int i = 0; i < recallStimWords.Count; ++i) {
            var wordStim = recallStimWords[i];
            int isiDuration = InterfaceManager.rnd.Value.Next(isiLimits[0], isiLimits[1]);
            int stimEarlyDuration = InterfaceManager.rnd.Value.Next(stimEarlyOnsetMsLimits[0], stimEarlyOnsetMsLimits[1]);
            isiDuration -= stimEarlyDuration;

            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(isiDuration));
            await InterfaceManager.Delay(isiDuration);

            manager.hostPC?.SendStimMsgTS();
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(stimEarlyDuration));
            await InterfaceManager.Delay(stimEarlyDuration);

            string wavPath = Path.Combine(manager.fileManager.SessionPath(), 
                "cuedRecall_" + currentSession.GetListIndex() + "_" + i +".wav");
            manager.recorder.StartRecording(wavPath);
            eventReporter.LogTS("start recall period");
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.RECALL(Config.stimulusDuration+Config.recallDuration));

            Dictionary<string, object> data = new() {
                { "word", wordStim.word.word },
                { "serialpos", i },
                { "stim", wordStim.stim },
            };
            eventReporter.LogTS("word stimulus info", data);
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.WORD(), data);

            textDisplayer.Display("word stimulus", "", "\n"+wordStim.word.word+"\n");
            await InterfaceManager.Delay(Config.stimulusDuration);
            eventReporter.LogTS("clear word stimulus", data);
            textDisplayer.Clear();

            await inputManager.WaitForKeyTS(skipKeys, TimeSpan.FromMilliseconds(Config.recallDuration));
            var clip = manager.recorder.StopRecording();
        }
    }

    protected async Task Recognition() {
        int[] isiLimits = Config.interStimulusDuration;
        int[] stimEarlyOnsetMsLimits = Config.stimEarlyOnsetMs;
        var recogStimWords = currentSession.GetState().recog;

        for (int i = 0; i < recogStimWords.Count; ++i) {
            var wordStim = recogStimWords[i];
            int isiDuration = InterfaceManager.rnd.Value.Next(isiLimits[0], isiLimits[1]);
            int stimEarlyDuration = InterfaceManager.rnd.Value.Next(stimEarlyOnsetMsLimits[0], stimEarlyOnsetMsLimits[1]);
            isiDuration -= stimEarlyDuration;

            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(isiDuration));
            await InterfaceManager.Delay(isiDuration);

            manager.hostPC?.SendStimMsgTS();
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.ISI(stimEarlyDuration));
            await InterfaceManager.Delay(stimEarlyDuration);

            string wavPath = Path.Combine(manager.fileManager.SessionPath(), 
                "cuedRecall_" + currentSession.GetListIndex() + "_" + i +".wav");
            manager.recorder.StartRecording(wavPath);
            eventReporter.LogTS("start recall period");
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.RECALL(Config.stimulusDuration+Config.recogDuration));

            Dictionary<string, object> data = new() {
                { "word", wordStim.word.word },
                { "serialpos", i },
                { "stim", wordStim.stim },
            };
            eventReporter.LogTS("word stimulus info", data);
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.WORD(), data);

            textDisplayer.Display("word stimulus", "", "\n"+wordStim.word.word+"\n");
            await InterfaceManager.Delay(Config.stimulusDuration);
            eventReporter.LogTS("clear word stimulus", data);
            textDisplayer.Clear();

            await inputManager.WaitForKeyTS(skipKeys, TimeSpan.FromMilliseconds(Config.recogDuration));
            var clip = manager.recorder.StopRecording();
        }
    }

    protected async Task Questioneer() {
        var ynKeyCodes = new List<KeyCode> {KeyCode.Y, KeyCode.N};

        textDisplayer.Display("Question 1", "", 
            "Can you recall any specific moments during the experiment when you knew or felt stimulation was being delivered?\n\nYes (y) or No (n)");
        KeyCode q1Resp = await inputManager.WaitForKeyTS(ynKeyCodes);
        textDisplayer.Clear();

        if (q1Resp == KeyCode.Y) {
            await textDisplayer.PressAnyKey("Question 1a", "", 
                "Please describe when and why you think stimulation was delivered.\n\nPress any key to continue to the recording.");

            string wavPath = System.IO.Path.Combine(manager.fileManager.SessionPath(), "q1a.wav");
            manager.recorder.StartRecording(wavPath);
            textDisplayer.Display("Question 1a recording", "", 
                "Please describe when and why you think stimulation was delivered.\n\n<color=red>Recording...</color>\n\nPress any key when finished");
            await inputManager.WaitForKeyTS();
            var clip = manager.recorder.StopRecording();
        }
    }

    // Word/Stim List Generation
    protected override void SetupWordList() {
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

        // Read words and generate the random subset needed
        var sourceWords = ReadWordpool<Word>();
        var words = new WordRandomSubset<Word>(sourceWords);

        // TODO: (feature) Load Session
        currentSession = GenerateSession<WordRandomSubset<Word>>(words);
    }

    protected MemMapTrial<PairedWord> MakeTrial<U>(U randomSubset, bool encStim, bool recallStim, bool recogStim, List<bool> wordOrders, List<int> recogOrders) 
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

    protected List<int> GenZigZagList(int numList1, int numList2) {
        var list1 = Enumerable.Range(0, wordsPerList).ToList();
        var list2 = Enumerable.Range(wordsPerList, lureWordsPerList).ToList();
        var retList = new List<int>();
        for (int i=0; i < Math.Max(wordsPerList, lureWordsPerList); ++i) {
            if (i < list1.Count) { retList.Add(list1[i]); }
            if (i < list2.Count) { retList.Add(list2[i]); }
        }
        return retList;
    }

    protected new MemMapSession<PairedWord> GenerateSession<T>(T randomSubset) 
        where T : WordRandomSubset<Word>
    {
        var session = new MemMapSession<PairedWord>();

        var tempRecogOrders = new List<int>() {0,1,5,3,4,2};

        // Practice Lists
        for (int i = 0; i < Config.practiceLists; i++) {
            var wordOrders = Enumerable.Range(0, wordsPerList).Select(i => i % 2 == 0).ToList();
            var recogOrders = GenZigZagList(wordsPerList, lureWordsPerList);
            session.Add(MakeTrial(randomSubset, false, false, false, wordOrders, recogOrders));
        }

        // Pre No-Stim Lists
        for (int i = 0; i < Config.preNoStimLists; i++) {
            var wordOrders = Enumerable.Range(0, wordsPerList).Select(i => i % 2 == 0).ToList();
            var recogOrders = Enumerable.Range(0, wordsPerList+lureWordsPerList).ToList().Shuffle();
            session.Add(MakeTrial(randomSubset, false, false, false, wordOrders, recogOrders));
        }

        // Check for invalid list types
        if (Config.encodingAndRetrievalLists != 0) {
            ErrorNotifier.ErrorTS(new Exception("Config's encodingAndRetrievalLists should be 0 in Config"));
        }

        int numEncLists = Config.encodingOnlyLists;
        int numRetLists = Config.retrievalOnlyLists;
        int numEncAndRetLists = Config.encodingAndRetrievalLists;
        int numNoStimLists = Config.noStimLists;

        List<List<bool>> uniqueBoolLists = GenerateUniqueBoolLists(wordsPerList);


        int maxNumListsPerStimType = Math.Max(numEncLists, Math.Max(numRetLists, Math.Max(numEncAndRetLists, numNoStimLists)));
        int numListsPerBlock = Math.Min(maxNumListsPerStimType, uniqueBoolLists.Count);
        int numBlocks = (int)Math.Ceiling((double)maxNumListsPerStimType / numListsPerBlock);

        // This example assumes numEncLists = 4, numRetLists = 4, numNoStimLists = 4, numEncAndRetLists = 0
        // For each block, do the following loop

        // This part below shows the first iteration of the loop

            // Make the word orders for each list in a block
            // Each word order has the same number of trues and falses (or 1 off if odd number of words)
            // wordOrders =  [[TTF], [TFT], [TFF]]

            // Create a list, for each stim type, of MemMapTrials with the wordOrders
            // This also adds the words to each MemMapTrial
            // encLists =    [MemMapTrial(TTF, E), MemMapTrial(TFT, E), MemMapTrial(TFF, E)]
            // retLists =    [MemMapTrial(TTF, R), MemMapTrial(TFT, R), MemMapTrial(TFF, R)]
            // noStimLists = [MemMapTrial(TTF, N), MemMapTrial(TFT, N), MemMapTrial(TFF, N)]

            // Randomize the order of each stim type's list (of wordOrders)
            // encLists =    [MemMapTrial(TFT, E), MemMapTrial(TFF, E), MemMapTrial(TTF, E)]  // shuffled
            // retLists =    [MemMapTrial(TFF, R), MemMapTrial(TTF, R), MemMapTrial(TFT, R)]  // shuffled
            // noStimLists = [MemMapTrial(TTF, N), MemMapTrial(TFT, N), MemMapTrial(TFF, N)]  // shuffled

            // Only use the correct amount of lists for each stim type
            // This does nothing on the first iteration
            // encLists =    [MemMapTrial(TFT, E), MemMapTrial(TFF, E), MemMapTrial(TTF, E)]
            // retLists =    [MemMapTrial(TFF, R), MemMapTrial(TTF, R), MemMapTrial(TFT, R)]
            // noStimLists = [MemMapTrial(TTF, N), MemMapTrial(TFT, N), MemMapTrial(TFF, N)]

                // Create a list using the first of each stim type, randomize it, and add the elements of that list to the session
                // randomizedList = [[MemMapTrial(TFT, E), MemMapTrial(TFF, R), MemMapTrial(TTF, N)]]
                // randomizedList = [[MemMapTrial(TFT, N), MemMapTrial(TTF, R), MemMapTrial(TFF, E)]] // shuffled
                // session =        [MemMapTrial(TFT, N), MemMapTrial(TTF, R), MemMapTrial(TFF, E)]

                // Create a list using the second of each stim type, randomize it, and add the elements of that list to the session
                // randomizedList = [[MemMapTrial(TFF, E), MemMapTrial(TTF, R), MemMapTrial(TFT, N)]]
                // randomizedList = [[MemMapTrial(TFF, N), MemMapTrial(TFT, E), MemMapTrial(TTF, R)]] // shuffled
                // session =        [MemMapTrial(TFT, N), MemMapTrial(TTF, R), MemMapTrial(TFF, E),
                //                   MemMapTrial(TFF, N), MemMapTrial(TFT, E), MemMapTrial(TTF, R)]

                // Create a list using the third of each stim type, randomize it, and add the elements of that list to the session
                // randomizedList = [[MemMapTrial(TTF, E), MemMapTrial(TFT, R), MemMapTrial(TFF, N)]]
                // randomizedList = [[MemMapTrial(TFF, R), MemMapTrial(TTF, N), MemMapTrial(TFT, E)]] // shuffled
                // session =        [MemMapTrial(TFT, N), MemMapTrial(TTF, R), MemMapTrial(TFF, E),
                //                   MemMapTrial(TFF, N), MemMapTrial(TFT, E), MemMapTrial(TTF, R),
                //                   MemMapTrial(TFF, R), MemMapTrial(TTF, N), MemMapTrial(TFT, E)]

        // This part shows the second iteration of the loop
            
            // wordOrders =  [[FFT], [TFT], [TTF]]

            // encLists =    [MemMapTrial(FFT, E), MemMapTrial(TFT, E), MemMapTrial(TTF, E)]
            // retLists =    [MemMapTrial(FFT, R), MemMapTrial(TFT, R), MemMapTrial(TTF, R)]
            // noStimLists = [MemMapTrial(FFT, N), MemMapTrial(TFT, N), MemMapTrial(TTF, N)]

            // encLists =    [MemMapTrial(TFT, E), MemMapTrial(TTF, E), MemMapTrial(FFT, E)]  // shuffled
            // retLists =    [MemMapTrial(FFT, R), MemMapTrial(TFT, R), MemMapTrial(TTF, R)]  // shuffled
            // noStimLists = [MemMapTrial(TFT, N), MemMapTrial(FFT, N), MemMapTrial(TTF, N)]  // shuffled

            // encLists =    [MemMapTrial(TFT, E)]
            // retLists =    [MemMapTrial(FFT, R)]
            // noStimLists = [MemMapTrial(TFT, N)]

                // randomizedList = [[MemMapTrial(TFT, E), MemMapTrial(FFT, R), MemMapTrial(TFT, N)]]
                // randomizedList = [[MemMapTrial(TFT, N), MemMapTrial(TFT, E), MemMapTrial(FFT, R)]] // shuffled
                // session =        [MemMapTrial(TFT, N), MemMapTrial(TTF, R), MemMapTrial(TFF, E),
                //                   MemMapTrial(TFF, N), MemMapTrial(TFT, E), MemMapTrial(TTF, R),
                //                   MemMapTrial(TFF, R), MemMapTrial(TTF, N), MemMapTrial(TFT, E),
                //                   MemMapTrial(TFT, N), MemMapTrial(TFT, E), MemMapTrial(FFT, R)]

        // This is the final result
        // session = [MemMapTrial(TFT, N), MemMapTrial(TTF, R), MemMapTrial(TFF, E),
        //            MemMapTrial(TFF, N), MemMapTrial(TFT, E), MemMapTrial(TTF, R),
        //            MemMapTrial(TFF, R), MemMapTrial(TTF, N), MemMapTrial(TFT, E),
        //            MemMapTrial(TFT, N), MemMapTrial(TFT, E), MemMapTrial(FFT, R)]

        // For each block, do the following loop
        for (int blockNum = 0; blockNum < numBlocks; ++blockNum) {
            // Make the word orders for each list in a block
            // Each word order has the same number of trues and falses (or 1 off if odd number of words)
            var wordOrders = uniqueBoolLists.Shuffle().Take(numListsPerBlock).ToList();

            // Create a list, for the 'encoding' stim type, of MemMapTrials with the wordOrders
            var encLists = new List<MemMapTrial<PairedWord>>();
            var numEncListsInBlock = Math.Min(numEncLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numEncListsInBlock; i++) {
                encLists.Add(MakeTrial(randomSubset, true, false, false, wordOrders[i], tempRecogOrders));
            }

            // Create a list, for the 'retrieval' stim type, of MemMapTrials with the wordOrders
            var retLists = new List<MemMapTrial<PairedWord>>();
            var numRetListsInBlock = Math.Min(numRetLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numRetListsInBlock; i++) {
                retLists.Add(MakeTrial(randomSubset, false, true, true, wordOrders[i], tempRecogOrders));
            }

            // Create a list, for the 'encoding and retrieval' stim type, of MemMapTrials with the wordOrders
            var encAndRetLists = new List<MemMapTrial<PairedWord>>();
            var numEncAndRetListsInBlock = Math.Min(numEncAndRetLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numEncAndRetListsInBlock; i++) {
                encAndRetLists.Add(MakeTrial(randomSubset, true, true, true, wordOrders[i], tempRecogOrders));
            }

            // Create a list, for the 'no stim' stim type, of MemMapTrials with the wordOrders
            var noStimLists = new List<MemMapTrial<PairedWord>>();
            var numNoStimListsInBlock = Math.Min(numNoStimLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numNoStimListsInBlock; i++) {
                noStimLists.Add(MakeTrial(randomSubset, false, false, false, wordOrders[i], tempRecogOrders));
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
            //      Create a list of MemMapTrial's from that index of each stim type
            //      Randomize this new list
            //      Add the elements of that list to the session
            for (int i=0; i < numListsPerBlock; ++i) {
                var randomizedList = new MemMapSession<PairedWord>();
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

    protected static List<List<bool>> GenerateUniqueBoolLists(int numElements) {
        var allCombinations = new List<List<bool>>();
        GenerateUniqueBoolListsHelper(new List<bool>(), numElements, ref allCombinations);
        return allCombinations;
    }
    protected static void GenerateUniqueBoolListsHelper(List<bool> current, int numElements, ref List<List<bool>> allCombinations) {
        // This function generates every combination of an even number of trues and falses for numElements
        // If numElements is odd, then it's every combination that is up to 1 different
        // Ex: numElements = 3 : TTF, TFT, FTT, TFF, FTF, FFT
        // Ex: numElements = 4 : TTFF, TFTF, FTTF, FFTT, FTFT, TFFT 

        if (current.Count == numElements) {
            allCombinations.Add(new List<bool>(current));
            return;
        }

        // Count trues and falses
        int countTrue = current.Count(x => x);
        int countFalse = current.Count(x => !x);

        // Add true if possible
        if (countTrue < numElements / 2 || (numElements % 2 != 0 && countTrue <= numElements / 2)) {
            GenerateUniqueBoolListsHelper(current.Append(true).ToList(), numElements, ref allCombinations);
        }

        // Add false if possible
        if (countFalse < numElements / 2 || (numElements % 2 != 0 && countFalse <= numElements / 2)) {
            GenerateUniqueBoolListsHelper(current.Append(false).ToList(), numElements, ref allCombinations);
        }
    }
}
