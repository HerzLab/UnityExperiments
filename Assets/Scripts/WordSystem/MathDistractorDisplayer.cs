//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEPL;

public class MathDistractorDiplayer : MonoBehaviour {
    public TextMeshProUGUI equation;
    protected int[] problemValues;
    public string Problem { get; protected set;} = "";
    public string Answer { get; protected set;} = "";

    protected DateTime startTime;

    public void Awake() {
        int fontSize = (int)equation.FindMaxFittingFontSize(new(){ "5 + 5 + 5 = 555" });
        fontSize -= 10; // Just for astetics

        equation.enableAutoSizing = false;
        equation.fontSizeMax = fontSize;
        equation.fontSize = fontSize;

        TurnOff();
    }

    public void TurnOff() {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Set a new equation with random values between 1 and 9 (inclusive).
    /// This also enables the MathDistractorDisplayer.
    /// </summary>
    /// <param name="numValues">The number of values in the equation</param>
    /// <exception cref="ArgumentException"></exception>
    public void SetNewRandomEquation(uint numValues) {
        if (numValues == 0) {
            throw new ArgumentException("Math distractor SetNewRandomEquation numValues must be greater than 0.");
        }

        var values = new int[numValues];
        for (int i = 0; i < numValues; i++) {
            values[i] = UnityEngine.Random.Range(1, 10);
        }
        SetNewEquation(values);
    }

    /// <summary>
    /// Set a new equation to display.
    /// This also enables the MathDistractorDisplayer.
    /// </summary>
    /// <param name="values">The values to display in the equation</param>
    public void SetNewEquation(int[] values) {
        gameObject.SetActive(true);

        problemValues = values;
        Problem = string.Join(" + ", values) + " = ";
        Answer = "";
        startTime = Clock.UtcNow;

        equation.text = Problem;

        Dictionary<string, object> dataDict = new() {
            { "problemValues", problemValues },
            { "equation", equation.text },
        };
        EventReporter.Instance.LogTS("math distractor new problem", dataDict);

        
    }

    /// <summary>
    /// Add a digit to the answer.
    /// Currently only diplays 3 digits in an answer.
    /// </summary>
    /// <param name="digit"></param>
    /// <exception cref="ArgumentException"></exception>
    public void AddDigitToAnswer(int digit) {
        if (digit < 0 || digit > 9) {
            throw new ArgumentException($"Math distractor AddDigitToAnswer value {digit} must be a digit.");
        } 
        
        if (Answer.Length < 3) {
            Answer += digit;
        }
        equation.text = Problem + Answer;

        Dictionary<string, object> dataDict = new() {
            { "problemValues", problemValues },
            { "answer", Answer },
            { "equation", equation.text },
        };
        EventReporter.Instance.LogTS("math distractor update answer", dataDict);
    }

    /// <summary>
    /// Remove the last digit from the answer.
    /// </summary>
    public void RemoveDigitFromAnswer() {
        if (Answer.Length > 0) {
            Answer = Answer.Substring(0, Answer.Length - 1);
        }
        equation.text = Problem + Answer;

        Dictionary<string, object> dataDict = new() {
            { "problemValues", problemValues },
            { "answer", Answer },
            { "equation", equation.text },
        };
        EventReporter.Instance.LogTS("math distractor update answer", dataDict);
    }

    /// <summary>
    /// Submit the current answer and check if it is correct.
    /// </summary>
    /// <returns>Whether the submitted answer was correct and response time in milliseconds.</returns>
    public (bool, int) SubmitAnswer() {
        bool correct;
        if (Answer == "") {
            correct = false;
        } else {
            correct = problemValues.Sum() == int.Parse(Answer);
        }

        int responseTimeMs = (int)(Clock.UtcNow - startTime).TotalMilliseconds;
        Dictionary<string, object> dataDict = new() {
            { "problemValues", problemValues },
            { "answer", Answer },
            { "equation", equation.text },
            { "correct", correct },
            { "responseTime", responseTimeMs },
        };
        EventReporter.Instance.LogTS("math distractor submit answer", dataDict);

        return (correct, responseTimeMs);
    }
}