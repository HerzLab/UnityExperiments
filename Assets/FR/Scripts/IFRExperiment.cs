using System.Threading.Tasks;
using UnityEngine;
using UnityEPL;

public class IFRExperiment : FRExperiment {

    protected override async Task PracticeTrialStates() {
        StartTrial();
        await NextPracticeListPrompt();
        await CountdownVideo();
        await Fixation();
        await Encoding();
        await FixationDistractor();
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
        await FixationDistractor();
        await PauseBeforeRecall();
        await RecallPrompt();
        await FreeRecall();
        FinishTrial();
    }
}
