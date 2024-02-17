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
        var recStimWords = currentSession.GetState().recall;

        for (int i = 0; i < recStimWords.Count; ++i) {
            var wordStim = recStimWords[i];
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
                { "word", wordStim.word.cuedWord },
                { "serialpos", i },
                { "stim", wordStim.stim },
            };
            eventReporter.LogTS("word stimulus info", data);
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.WORD(), data);

            textDisplayer.Display("word stimulus", "", "\n"+wordStim.word.cuedWord+"\n");
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
        var recStimWords = currentSession.GetState().recall;

        for (int i = 0; i < recStimWords.Count; ++i) {
            var wordStim = recStimWords[i];
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
                { "word", wordStim.word.recogWord },
                { "serialpos", i },
                { "stim", wordStim.stim },
            };
            eventReporter.LogTS("word stimulus info", data);
            manager.hostPC?.SendStateMsgTS(HostPcStateMsg.WORD(), data);

            textDisplayer.Display("word stimulus", "", "\n"+wordStim.word.recogWord+"\n");
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
    // protected List<PairedWord> ReadPairedWordpool() {
    //     // wordpool is a file with 'word' as a header and one word per line.
    //     // repeats are described in the config file with two matched arrays,
    //     // repeats and counts, which describe the number of presentations
    //     // words can have and the number of words that should be assigned to
    //     // each of those presentation categories.
    //     string sourceList = manager.fileManager.GetWordList();
    //     var sourceWords = new List<PairedWord>();

    //     // skip line for tsv header
    //     var fileLines = File.ReadLines(sourceList).Skip(1).ToList().ShuffleInPlace(InterfaceManager.stableRnd.Value); 
    //     for (int i = 0; i < fileLines.Count - 1; i += 3) {
    //         var word = fileLines[i].Trim().Split('\t')[0];
    //         var pairedWord = fileLines[i+1].Trim().Split('\t')[0];
    //         sourceWords.Add(new PairedWord(word, pairedWord));
    //     }

    //     // copy full wordpool to session directory
    //     string path = System.IO.Path.Combine(manager.fileManager.SessionPath(), "wordpool.txt");
    //     File.WriteAllText(path, String.Join("\n", sourceWords));

    //     // copy original wordpool to session directory
    //     string origPath = System.IO.Path.Combine(manager.fileManager.SessionPath(), "original_wordpool.txt");
    //     File.Copy(sourceList, origPath, true);

    //     return sourceWords;
    // }

    // protected override void SetupWordList() {
    //     var wordRepeats = Config.wordRepeats;
    //     if (wordRepeats.Count() != 1 && wordRepeats[0] != 1) {
    //         ErrorNotifier.ErrorTS(new Exception("Config's wordRepeats should only have one item with a value of 1"));
    //     }

    //     wordsPerList = Config.wordCounts[0];

    //     var sourceWords = ReadWordpool<Word>();
    //     var words = new WordRandomSubset<Word>(sourceWords);
    //     //var sourceWords = ReadPairedWordpool();

    //     // TODO: (feature) Load Session
    //     currentSession = GenerateSession(words);

    // }

    protected override void SetupWordList() {
        var wordRepeats = Config.wordRepeats;
        var wordCounts = Config.wordCounts;
        if (wordRepeats.Count() != 1 && wordRepeats[0] != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's wordRepeats should only have one item with a value of 1"));
        } else if (wordCounts.Count() != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's wordCounts should only have one item in it"));
        }

        var lureWordRepeats = Config.lureWordRepeats;
        var lureWordCounts = Config.lureWordCounts;
        if (lureWordRepeats.Count() != 1 && lureWordRepeats[0] != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's lureWordRepeats should only have one item with a value of 1"));
        } else if (lureWordCounts.Count() != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's lureWordCounts should only have one item in it"));
        }

        wordsPerList = wordCounts[0];
        lureWordsPerList = lureWordCounts[0];

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
        for (int i = 0; i < randomWords.Count - 1; ++i) {
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

    // protected MemMapSession<Word> MakeAndSetTrial<U>(U randomSubset, bool encStim, bool recStim, bool recogStim, List<bool> wordOrders) 
    //     where U : WordRandomSubset<PairedWord>
    // {
    //     // Make run
    //     var frRun = MakeRun(randomSubset, encStim, recStim);
    //     // Set word orders
    //     for (int i=0; i < frRun.encoding.Count; ++i) {
    //         frRun.encoding[i].word.setCuedWord(wordOrders[i]);
    //     }
    //     for (int i=0; i < frRun.recall.Count; ++i) {
    //         frRun.recall[i].word.setCuedWord(wordOrders[i]);
    //     }

    //     return frRun;
    // }

    protected StimWordList<PairedWord> GenStimList(List<PairedWord> inputWords, bool stim) {
        var stimList = Enumerable.Range(1, inputWords.Count).Select(i => stim).ToList();
        return new StimWordList<PairedWord>(inputWords, stimList);
    }

    protected new MemMapSession<PairedWord> GenerateSession<V>(V randomSubset) 
        where V : WordRandomSubset<Word>
    {
        var session = new MemMapSession<PairedWord>();

        var tempRecogOrder = new List<int>() {1,2,3,4,5,6};

        for (int i = 0; i < Config.practiceLists; i++) {
            var wordOrders = Enumerable.Range(0, wordsPerList).Select(i => i % 2 == 0).ToList();
            session.Add(MakeTrial(randomSubset, false, false, false, wordOrders, tempRecogOrder));
        }

        for (int i = 0; i < Config.preNoStimLists; i++) {
            var wordOrders = Enumerable.Range(0, wordsPerList).Select(i => i % 2 == 0).ToList();
            session.Add(MakeTrial(randomSubset, false, false, false, wordOrders, tempRecogOrder));
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
            var wordOrders = GenerateUniqueBoolLists(numListsPerBlock, numStimMethods);

            // Create a list, for the 'encoding' stim type, of MemMapTrials with the wordOrders
            var encLists = new List<MemMapTrial<PairedWord>>();
            var numEncListsInBlock = Math.Min(numEncLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numListsPerBlock; i++) {
                encLists.Add(MakeTrial(randomSubset, true, false, false, wordOrders[i], tempRecogOrder));
            }

            // Create a list, for the 'retrieval' stim type, of MemMapTrials with the wordOrders
            var retLists = new List<MemMapTrial<PairedWord>>();
            var numRetListsInBlock = Math.Min(numRetLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numListsPerBlock; i++) {
                retLists.Add(MakeTrial(randomSubset, false, true, true, wordOrders[i], tempRecogOrder));
            }

            // Create a list, for the 'encoding and retrieval' stim type, of MemMapTrials with the wordOrders
            var encAndRetLists = new List<MemMapTrial<PairedWord>>();
            var numEncAndRetListsInBlock = Math.Min(numEncAndRetLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numListsPerBlock; i++) {
                encAndRetLists.Add(MakeTrial(randomSubset, true, true, true, wordOrders[i], tempRecogOrder));
            }

            // Create a list, for the 'no stim' stim type, of MemMapTrials with the wordOrders
            var noStimLists = new List<MemMapTrial<PairedWord>>();
            var numNoStimListsInBlock = Math.Min(numNoStimLists-blockNum*numBlocks, numListsPerBlock);
            for (int i = 0; i < numListsPerBlock; i++) {
                noStimLists.Add(MakeTrial(randomSubset, false, false, false, wordOrders[i], tempRecogOrder));
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
