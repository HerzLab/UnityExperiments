using System;
using UnityEPL;

namespace UnityEPL {

    public class FRRun<WordType> 
        where WordType : Word
    {
        public StimWordList<WordType> encoding;
        public StimWordList<WordType> recall;
        public bool encodingStim;
        public bool recallStim;


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
