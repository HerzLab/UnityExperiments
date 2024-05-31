//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>.

namespace UnityEPL {

    public static partial class Config {
        // Game Section Skips
        public static bool skipIntros { get { return GetSetting<bool>("skipIntros"); } }

        // FRExperiment.cs
        public static string countdownVideo { get { return GetSetting<string>("countdownVideo"); } }


        public static int distractorDurationMs { get { return GetSetting<int>("distractorDurationMs"); } }
        public static int[] fixationDurationMs { get { return GetSetting<int[]>("fixationDurationMs"); } }
        public static int[] postFixationDelayMs { get { return GetSetting<int[]>("postFixationDelayMs"); } }
        public static int stimulusDurationMs { get { return GetSetting<int>("stimulusDurationMs"); } }
        public static int[] interStimulusDurationMs { get { return GetSetting<int[]>("interStimulusDurationMs"); } }
        public static int recallDurationMs { get { return GetSetting<int>("recallDurationMs"); } }
        public static int recallOrientationDurationMs { get { return GetSetting<int>("recallOrientationDurationMs"); } }
        public static int finalRecallDuration { get { return GetSetting<int>("finalRecallDuration"); } }

        public static int recallStimIntervalMs { get { return GetSetting<int>("recallStimIntervalMs"); } }
        public static int recallStimDurationMs { get { return GetSetting<int>("recallStimDurationMs"); } }

        public static bool splitWordsOverTwoSessions { get { return GetSetting<bool>("splitWordsOverTwoSessions"); } }


        // RepFRExperiment.cs
        public static int[] wordRepeats { get { return GetSetting<int[]>("wordRepeats"); } }
        public static int[] wordCounts { get { return GetSetting<int[]>("wordCounts"); } }
        public static int[] recallDelayMs { get { return GetSetting<int[]>("recallDelayMs"); } }
        
        public static int restDurationMs { get { return GetSetting<int>("restDurationMs"); } }
        public static int practiceLists { get { return GetSetting<int>("practiceLists"); } }
        public static int preNoStimLists { get { return GetSetting<int>("preNoStimLists"); } }
        public static int encodingOnlyLists { get { return GetSetting<int>("encodingOnlyLists"); } }
        public static int retrievalOnlyLists { get { return GetSetting<int>("retrievalOnlyLists"); } }
        public static int encodingAndRetrievalLists { get { return GetSetting<int>("encodingAndRetrievalLists"); } }
        public static int noStimLists { get { return GetSetting<int>("noStimLists"); } }

        // ltpRepFRExperiment.cs
        public static int[] restLists { get { return GetSetting<int[]>("restLists"); } }

        // CPSExperiment.cs
        public static string video { get { return GetSetting<string>("video"); } }

        // MemMapExperiment.cs
        public static int recogDurationMs { get { return GetSetting<int>("recogDurationMs"); } }
        public static int[] stimEarlyOnsetMs { get { return GetSetting<int[]>("stimEarlyOnsetMs"); } }
        public static int[] lureWordRepeats { get { return GetSetting<int[]>("lureWordRepeats"); } }
        public static int[] lureWordCounts { get { return GetSetting<int[]>("lureWordCounts"); } }
    }

}