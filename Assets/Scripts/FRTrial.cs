//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System.Collections.Generic;
using PsyForge;

public class FRTrial<WordType>
    where WordType : Word
{
    public StimWordList<WordType> encoding;
    public StimWordList<WordType> recall;
    public bool encodingStim;
    public bool recallStim;

    public virtual Dictionary<string, bool> GetStimValues() {
        return new() { {"encodingStim", encodingStim}, {"recallStim", recallStim} };
    }

    public FRTrial(StimWordList<WordType> encodingList, StimWordList<WordType> recallList,
        bool setEncodingStim = false, bool setRecallStim = false) {
        encoding = encodingList;
        recall = recallList;
        encodingStim = setEncodingStim;
        recallStim = setRecallStim;
    }
}