public class CategorizedWord : Word {
    public string category { get; protected set; }

    public CategorizedWord(string word, string category) : base(word) {
        this.category = category;
    }
}
