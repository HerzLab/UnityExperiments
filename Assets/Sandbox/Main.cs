//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;
using System.Diagnostics;
using UnityEngine.UI;
using Unity.Collections;
using System.Text.RegularExpressions;
using System.Text;

using PsyForge;

public class Main : EventMonoBehaviour {
    protected override void AwakeOverride() { }

    const long delay = 10000000000;

    void Start() {
        UnityEngine.Debug.Log("ThreadID - Start: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        //UnityEngine.Debug.Log(ThreadPool.SetMinThreads(1, 1));
        //UnityEngine.Debug.Log(ThreadPool.SetMaxThreads(1, 1));

        //UnityEngine.Debug.Log(UnsafeUtility.IsBlittable(typeof(StackString)));
    }
}


