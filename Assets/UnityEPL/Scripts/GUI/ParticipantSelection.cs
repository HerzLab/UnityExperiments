﻿using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEPL {

    /// <summary>
    /// This is attached to the participant selection dropdown.  It is responsible for loading the information about the selected participant.
    /// 
    /// It also allows users to edit the loaded information with Increase/Decrease Session/List number buttons.
    /// </summary>
    [RequireComponent(typeof(Dropdown))]
    public class ParticipantSelection : EventMonoBehaviour {
        protected override void AwakeOverride() { }

        public UnityEngine.UI.InputField participantNameInput;
        public UnityEngine.UI.Text sessionNumberText;
        public UnityEngine.UI.Text listNumberText;

        public static int nextSessionNumber = 0;
        public static int nextListNumber = 0;

        private bool experimentUpdated = false;
        // ^ updtaed by GUI event when experiment is
        // selected from dropdown menu

        void Update() {
            // update participants when new experiments are loaded
            if (experimentUpdated && Config.experimentName != null) {
                experimentUpdated = false;
                FindParticipants();
            }
        }

        public void ExperimentUpdatedMB() {
            DoMB(ExperimentUpdatedHelper);
        }
        protected void ExperimentUpdatedHelper() {
            experimentUpdated = true;
        }

        
        public void ParticipantSelectedMB() {
            DoMB(ParticipantSelectedHelper);
        }
        protected void ParticipantSelectedHelper() {
            Dropdown dropdown = GetComponent<Dropdown>();
            if (dropdown.value <= 1) {
                participantNameInput.text = "New Participant";
            } else {
                LoadParticipant();
            }
        }


        public void DecreaseSessionNumberMB() {
            DoMB(DecreaseSessionNumberHelper);
        }
        protected void DecreaseSessionNumberHelper() {
            if (nextSessionNumber > 0)
                nextSessionNumber--;
            LoadSession();
        }

        public void IncreaseSessionNumberMB() {
            DoMB(IncreaseSessionNumberHelper);
        }
        protected void IncreaseSessionNumberHelper() {
            nextSessionNumber++;
            LoadSession();
        }

        protected void FindParticipants() {
            Dropdown dropdown = GetComponent<Dropdown>();

            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>() { "Select participant", "New Participant" });

            string participantDirectory = manager.fileManager.ExperimentPath();
            if (Directory.Exists(participantDirectory)) {
                string[] filepaths = Directory.GetDirectories(participantDirectory);
                List<string> filenames = new List<string>();

                for (int i = 0; i < filepaths.Length; i++)
                    if (manager.fileManager.isValidParticipant(Path.GetFileName(filepaths[i])))
                        filenames.Add(Path.GetFileName(filepaths[i]));

                dropdown.AddOptions(filenames);
            }
            dropdown.value = 0;
            dropdown.RefreshShownValue();

            nextSessionNumber = 0;
            nextListNumber = 0;
            UpdateTexts();
        }
        protected void LoadParticipant() {
            Dropdown dropdown = GetComponent<Dropdown>();
            string selectedParticipant = dropdown.captionText.text;

            if (!Directory.Exists(manager.fileManager.ParticipantPath(selectedParticipant)))
                throw new UnityException("You tried to load a participant that doesn't exist.");

            participantNameInput.text = selectedParticipant;

            nextSessionNumber = manager.fileManager.CurrentSession(selectedParticipant);

            UpdateTexts();
        }
        protected void LoadSession() {
            Dropdown dropdown = GetComponent<Dropdown>();
            string selectedParticipant = dropdown.captionText.text;
            string sessionFilePath = manager.fileManager.SessionPath(selectedParticipant, nextSessionNumber);
            UpdateTexts();
        }
        protected void UpdateTexts() {
            sessionNumberText.text = nextSessionNumber.ToString();
        }
    }

}