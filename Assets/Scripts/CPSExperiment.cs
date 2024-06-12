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
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEPL;

public class CPSExperiment : ExperimentBase<CPSExperiment> {
    protected override void AwakeOverride() { }

    protected void Start() {
        Run();
    }

    protected void SetVideo() {
        // absolute video path
        string videoPath = Path.Combine(manager.fileManager.ExperimentRoot(), Config.video);

        if (videoPath == null) {
            throw new Exception("Video resource not found");
        }

        manager.videoControl.SetVideo(videoPath, true);
    }

    protected override async Task TrialStates() {
        await SetupExp();
        await ShowVideo();
        await FinishExperiment();
    }

    protected override Task PreTrialStates() { return Task.CompletedTask; }
    protected override Task PracticeTrialStates() { return Task.CompletedTask; }
    protected override Task PostTrialStates() { return Task.CompletedTask; }

    protected async Task SetupExp() {
        if (manager.hostPC == null) {
            throw new Exception("CPS experiment must use a Host PC.\n The hostPC is null");
        }
        await manager.hostPC.SendTrialMsgTS(0, true);
    }

    protected async Task FinishExperiment() {
        await manager.textDisplayer.PressAnyKey("display end message", LangStrings.SessionEnd());
    }

    protected async Task ShowVideo() {
        string startingPath = Path.Combine(manager.fileManager.ParticipantPath(), "..", "..", "CPS_Movies");
        var extensions = new[] {
            new SFB.ExtensionFilter("Videos", "mp4", "mov"),
            new SFB.ExtensionFilter("All Files", "*" ),
        };

        var videoPath = await manager.videoControl.SelectVideoFile(startingPath, extensions);
        UnityEngine.Debug.Log(videoPath);
        Dictionary<string, object> movieInfo = new() {
            { "movie title", Path.GetFileName(videoPath) },
            { "movie path", Path.GetDirectoryName(videoPath)},
            { "movie duration seconds", manager.videoControl.VideoLength()}
        };
        eventReporter.LogTS("movie", movieInfo);

        await manager.textDisplayer.PressAnyKey("instructions", LangStrings.CPSInstructions());

        UnityEngine.Debug.Log(1);
        await manager.hostPC.SendStateMsgTS(HostPcStateMsg.ENCODING(), movieInfo);

        // Remove 10s to not overrun video legnth
        UnityEngine.Debug.Log(2);
        var cclLength = manager.videoControl.VideoLength() - 10.0;
        var cclMsg = HostPcCclMsg.START_STIM(Convert.ToInt32(cclLength));
        await manager.hostPC.SendCCLMsgTS(cclMsg);
        await manager.videoControl.PlayVideo();
    }
}