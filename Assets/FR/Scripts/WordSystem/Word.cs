public class Word {
    public string word { get; protected set; }

    public Word(string word) {
        this.word = word;
    }

    public static implicit operator string(Word word) {
        return word;
    }
    public override string ToString() {
        return word;
    }
}
