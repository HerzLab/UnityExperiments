//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>.

namespace UnityEPL {

    public static partial class Config {
        // Game Section Skips
        public static bool skipIntros { get { return Config.GetSetting<bool>("skipIntros"); } }

        // FRExperiment.cs
        public static string countdownVideo { get { return Config.GetSetting<string>("countdownVideo"); } }


        public static int distractorDurationMs { get { return Config.GetSetting<int>("distractorDurationMs"); } }
        public static int[] fixationDurationMs { get { return Config.GetSetting<int[]>("fixationDurationMs"); } }
        public static int[] postFixationDelayMs { get { return Config.GetSetting<int[]>("postFixationDelayMs"); } }
        public static int stimulusDurationMs { get { return Config.GetSetting<int>("stimulusDurationMs"); } }
        public static int[] interStimulusDurationMs { get { return Config.GetSetting<int[]>("interStimulusDurationMs"); } }
        public static int recallDurationMs { get { return Config.GetSetting<int>("recallDurationMs"); } }
        public static int recallOrientationDurationMs { get { return Config.GetSetting<int>("recallOrientationDurationMs"); } }
        public static int finalRecallDuration { get { return Config.GetSetting<int>("finalRecallDuration"); } }

        public static int recallStimIntervalMs { get { return Config.GetSetting<int>("recallStimIntervalMs"); } }
        public static int recallStimDurationMs { get { return Config.GetSetting<int>("recallStimDurationMs"); } }

        public static bool splitWordsOverTwoSessions { get { return Config.GetSetting<bool>("splitWordsOverTwoSessions"); } }


        // RepFRExperiment.cs
        public static int[] wordRepeats { get { return Config.GetSetting<int[]>("wordRepeats"); } }
        public static int[] wordCounts { get { return Config.GetSetting<int[]>("wordCounts"); } }
        public static int[] recallDelayMs { get { return Config.GetSetting<int[]>("recallDelayMs"); } }
        
        public static int restDurationMs { get { return Config.GetSetting<int>("restDurationMs"); } }
        public static int practiceLists { get { return Config.GetSetting<int>("practiceLists"); } }
        public static int preNoStimLists { get { return Config.GetSetting<int>("preNoStimLists"); } }
        public static int encodingOnlyLists { get { return Config.GetSetting<int>("encodingOnlyLists"); } }
        public static int retrievalOnlyLists { get { return Config.GetSetting<int>("retrievalOnlyLists"); } }
        public static int encodingAndRetrievalLists { get { return Config.GetSetting<int>("encodingAndRetrievalLists"); } }
        public static int noStimLists { get { return Config.GetSetting<int>("noStimLists"); } }

        // ltpRepFRExperiment.cs
        public static int[] restLists { get { return Config.GetSetting<int[]>("restLists"); } }

        // CPSExperiment.cs
        public static string video { get { return Config.GetSetting<string>("video"); } }

        // MemMapExperiment.cs
        public static int recogDurationMs { get { return Config.GetSetting<int>("recogDurationMs"); } }
        public static int[] stimEarlyOnsetMs { get { return Config.GetSetting<int[]>("stimEarlyOnsetMs"); } }
        public static int[] lureWordRepeats { get { return Config.GetSetting<int[]>("lureWordRepeats"); } }
        public static int[] lureWordCounts { get { return Config.GetSetting<int[]>("lureWordCounts"); } }
    }

}