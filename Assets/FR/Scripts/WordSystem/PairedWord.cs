public class PairedWord : Word {
    public string pairedWord { get; protected set; }
    public string cuedWord { get; protected set; }
    public string recogWord { get; protected set; }

    public PairedWord() : base("") {
        pairedWord = "";
        cuedWord = "";
        recogWord = "";
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

    public void setCuedWord(bool useBaseWord) {
        if (useBaseWord) {
            cuedWord = word;
            recogWord = pairedWord;
        } else {
            cuedWord = pairedWord;
            recogWord = word;
        }
    }

    public override string ToString() {
        return $"({word}, {pairedWord}, {cuedWord}, {recogWord})";
    }
}
