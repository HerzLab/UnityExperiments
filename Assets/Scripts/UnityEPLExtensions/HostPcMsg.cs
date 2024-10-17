//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>.

namespace PsyForge.ExternalDevices {
    public partial class HostPcStatusMsg {
        public static HostPcStatusMsg REST() { return new HostPcStatusMsg("REST"); }
        public static HostPcStatusMsg ORIENT() { return new HostPcStatusMsg("ORIENT"); }
        public static HostPcStatusMsg COUNTDOWN() { return new HostPcStatusMsg("COUNTDOWN"); }
        public static HostPcStatusMsg DISTRACT() { return new HostPcStatusMsg("DISTRACT"); }
        public static HostPcStatusMsg INSTRUCT() { return new HostPcStatusMsg("INSTRUCT"); }
        public static HostPcStatusMsg SYNC() { return new HostPcStatusMsg("SYNC"); }
        public static HostPcStatusMsg VOCALIZATION() { return new HostPcStatusMsg("VOCALIZATION"); }
        public static HostPcStatusMsg FIXATION() { return new HostPcStatusMsg("FIXATION"); }
        public static HostPcStatusMsg ENCODING(uint trailNum) { return new HostPcStatusMsg("ENCODING", new() {{"current_trial", trailNum}}); }
        public static HostPcStatusMsg RETRIEVAL() { return new HostPcStatusMsg("RETRIEVAL"); }
        public static HostPcStatusMsg MATH() { return new HostPcStatusMsg("MATH"); }
        public static HostPcStatusMsg ISI(float duration) { return new HostPcStatusMsg("ISI", new() {{"duration", duration}}); }
        public static HostPcStatusMsg FREE_RECALL(float duration, uint trialNum) { return new HostPcStatusMsg("FREE_RECALL", new() {{"duration", duration}, {"current_trial", trialNum}}); }
        public static HostPcStatusMsg CUED_RECALL(float duration, uint trialNum) { return new HostPcStatusMsg("CUED_RECALL", new() {{"duration", duration}, {"current_trial", trialNum}}); }
        public static HostPcStatusMsg FINAL_RECALL(float duration, uint trialNum) { return new HostPcStatusMsg("FINAL_RECALL", new() {{"duration", duration}, {"current_trial", trialNum}}); }
        public static HostPcStatusMsg RECOGNITION(float duration, uint trialNum) { return new HostPcStatusMsg("RECOGNITION", new() {{"duration", duration}, {"current_trial", trialNum}}); }
    }
}