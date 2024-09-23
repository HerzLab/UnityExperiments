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

        public static int restDurationMs { get { return GetSetting<int>("restDurationMs"); } }
        public static int numPracticeLists { get { return GetSetting<int>("numPracticeLists"); } }
        public static int numNoStimLists { get { return GetSetting<int>("numNoStimLists"); } }
        public static int numPreNoStimLists { get { return GetSetting<int>("numPreNoStimLists"); } }
        public static int numEncodingLists { get { return GetSetting<int>("numEncodingLists"); } }
        public static int numRetrievalLists { get { return GetSetting<int>("numRetrievalLists"); } }
        public static int numEncodingAndRetrievalLists { get { return GetSetting<int>("numEncodingAndRetrievalLists"); } }
        public static bool optionalExtraPracticeTrials { get { return GetSetting<bool>("optionalExtraPracticeTrials"); } }
        public static bool onlyPracticeOnFirstSession { get { return GetSetting<bool>("onlyPracticeOnFirstSession"); } }

        // RepFRExperiment.cs
        public static int[] wordRepeats { get { return GetSetting<int[]>("wordRepeats"); } }
        public static int[] wordCounts { get { return GetSetting<int[]>("wordCounts"); } }

        // ltpRepFRExperiment.cs
        public static int[] restLists { get { return GetSetting<int[]>("restLists"); } }

        // CPSExperiment.cs
        public static string video { get { return GetSetting<string>("video"); } }

        // MemMapExperiment.cs
        public static int[] lureWordRepeats { get { return GetSetting<int[]>("lureWordRepeats"); } }
        public static int[] lureWordCounts { get { return GetSetting<int[]>("lureWordCounts"); } }
    }

}