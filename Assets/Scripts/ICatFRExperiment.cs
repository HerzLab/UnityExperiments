using System.Threading.Tasks;
using UnityEngine;
using UnityEPL;

public class ICatFRExperiment : CatFRExperiment {

    protected override async Task PracticeTrialStates() {
        StartTrial();
        await NextPracticeListPrompt();
        await CountdownVideo();
        await Fixation();
        await Encoding();
        await FixationDistractor();
        await PauseBeforeRecall();
        await RecallOrientation();
        await FreeRecall();
        FinishTrial();
    }
    protected override async Task TrialStates() {
        StartTrial();
        await NextListPrompt();
        await CountdownVideo();
        await Fixation();
        await Encoding();
        await FixationDistractor();
        await PauseBeforeRecall();
        await RecallOrientation();
        await FreeRecall();
        FinishTrial();
    }
}
