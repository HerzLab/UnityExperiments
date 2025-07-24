//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>.

namespace PsyForge {

    public static partial class Config {
        // Game Section Skips
        public static Conf<bool> skipIntros;

        // FRExperiment.cs
        public static Conf<string> countdownVideo;

        public static Conf<int> restDurationMs;
        public static Conf<int> numPracticeLists;
        public static Conf<int> numNoStimLists;
        public static Conf<int> numPreNoStimLists;
        public static Conf<int> numEncodingStimLists;
        public static Conf<int> numRetrievalStimLists;
        public static Conf<int> numEncodingAndRetrievalStimLists;
        public static Conf<bool> optionalExtraPracticeTrials;
        public static Conf<bool> onlyPracticeOnFirstSession;

        // RepFRExperiment.cs
        public static Conf<int[]> wordRepeats;
        public static Conf<int[]> wordCounts;

        // ltpRepFRExperiment.cs
        public static Conf<int[]> restLists;

        // CPSExperiment.cs
        public static Conf<string> video;

        // MemMapExperiment.cs
        public static Conf<int[]> lureWordRepeats;
        public static Conf<int[]> lureWordCounts;
    }

}