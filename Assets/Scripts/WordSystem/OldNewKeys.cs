using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEPL;

public class OldNewKeys : MonoBehaviour {
    public TextMeshProUGUI rightKey;
    public TextMeshProUGUI leftKey;

    protected float fontSize = 0;
    protected bool oldNewPosition = false;
    protected bool isOn = false;

    public void SetKeySize() {
        var strList = new List<string>() { "old", "new" };
        fontSize = (int)UnityUtilities.FindMaxFittingFontSize(strList, rightKey);
        fontSize -= 10; // decrease font size for enabling enlargment

        rightKey.enableAutoSizing = false;
        rightKey.fontSizeMax = fontSize;
        rightKey.fontSize = fontSize;

        leftKey.enableAutoSizing = false;
        leftKey.fontSizeMax = fontSize;
        leftKey.fontSize = fontSize;
    }

    public void SetupKeyPositions() {
        oldNewPosition = InterfaceManager.stableRnd.Value.Next(0,2) != 0;
        leftKey.text = "";
        rightKey.text = "";
    }

    public void TurnOn() {
        if (oldNewPosition) {
            leftKey.text = "old";
            rightKey.text = "new";
        } else {
            leftKey.text = "new";
            rightKey.text = "old";
        }
        leftKey.fontSizeMax = fontSize;
        leftKey.fontSize = fontSize;
        leftKey.fontStyle &= ~FontStyles.Bold;
        rightKey.fontSizeMax = fontSize;
        rightKey.fontSize = fontSize;
        rightKey.fontStyle &= ~FontStyles.Bold;

        isOn = true;
    }
    public void TurnOff() {
        leftKey.text = "";
        rightKey.text = "";

        isOn = false;
    }

    void Update() {
        if (isOn) {
            if (InputManager.Instance.GetKeyDown(KeyCode.LeftShift)) {
                EventReporter.Instance.LogTS("old new keys", new() {
                    { "key", KeyCode.LeftShift.ToString() }
                });
                // Bold left key
                leftKey.fontSizeMax = fontSize + 6;
                leftKey.fontSize = fontSize + 6;
                leftKey.fontStyle |= FontStyles.Bold;
                // Unbold right key
                rightKey.fontSizeMax = fontSize;
                rightKey.fontSize = fontSize;
                rightKey.fontStyle &= ~FontStyles.Bold;
            } else if (InputManager.Instance.GetKeyDown(KeyCode.RightShift)) {
                EventReporter.Instance.LogTS("old new keys", new() {
                    { "key", KeyCode.RightShift.ToString() }
                });
                // Bold right key
                rightKey.fontSizeMax = fontSize + 6;
                rightKey.fontSize = fontSize + 6;
                rightKey.fontStyle |= FontStyles.Bold;
                // Unbold left key
                leftKey.fontSizeMax = fontSize;
                leftKey.fontSize = fontSize;
                leftKey.fontStyle &= ~FontStyles.Bold;
            }
        }
    }
}
