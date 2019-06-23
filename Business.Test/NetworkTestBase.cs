using Trustcoin.Business;

namespace Business.Test
{
    public abstract class NetworkTestBase
    {
        protected readonly INetwork _network = new Network();
        protected readonly IAccount MyAccount;
        protected const string AccountName = "MyAccount";
        protected const string OtherAccountName = "OtherAccount";

        protected NetworkTestBase()
        {
            MyAccount = _network.CreateAccount(AccountName);
            _network.CreateAccount(OtherAccountName);
        }
    }
}