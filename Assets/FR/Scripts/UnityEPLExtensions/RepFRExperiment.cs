using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEPL {

    public class RepFRExperiment : ExperimentBase<RepFRExperiment> {
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
            EndTrials();
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
            //await InterfaceManager.Delay((int)(manager.lowBeep.clip.length * 1000) + 100)
            manager.recorder.StartRecording(wavPath);
            manager.textDisplayer.DisplayText("microphone test recording", "<color=red>Recording...</color>");
            await InterfaceManager.Delay(Config.micTestDuration);

            manager.textDisplayer.DisplayText("microphone test playing", "<color=green>Playing...</color>");
            var clip = manager.recorder.StopRecording();
            manager.playback.Play(clip);
            await InterfaceManager.Delay(Config.micTestDuration);
        }
    }

}