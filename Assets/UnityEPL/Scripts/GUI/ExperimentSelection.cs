﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEPL {

    /// <summary>
    /// This is attached to the dropdown menu which selects experiments.
    /// 
    /// It only needs to call UnityEPL.SetExperimentName().
    /// </summary>
    public class ExperimentSelection : MonoBehaviour {
        public InterfaceManager manager;

        void Awake() {
            manager = InterfaceManager.Instance;

            UnityEngine.UI.Dropdown dropdown = GetComponent<UnityEngine.UI.Dropdown>();

            List<string> experiments = new(Config.availableExperiments);

            dropdown.AddOptions(new List<string>(new string[] { "Select Task..." }));
            dropdown.AddOptions(experiments);
            SetExperiment();
        }

        public void SetExperiment() {
            UnityEngine.UI.Dropdown dropdown = GetComponent<UnityEngine.UI.Dropdown>();

            if (dropdown.captionText.text != "Select Task...") {
                Debug.Log("Task chosen");

                manager.LoadExperimentConfig(dropdown.captionText.text);
            }
        }
    }

}