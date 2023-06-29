using System.Threading.Tasks;
using UnityEngine;
using UnityEPL;

public class IFRExperiment : FRExperiment {

    protected override async Task PracticeTrialStates() {
        StartTrial();
        await NextPracticeListPrompt();
        await CountdownVideo();
        await Orientation();
        await Encoding();
        await FixationDistractor();
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
        await FixationDistractor();
        await PauseBeforeRecall();
        await RecallPrompt();
        await Recall();
        FinishTrial();
    }
}
