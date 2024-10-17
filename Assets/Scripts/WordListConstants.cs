//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using PsyForge.Experiment;

public abstract class WordListConstants : ExperimentConstants {
    public virtual int distractorDurationMs { get; }
    public virtual int[] fixationDurationMs { get; }
    public virtual int[] postFixationDelayMs { get; }
    public virtual int stimulusDurationMs { get; }
    public virtual int[] interStimulusDurationMs { get; }
    public virtual int[] recallDelayMs { get; }
    public virtual int recallDurationMs { get; }
    public virtual int recallOrientationDurationMs { get; }
    public virtual int finalRecallDuration { get; }

    public virtual int recallStimIntervalMs { get; }
    public virtual int recallStimDurationMs { get; }

    public virtual bool splitWordsOverTwoSessions { get; }
}