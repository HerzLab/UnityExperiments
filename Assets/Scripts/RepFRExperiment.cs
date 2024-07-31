//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEPL;

public class RepFRExperiment : ExperimentBase<RepFRExperiment, FRSession<Word>, FRTrial<Word>> {
    protected override void AwakeOverride() { }

    protected void Start() {
        Run();
    }

    protected void SetVideo() {
        // absolute video path
        string videoPath = System.IO.Path.Combine(manager.fileManager.ExperimentRoot(), Config.introductionVideo);

        if (videoPath == null) {
            throw new Exception("Video resource not found");
        }

        manager.videoControl.SetVideo(videoPath, true);
    }

    protected override async Task TrialStates() {
        await RecordTest();
        //SetVideo();
        //await manager.videoControl.PlayVideo();
        EndCurrentSession();
    }

    protected override Task PreTrialStates() { return Task.CompletedTask; }
    protected override Task PracticeTrialStates() { return Task.CompletedTask; }
    protected override Task PostTrialStates() { return Task.CompletedTask; }

    // NOTE: rather than use flags for the audio test, this is entirely based off of timings.
    // Since there is processing latency (which seems to be unity version dependent), this
    // is really a hack that lets us get through the mic test unscathed. More time critical
    // applications need a different approach
    protected async Task RecordTest() {
        string wavPath = System.IO.Path.Combine(manager.fileManager.SessionPath(), "microphone_test_"
                    + Clock.UtcNow.ToString("yyyy-MM-dd_HH_mm_ss") + ".wav");

        manager.lowBeep.Play();
        await DoWaitWhile(() => manager.lowBeep.isPlaying);
        //await manager.Delay((int)(manager.lowBeep.clip.length * 1000) + 100)
        manager.recorder.StartRecording(wavPath);
        textDisplayer.DisplayText("microphone test recording", LangStrings.MicrophoneTestRecording().Color("red"));
        await manager.Delay(Config.micTestDurationMs);

        textDisplayer.DisplayText("microphone test playing", LangStrings.MicrophoneTestPlaying().Color("green"));
        var clip = manager.recorder.StopRecording();
        manager.playback.Play(clip);
        await manager.Delay(Config.micTestDurationMs);
    }
}
