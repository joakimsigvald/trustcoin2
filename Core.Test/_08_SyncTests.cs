using System.Linq;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;
using Xunit;
using static Trustcoin.Core.Entities.Constants;

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
            MyActor.SyncAll();

            Assert.Equal(money, MyAccount.Self.Money);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 100, 50)]
        public void Given_1_PeerWithFullTrustAndConnectivity_WhenSyncSelf_ThenGetMeanPeerMoney(
            float myMoneyBefore, float peerAssessment, float myMoneyAfter)
        {
            Interconnect(MyActor, OtherActor);
            OtherAccount.SetMoney(MyId, (Money)peerAssessment);
            MyAccount.Self.Money = (Money)myMoneyBefore;
            MyAccount.SetTrust(OtherId, (SignedWeight)1);

            MyActor.SyncAll();

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
            Interconnect(MyActor, OtherActor, ThirdActor);
            OtherAccount.SetMoney(MyId, (Money)firstPeerAssessment);
            ThirdAccount.SetMoney(MyId, (Money)secondPeerAssessment);
            MyAccount.Self.Money = (Money)myMoneyBefore;
            MyAccount.SetTrust(OtherId, (SignedWeight)1);
            MyAccount.SetTrust(ThirdId, (SignedWeight)1);

            MyActor.SyncAll();

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
            Interconnect(MyActor, OtherActor);
            MyAccount.Self.Money = (Money)mySelfMoneyBefore;
            MyAccount.SetMoney(OtherId, (Money)myPeerMoneyBefore);
            OtherAccount.Self.Money = (Money)peerSelfMoneyBefore;
            OtherAccount.SetMoney(MyId, (Money)peerMyMoneyBefore);
            MyAccount.SetTrust(OtherId, (SignedWeight)1);

            MyActor.SyncAll();

            Assert.Equal(mySelfMoneyAfter, MyAccount.Self.Money);
            Assert.Equal(myPeerMoneyAfter, MyAccount.GetMoney(OtherId));
        }

        [Theory]
        [InlineData(0, 0.5, 50, 0.5, 100, 25)]
        [InlineData(0, 1, 50, 0.5, 100, 50)]
        [InlineData(0, 0.5, 50, 1, 100, 50)]
        [InlineData(0, -0.5, 150, 1, 100, 50)]
        public void Given_2_PeersWithFullConnectivity_WhenSyncSelf_ThenGetMeanWeightedByTrustPeerMoney(
            float myMoneyBefore,
            float firstPeerTrust,
            float firstPeerAssessment,
            float secondPeerTrust,
            float secondPeerAssessment,
            float myMoneyAfter)
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            OtherAccount.SetMoney(MyId, (Money)firstPeerAssessment);
            ThirdAccount.SetMoney(MyId, (Money)secondPeerAssessment);
            MyAccount.Self.Money = (Money)myMoneyBefore;
            MyAccount.SetTrust(OtherId, (SignedWeight)firstPeerTrust);
            MyAccount.SetTrust(ThirdId, (SignedWeight)secondPeerTrust);

            MyActor.SyncAll();

            Assert.Equal(myMoneyAfter, MyAccount.Self.Money);
        }

        [Theory]
        [InlineData(0, new[] { 1f }, new[] { 0 }, 0)]
        [InlineData(0, new[] { 1f }, new[] { 100 }, 50)]
        [InlineData(0, new[] { 1f, 1f }, new[] { 100, 100 }, 100)]
        [InlineData(0, new[] { 1f, 1f }, new[] { 90, -1 }, 30)]
        [InlineData(0, new[] { 1f, 0.5f }, new[] { 90, -1 }, 36)]
        [InlineData(0, new[] { 0.5f, 1f, 0.5f }, new[] { 90, -1, 100 }, 30)]
        [InlineData(30, new[] { 0.5f, 1f, 0.5f }, new[] { 90, -1, 100 }, 50)]
        [InlineData(0, new[] { 0.5f, 1f, 0.5f, -0.5f, -0.5f, -0.5f, -0.5f }, new[] { 90, -1, 100, 200, 200, 200, 200 }, 30)]
        [InlineData(0, new[] { 0.5f, 1f, 0.5f, -0.5f, -0.5f, -0.5f, -0.5f }, new[] { 90, -1, 100, 10, 10, 10, 10 }, 30)]
        public void WhenSyncPeer_ThenUpdateFromConnectedPeersAndWeighBySumOfTheirTrust(
            float peerMoneyBefore,
            float[] peerTrusts,
            int[] peerAssessments,
            float peerMoneyAfter)
        {
            var peers = peerTrusts
                .Select((pt, i) => _network
                .CreateRootAccount($"Peer{i}", (byte)(10 + i))
                .GetActor(_network, new TransactionFactory()))
                .ToArray();
            int i = 0;
            var peerToUpdate = peers[0];
            foreach (var peer in peers)
            {
                Interconnect(MyActor, peer);
                MyAccount.SetTrust(peer.Account.Id, (SignedWeight)peerTrusts[i]);
                var peerAssessment = peerAssessments[i];
                if (peerAssessment >= 0)
                {
                    Interconnect(peer, peerToUpdate);
                    peer.Account.SetMoney(peerToUpdate.Account.Id, (Money)peerAssessments[i]);
                }
                i++;
            }
            MyAccount.SetMoney(peerToUpdate.Account.Id, (Money)peerMoneyBefore);

            MyActor.SyncAll();

            Assert.Equal(peerMoneyAfter, MyAccount.GetMoney(peerToUpdate.Account.Id));
        }

        [Fact]
        public void WhenConnectPeer_PeerIsSynced()
        {
            Interconnect(MyActor, OtherActor);
            Interconnect(ThirdActor, OtherActor);
            OtherAccount.SetMoney(ThirdId, (Money)100);
            MyAccount.SetTrust(OtherId, SignedWeight.Max);

            MyActor.Connect(ThirdId);

            Assert.Equal((Money)50, MyAccount.GetMoney(ThirdId));
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 0.1)]
        public void GivenMajorityOfPeersBelieveKnownArtefactHasDifferentOwner_WhenSync_ArtefactOwnerIsChanged(float otherTrust, float thirdTrust)
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var artefact = ThirdActor.CreateArtefact(Artefact.Name);
            MyAccount.SetTrust(OtherId, (SignedWeight)otherTrust);
            MyAccount.SetTrust(ThirdId, (SignedWeight)thirdTrust);
            MyAccount.MoveArtefact(artefact, MyId);

            MyActor.SyncAll();

            Assert.False(MyAccount.Self.HasArtefact(artefact.Id));
            Assert.Equal(ThirdId, MyAccount.GetArtefact(artefact.Id).OwnerId);
            Assert.Equal((SignedWeight)1, MyAccount.Self.Trust);
        }

        [Theory]
        [InlineData(0.5, 0.5)]
        [InlineData(1, 0)]
        public void GivenMinorityOfPeersBelieveKnownArtefactHasDifferentOwner_WhenSync_ArtefactOwnerIsNotChanged(float otherTrust, float thirdTrust)
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var artefact = ThirdActor.CreateArtefact(Artefact.Name);
            MyAccount.SetTrust(OtherId, (SignedWeight)otherTrust);
            MyAccount.SetTrust(ThirdId, (SignedWeight)thirdTrust);
            MyAccount.MoveArtefact(artefact, MyId);

            MyActor.SyncAll();

            Assert.True(MyAccount.Self.HasArtefact(artefact.Id));
            Assert.Equal(MyId, MyAccount.GetArtefact(artefact.Id).OwnerId);
        }

        [Theory]
        [InlineData(1)]
        public void GivenMinorityOfPeersBelieveKnownArtefactHasDifferentOwner_WhenSync_TheyLooseTrust(float trustValueBefore)
        {
            var trustBefore = (SignedWeight)trustValueBefore;
            Interconnect(MyActor, OtherActor);
            var artefact = OtherActor.CreateArtefact(Artefact.Name);
            MyAccount.SetTrust(OtherId, trustBefore);
            MyAccount.MoveArtefact(artefact, MyId);

            MyActor.SyncAll();

            var expectedTrust = trustBefore.Decrease(HoldCounterfeitArtefactDistrustFactor);
            Assert.Equal(expectedTrust, MyAccount.GetTrust(OtherId));
        }

        [Fact]
        public void WhenSync_LearnAboutUnknownArtefactsPossessedByPeers()
        {
            Interconnect(OtherActor, ThirdActor);
            var artefact = ThirdActor.CreateArtefact(Artefact.Name);
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherId, (SignedWeight)1);
            MyAccount.SetTrust(ThirdId, (SignedWeight)1);

            MyActor.SyncAll();

            Assert.True(MyAccount.KnowsArtefact(artefact.Id));
        }

        [Fact]
        public void WhenSyncWithAgentThatMyPeersDontKnow_TheyAskNearestFriends()
        {
            var fullTrust = SignedWeight.Max;

            var a = _network.CreateActor(_network.CreateRootAccount("a", 11));
            var b = _network.CreateActor(_network.CreateRootAccount("b", 12));
            var c = _network.CreateActor(_network.CreateRootAccount("c", 13));
            var d = _network.CreateActor(_network.CreateRootAccount("d", 14));
            var e = _network.CreateActor(_network.CreateRootAccount("e", 15));
            var f = _network.CreateActor(_network.CreateRootAccount("f", 16));

            Interconnect(fullTrust, a, b, c);
            Interconnect(fullTrust, d, e, f);
            Interconnect(fullTrust, b, f);
            Interconnect(fullTrust, c, e);

            a.Account.SetMoney(a.Account.Id, SomeMoney);
            b.Account.SetMoney(a.Account.Id, SomeMoney);
            c.Account.SetMoney(a.Account.Id, SomeMoney);

            Interconnect(fullTrust, a, d);

            Assert.Equal(SomeMoney, d.Account.GetMoney(a.Account.Id));
        }

        [Fact]
        public void WhenSyncWithAgentThatMyPeersDontKnow_TheyAskNearestFriend_Cascading()
        {
            var fullTrust = SignedWeight.Max;

            var a = _network.CreateActor(_network.CreateRootAccount("a", 11));
            var b = _network.CreateActor(_network.CreateRootAccount("b", 12));
            var c = _network.CreateActor(_network.CreateRootAccount("c", 13));
            var d = _network.CreateActor(_network.CreateRootAccount("d", 14));
            var e = _network.CreateActor(_network.CreateRootAccount("e", 15));
            var f = _network.CreateActor(_network.CreateRootAccount("f", 16));
            var g = _network.CreateActor(c.CreateAccount("g"));

            Interconnect(fullTrust, a, b, c);
            Interconnect(fullTrust, d, e, f);
            Interconnect(fullTrust, b, f);
            Interconnect(fullTrust, c, g);
            Interconnect(fullTrust, g, e);

            a.Account.SetMoney(a.Account.Id, SomeMoney);
            b.Account.SetMoney(a.Account.Id, SomeMoney);
            c.Account.SetMoney(a.Account.Id, SomeMoney);

            Interconnect(fullTrust, a, d);

            Assert.Equal(SomeMoney, d.Account.GetMoney(a.Account.Id));
        }

        [Fact]
        public void WhenSyncWithAgentThatMyPeersDontKnow_TheyAskNearestFriends_Cascading()
        {
            var fullTrust = SignedWeight.Max;

            var a = _network.CreateActor(_network.CreateRootAccount("a", 11));
            var b = _network.CreateActor(_network.CreateRootAccount("b", 12));
            var c = _network.CreateActor(_network.CreateRootAccount("c", 13));
            var d = _network.CreateActor(_network.CreateRootAccount("d", 14));
            var e = _network.CreateActor(_network.CreateRootAccount("e", 15));
            var f = _network.CreateActor(_network.CreateRootAccount("f", 16));
            var g = _network.CreateActor(c.CreateAccount("g"));
            var h = _network.CreateActor(a.CreateAccount("h"));

            Interconnect(fullTrust, a, b, c, h);
            Interconnect(fullTrust, d, e, f);
            Interconnect(fullTrust, b, f);
            Interconnect(fullTrust, c, g, h);
            Interconnect(fullTrust, e, g);

            a.Account.SetMoney(a.Account.Id, SomeMoney);
            b.Account.SetMoney(a.Account.Id, SomeMoney);
            c.Account.SetMoney(a.Account.Id, 0.8f * SomeMoney);
            h.Account.SetMoney(a.Account.Id, 0.4f * SomeMoney);

            Interconnect(fullTrust, a, d);

            Assert.Equal(0.8f * SomeMoney, d.Account.GetMoney(a.Account.Id), 5);
        }
    }
}