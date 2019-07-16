namespace Trustcoin.Core.Entities
{
    public class Artefact
    {
        public Artefact(string name, string ownerName)
        {
            Name = name;
            OwnerName = ownerName;
        }

        public string Name { get; }
        public string OwnerName { get; }
    }
}