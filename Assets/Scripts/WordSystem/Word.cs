

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