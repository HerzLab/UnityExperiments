using System;
using UnityEPL;

namespace UnityEPL {

    public class FRRun {
        public StimWordList encoding;
        public StimWordList recall;
        public bool encodingStim;
        public bool recallStim;

        public FRRun(StimWordList encodingList, StimWordList recallList,
            bool setEncodingStim = false, bool setRecallStim = false) {
            encoding = encodingList;
            recall = recallList;
            encodingStim = setEncodingStim;
            recallStim = setRecallStim;
        }
    }

    [Serializable]
    public class FRSession : Timeline<FRRun> {

        public bool NextWord() {
            return GetState().encoding.IncrementState();
        }

        public WordStim GetWord() {
            return GetState().encoding.GetState();
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
    }

}
