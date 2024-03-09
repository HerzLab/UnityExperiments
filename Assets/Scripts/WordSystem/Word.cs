//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

public class Word {
    public string word { get; protected set; }

    public Word() {
        word = "";
    }
    public Word(string line) {
        string[] items = line.Split('\t');
        word = items[0];
    }

    public static implicit operator string(Word word) {
        return word;
    }
    public override string ToString() {
        return word;
    }

    public virtual string ToTSV() {
        return word;
    }

    public virtual string ToDisplayString() {
        return word;
    }
}
