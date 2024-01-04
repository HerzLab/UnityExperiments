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
}
