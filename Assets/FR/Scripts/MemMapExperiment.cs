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
        // await NextPracticeListPrompt();
        // await CountdownVideo();
        // await Fixation();
        // await Encoding();
        // await MathDistractor();
        // await PauseBeforeRecall();
        // await RecallPrompt();
        // await Recall();
        FinishPracticeTrial();
    }
    protected override async Task TrialStates() {
        StartTrial();
        await NextListPrompt();
        await CountdownVideo();
        await Fixation();
        await Encoding();
        await MathDistractor();
        await Fixation();
        //await PauseBeforeRecall();
        //await RecallPrompt();
        await FreeRecall();
        FinishTrial();
        await CuedRecall();
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

    protected override void SetupWordList() {
        var wordRepeats = Config.wordRepeats;
        if (wordRepeats.Count() != 1 && wordRepeats[0] != 1) {
            ErrorNotifier.ErrorTS(new Exception("Config's wordRepeats should only have one item with a value of 1"));
        }

        wordsPerList = Config.wordCounts[0];
        blankWords = new List<PairedWord>(Enumerable.Range(1, wordsPerList).Select(i => new PairedWord("", "")).ToList());

        var sourceWords = ReadPairedWordpool();
        var words = new WordRandomSubset<PairedWord>(sourceWords);

        // TODO: (feature) Load Session
        currentSession = GenerateSession(words);
    }

    protected List<PairedWord> ReadPairedWordpool() {
        // wordpool is a file with 'word\tpaired word' as a header
        // with one category and one word per line.
        // repeats are described in the config file with two matched arrays,
        // repeats and counts, which describe the number of presentations
        // words can have and the number of words that should be assigned to
        // each of those presentation categories.
        string sourceList = manager.fileManager.GetWordList();
        Debug.Log(sourceList);
        var sourceWords = new List<PairedWord>();

        //skip line for tsv header
        foreach (var line in File.ReadLines(sourceList).Skip(1)) {
            string[] wordAndPairedWord = line.Split('\t');
            sourceWords.Add(new PairedWord(wordAndPairedWord[1], wordAndPairedWord[0]));
        }

        // copy full wordpool to session directory
        string path = System.IO.Path.Combine(manager.fileManager.SessionPath(), "wordpool.txt");
        File.WriteAllText(path, String.Join("\n", sourceWords));

        // copy full paired wordpool to session directory
        string pairedPath = System.IO.Path.Combine(manager.fileManager.SessionPath(), "paired_wordpool.txt");
        var pairedList = sourceWords.ConvertAll(x => $"{x.word}\t{x.pairedWord}");
        File.WriteAllText(pairedPath, String.Join("\n", pairedList));

        return sourceWords;
    }

    protected override FRRun<PairedWord> MakeRun<U>(U subsetGen, bool encStim, bool recStim) {
        var inputWords = subsetGen.Get(wordsPerList).ToList();
        var encList = WordGenerator<PairedWord>.Generate(inputWords, encStim);
        var recList = WordGenerator<PairedWord>.Generate(blankWords, recStim);
        return new FRRun<PairedWord>(encList, recList, encStim, recStim);
    }
    protected override FRSession<PairedWord> GenerateSession<U>(U randomSubset) {
        var session = new FRSession<PairedWord>();

        for (int i = 0; i < Config.practiceLists; i++) {
            session.Add(MakeRun(randomSubset, false, false));
        }

        for (int i = 0; i < Config.preNoStimLists; i++) {
            session.Add(MakeRun(randomSubset, false, false));
        }

        var randomized_list = new FRSession<PairedWord>();

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
