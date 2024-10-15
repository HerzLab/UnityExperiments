//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System.Collections.Generic;

public class CategorizedWord : Word {
    public string category { get; protected set; }

    public CategorizedWord() : base("") {
        category = "";
    }
    public CategorizedWord(string line) {
        string[] items = line.Split('\t');
        word = items[1];
        category = items[0];
    }
    public CategorizedWord(string word, string category) : base(word) {
        this.category = category;
    }

    public override string ToTSV() {
        return category + "\t" + word;
    }

    public override Dictionary<string, object> ToJSON() {
        var json = base.ToJSON();
        json.Add("category", category);
        return json;
    }
}
