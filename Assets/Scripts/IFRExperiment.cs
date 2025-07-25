//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System.Threading.Tasks;
using UnityEngine;
using PsyForge;
using System.Threading;

public class IFRExperiment : FRExperiment {

    protected override async Awaitable PracticeTrialStates(CancellationToken ct) {
        await StartTrial();
        await NextPracticeTrialPrompt();
        await CountdownVideo();
        await Fixation();
        await Encoding();
        await FixationDistractor();
        await PauseBeforeRecall();
        await RecallOrientation();
        await FreeRecall();
    }
    protected override async Awaitable TrialStates(CancellationToken ct) {
        await StartTrial();
        await NextTrialPrompt();
        await CountdownVideo();
        await Fixation();
        await Encoding();
        await FixationDistractor();
        await PauseBeforeRecall();
        await RecallOrientation();
        await FreeRecall();
    }
}
