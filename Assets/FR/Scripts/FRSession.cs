using System;
using UnityEPL;

namespace UnityEPL {

    public class FRRun<T> 
        where T : Word
    {
        public StimWordList<T> encoding;
        public StimWordList<T> recall;
        public bool encodingStim;
        public bool recallStim;

        public FRRun(StimWordList<T> encodingList, StimWordList<T> recallList,
            bool setEncodingStim = false, bool setRecallStim = false) {
            encoding = encodingList;
            recall = recallList;
            encodingStim = setEncodingStim;
            recallStim = setRecallStim;
        }
    }

    [Serializable]
    public class FRSession<T> : Timeline<FRRun<T>> 
        where T : Word 
    {

        public bool NextWord() {
            return GetState().encoding.IncrementState();
            return GetState().recall.IncrementState();
        }

        public WordStim<T> GetEncWord() {
            return GetState().encoding.GetState();
        }
        public WordStim<T> GetRecWord() {
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

}
