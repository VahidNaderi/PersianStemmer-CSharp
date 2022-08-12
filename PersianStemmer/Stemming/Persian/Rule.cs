namespace Stemming.Persian
{
    public class Rule
    {
        public string Body { get; set; }
        public string Substitution { get; set; }
        public char PoS { get; set; }
        public byte MinLength { get; set; }
        public bool State { get; set; }

        public Rule(string body, string substitution, char poS, byte minLength, bool state)
        {
            this.Body = body;
            this.Substitution = substitution;
            this.PoS = poS;
            this.MinLength = minLength;
            this.State = state;
        }
    }
}