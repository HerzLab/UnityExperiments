//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections.Generic;
using UnityEPL;

namespace UnityEPL {

    public class FRRun<WordType> 
        where WordType : Word
    {
        public StimWordList<WordType> encoding;
        public StimWordList<WordType> recall;
        public bool encodingStim;
        public bool recallStim;

        public virtual Dictionary<string, bool> GetStimValues() {
            return new() { {"encodingStim", encodingStim}, {"recallStim", recallStim} };
        }

        public FRRun(StimWordList<WordType> encodingList, StimWordList<WordType> recallList,
            bool setEncodingStim = false, bool setRecallStim = false) {
            encoding = encodingList;
            recall = recallList;
            encodingStim = setEncodingStim;
            recallStim = setRecallStim;
        }
    }

    [Serializable]
    public class FRSessionBase<WordType, TrialType> : Timeline<TrialType>
        where WordType : Word
        where TrialType : FRRun<WordType>
    {

        public bool NextWord() {
            bool ret = GetState().recall.IncrementState();
            return ret & GetState().encoding.IncrementState();
        }

        public WordStim<WordType> GetEncWord() {
            return GetState().encoding.GetState();
        }
        public WordStim<WordType> GetRecWord() {
            return GetState().recall.GetState();
        }

        public bool NextList() {
            return IncrementState();
        }

        public int GetSerialPos() {
            return GetState().encoding.index;
        }

        public int GetListIndex() {
            return index;
        }

        public void PrintAllWordsToDebugLog() {
            UnityEngine.Debug.Log("Words in each list\n" +
                String.Join("\n", items.ConvertAll(x => String.Join(", ", x.encoding.words))));
        }
    }

    public class FRSession<T> : FRSessionBase<T, FRRun<T>> 
        where T : Word
    {}

}
