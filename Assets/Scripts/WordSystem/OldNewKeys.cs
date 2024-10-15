using System.Collections.Generic;
using UnityEngine;
using TMPro;

using UnityEPL;
using UnityEPL.Extensions;
using UnityEPL.DataManagement;

public class OldNewKeys : MonoBehaviour {
    public TextMeshProUGUI rightKey;
    public TextMeshProUGUI leftKey;

    protected float smallFontSize = 0;
    protected float largeFontSize = 0;
    protected bool oldNewPosition = false;
    protected bool isOn = false;
    

    public void SetKeySize() {
        var strList = new List<string>() { "old", "new" };
        rightKey.Bold(true);
        gameObject.SetActive(true);
        largeFontSize = (int)rightKey.FindMaxFittingFontSize(strList);
        gameObject.SetActive(false);
        smallFontSize = largeFontSize - 6; // decrease font size for enabling enlargment

        rightKey.enableAutoSizing = false;
        rightKey.fontSizeMax = largeFontSize;
        rightKey.fontSize = smallFontSize;
        rightKey.Bold(false);

        leftKey.enableAutoSizing = false;
        leftKey.fontSizeMax = largeFontSize;
        leftKey.fontSize = smallFontSize;
        leftKey.Bold(false);
    }

    public void SetupKeyPositions() {
        oldNewPosition = UnityEPL.Utilities.Random.StableRnd.Next(0,2) != 0;
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
        leftKey.fontSizeMax = largeFontSize;
        leftKey.fontSize = smallFontSize;
        leftKey.Bold(false);
        rightKey.fontSizeMax = largeFontSize;
        rightKey.fontSize = smallFontSize;
        rightKey.Bold(false);

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
                    { "key", KeyCode.LeftShift.ToString() },
                    { "old new", oldNewPosition ? "old" : "new" },
                });
                // Bold left key
                leftKey.fontSize = largeFontSize;
                leftKey.Bold(true);
                // Unbold right key
                rightKey.fontSize = smallFontSize;
                rightKey.Bold(false);
            } else if (InputManager.Instance.GetKeyDown(KeyCode.RightShift)) {
                EventReporter.Instance.LogTS("old new keys", new() {
                    { "key", KeyCode.RightShift.ToString() },
                    { "old new", oldNewPosition ? "new" : "old" },
                });
                // Bold right key
                rightKey.fontSize = largeFontSize;
                rightKey.Bold(true);
                // Unbold left key
                leftKey.fontSize = smallFontSize;
                leftKey.Bold(false);
            }
        }
    }
}
