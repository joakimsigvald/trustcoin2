namespace Trustcoin.Business
{
    public interface INetwork
    {
        public IAccount CreateAccount(string name);

        public IAgent FindAgent(string name);
    }
}