//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEPL.Utilities;
using UnityEPL.Experiment;

[Serializable]
public abstract class WordListSessionBase<WordType, TrialType> : ExperimentSession<TrialType>
    where WordType : Word
    where TrialType : FRTrial<WordType>
{

    public Timeline<TrialType> trials = new();
    public TrialType Trial;
    public int NumTrials { get {return trials.Count();}}

    public void AddTrial(TrialType trial) {
        trials.Add(trial);
    }

    public void AddTrials(IEnumerable<TrialType> trials) {
        this.trials.AddRange(trials);
    }

    public bool NextTrial() {
        if (Trial == null && trials.Count > 0) {
            Trial = trials[0];
            return true;
        } else if (trials.IncrementState()) {
            Trial = trials.GetState();
            return true;
        }
        return false;
    }

    public void PrintAllWordsToDebugLog() {
        UnityEngine.Debug.Log("Words in each list\n" +
            String.Join("\n", trials.items.ConvertAll(x => String.Join(", ", x.encoding.words))));
    }
}
