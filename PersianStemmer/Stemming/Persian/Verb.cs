namespace Stemming.Persian
{
    public class Verb
    {
        public string Present { get; set; }
        public string Past { get; set; }

        public Verb(string past, string present)
        {
            this.Present = present;
            this.Past = past;

        }

    }
}