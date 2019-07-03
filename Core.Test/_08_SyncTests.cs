using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class SyncTests : TestBase
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(99)]
        public void GivenNoPeers_WhenSyncSelf_ThenGetSameMoney(float money)
        {
            MyAccount.Self.Money = (Money)money;
            MyAccount.SyncAll();

            Assert.Equal(money, MyAccount.Self.Money);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 100, 50)]
        public void Given_1_PeerWithFullTrustAndConnectivity_WhenSyncSelf_ThenGetMeanPeerMoney(
            float myMoneyBefore, float peerAssessment, float myMoneyAfter)
        {
            Interconnect(MyAccount, OtherAccount);
            OtherAccount.SetMoney(MyAccountName, (Money)peerAssessment);
            MyAccount.Self.Money = (Money)myMoneyBefore;
            MyAccount.SetTrust(OtherAccountName, (Weight)1);

            MyAccount.SyncAll();

            Assert.Equal(myMoneyAfter, MyAccount.Self.Money);
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(0, 100, 100, 100)]
        [InlineData(0, 50, 100, 50)]
        public void Given_2_PeersWithFullTrustAndConnectivity_WhenSyncSelf_ThenGetMeanPeerMoney(
            float myMoneyBefore,
            float firstPeerAssessment,
            float secondPeerAssessment,
            float myMoneyAfter)
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            OtherAccount.SetMoney(MyAccountName, (Money)firstPeerAssessment);
            ThirdAccount.SetMoney(MyAccountName, (Money)secondPeerAssessment);
            MyAccount.Self.Money = (Money)myMoneyBefore;
            MyAccount.SetTrust(OtherAccountName, (Weight)1);
            MyAccount.SetTrust(ThirdAccountName, (Weight)1);

            MyAccount.SyncAll();

            Assert.Equal(myMoneyAfter, MyAccount.Self.Money);
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0)]
        [InlineData(10, 10, 20, 20, 15, 15)]
        [InlineData(10, 20, 20, 40, 15, 30)]
        public void Given_1_PeerWithFullTrustAndConnectivity_WhenSyncAll_ThenGetMeanPeerMoney(
            float mySelfMoneyBefore,
            float myPeerMoneyBefore,
            float peerMyMoneyBefore,
            float peerSelfMoneyBefore,
            float mySelfMoneyAfter,
            float myPeerMoneyAfter)
        {
            Interconnect(MyAccount, OtherAccount);
            MyAccount.Self.Money = (Money)mySelfMoneyBefore;
            MyAccount.SetMoney(OtherAccountName, (Money)myPeerMoneyBefore);
            OtherAccount.Self.Money = (Money)peerSelfMoneyBefore;
            OtherAccount.SetMoney(MyAccountName, (Money)peerMyMoneyBefore);
            MyAccount.SetTrust(OtherAccountName, (Weight)1);

            MyAccount.SyncAll();

            Assert.Equal(mySelfMoneyAfter, MyAccount.Self.Money);
            Assert.Equal(myPeerMoneyAfter, MyAccount.GetMoney(OtherAccountName));
        }

        [Theory]
        [InlineData(0, 0.5, 50, 0.5, 100, 25)]
        [InlineData(0, 1, 50, 0.5, 100, 50)]
        [InlineData(0, 0.5, 50, 1, 100, 50)]
        public void Given_2_PeersWithFullConnectivity_WhenSyncSelf_ThenGetMeanWeightedByTrustPeerMoney(
            float myMoneyBefore,
            float firstPeerTrust,
            float firstPeerAssessment,
            float secondPeerTrust,
            float secondPeerAssessment,
            float myMoneyAfter)
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            OtherAccount.SetMoney(MyAccountName, (Money)firstPeerAssessment);
            ThirdAccount.SetMoney(MyAccountName, (Money)secondPeerAssessment);
            MyAccount.Self.Money = (Money)myMoneyBefore;
            MyAccount.SetTrust(OtherAccountName, (Weight)firstPeerTrust);
            MyAccount.SetTrust(ThirdAccountName, (Weight)secondPeerTrust);

            MyAccount.SyncAll();

            Assert.Equal(myMoneyAfter, MyAccount.Self.Money);
        }
    }
}