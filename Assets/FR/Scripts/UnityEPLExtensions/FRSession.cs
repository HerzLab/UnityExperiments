using System;
using UnityEPL;

namespace UnityEPL {

    public class FRRun {
        public StimWordList encoding;
        public StimWordList recall;
        public bool encoding_stim;
        public bool recall_stim;

        public FRRun(StimWordList encoding_list, StimWordList recall_list,
            bool set_encoding_stim = false, bool set_recall_stim = false) {
            encoding = encoding_list;
            recall = recall_list;
            encoding_stim = set_encoding_stim;
            recall_stim = set_recall_stim;
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
