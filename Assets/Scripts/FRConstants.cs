//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System;

public class FRConstants : WordListConstants {
    // WordListExperimentBase
    public override int distractorDurationMs => 20000;
    public override int[] fixationDurationMs => new int[2] {1000, 1400};
    public override int[] postFixationDelayMs => new int[2] {-1, -1};
    public override int stimulusDurationMs => 1600;
    public override int[] interStimulusDurationMs => new int[2] {750, 1000};
    public override int[] recallDelayMs => new int[2] {1000, 1400};
    public override int recallDurationMs => 30000;
    public override int recallOrientationDurationMs => 1000;
    public override int finalRecallDuration => throw new NotImplementedException();
    public override int recallStimIntervalMs => throw new NotImplementedException();
    public override int recallStimDurationMs => throw new NotImplementedException();
    public override bool splitWordsOverTwoSessions => true;
}