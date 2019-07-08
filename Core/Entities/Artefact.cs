namespace Trustcoin.Core.Entities
{
    public class Artefact : IArtefact
    {
        public string Name { get; }
        public string OwnerName { get; }

        public Artefact(string name, string ownerName)
        {
            Name = name;
            OwnerName = ownerName;
        }
    }
}