public class PairedWord : Word {
    public string pairedWord { get; protected set; }

    public PairedWord() : base("") {
        pairedWord = "";
    }
    public PairedWord(string line) {
        string[] items = line.Split('\t');
        word = items[0];
        pairedWord = items[1];
    }
    public PairedWord(string word, string pairedWord) : base(word) {
        this.pairedWord = pairedWord;
    }

    public override string ToDisplayString() {
        return word+"\n\n"+pairedWord;
    }

    public override string ToTSV() {
        return word + "\t" + pairedWord;
    }

    public override string ToString() {
        return $"({word}, {pairedWord})";
    }
}
