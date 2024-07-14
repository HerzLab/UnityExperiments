//Copyright (c) 2024 Jefferson University

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>.

namespace UnityEPL {

    public static partial class LangStrings {
        // FRExperimentBase
        public static LangString TrialPrompt(uint trialNum) { return new( new() {
            { Language.English, $"Press any key to start Trial {trialNum}." },
        }); }
        public static LangString PracticeTrialPrompt(uint trialNum) { return new( new() {
            { Language.English, $"Press any key to start Practice Trial {trialNum}." },
        }); }
        public static LangString SessionEnd() { return new( new() {
            { Language.English, "Yay! Session Complete.\n\n Press any key to quit." },
        }); }
        
        // MemMapExperiment
        public static LangString DoPracticeQuestion() { return new( new() {
            { Language.English, "Would you like to do a practice round?"
                + "\n\nPress Y to do a practice round."
                + "\nPress N to continue to the real task." },
        }); }
        public static LangString RepeatPracticeQuestion() { return new( new() {
            { Language.English, "Would you like to do another practice round?"
                + "\n\nPress Y to do another practice round."
                + "\nPress N to continue to the real task." },
        }); }
        public static LangString RecognitionInstructions() { return new( new() {
            { Language.English, "For each word, indicate if it was shown in this list (‘old’) or not (‘new’) using the right/left shift keys.\n\n" +
            "<size=-20>Press any key to start</size>" },
        }); }
        public static LangString QuestioneerQ1() { return new( new() {
            { Language.English, "Can you recall any specific moments during the experiment when you knew or felt stimulation was being delivered?\n\nYes (Y) or No (N)" },
        }); }
        public static LangString QuestioneerQ1a() { return new( new() {
            { Language.English, "Please describe when and why you think stimulation was delivered.\n\nPress any key to continue to the recording." },
        }); }
        public static LangString QuestioneerQ1b() { return new( new() {
            { Language.English, "Please describe when and why you think stimulation was delivered.\n\n<color=red>Recording...</color>\n\nPress any key when finished" },
        }); }
        
        // CPSExperiment
        public static LangString CPSInstructions() { return new( new() {
            { Language.English, "In this experiment, you will watch a short educational film lasting about twenty-five minutes. Please pay attention to the film to the best of your ability. You will be asked a series of questions about the video after its completion. After the questionnaire, you will have the opportunity to take a break.\n\n Press any key to begin watching." },
        }); }
    }

}