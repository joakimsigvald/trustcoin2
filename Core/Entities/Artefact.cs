namespace Trustcoin.Core.Entities
{
    public class Artefact : IArtefact
    {
        public string Name { get; }

        public Artefact(string name)
        {
            Name = name;
        }
    }
}