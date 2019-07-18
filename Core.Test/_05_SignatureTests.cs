using System;
using System.Security.Cryptography;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Cryptography;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class SignatureTests : TestBase
    {
        private static readonly RsaCryptographyFactory _cryptographyFactory = new RsaCryptographyFactory();
        public readonly ICryptography _cryptography = _cryptographyFactory.CreateCryptography();

        public SignatureTests() : base(_cryptographyFactory) { }

        [Fact]
        public void CreatedAccountHasPublicKey()
        {
            Assert.NotEmpty(MyAccount.PublicKey);
        }

        [Fact]
        public void AgentHasSameKeyAsAccount()
        {
            Assert.Equal(MyAccount.PublicKey, _network.FindAgent(MyId).PublicKey);
        }

        [Fact]
        public void DifferentAccountsHaveDifferentPublicKeys()
        {
            Assert.NotEqual(_network.CreateRootAccount("A", 10).PublicKey, _network.CreateRootAccount("B", 11).PublicKey);
        }

        [Fact]
        public void CanCreateVerifyableSignature()
        {
            var payload = new byte[] { 1, 2, 3};
            var key = RSA.Create();
            var signature = new RsaSignature(payload, key);

            Assert.True(signature.Verify(payload, key.ExportRSAPublicKey()));
        }

        [Fact]
        public void CannotVerifySignatureWithWrongPayload()
        {
            var payload1 = new byte[] { 1, 2, 3 };
            var payload2 = new byte[] { 2, 3, 4 };
            var key = RSA.Create();
            var signature = new RsaSignature(payload1, key);

            Assert.False(signature.Verify(payload2, key.ExportRSAPublicKey()));
        }

        [Fact]
        public void CannotVerifySignatureWithWrongKey()
        {
            var payload = new byte[] { 1, 2, 3 };
            var key = RSA.Create();
            var otherKey = RSA.Create();
            var signature = new RsaSignature(payload, key);

            Assert.False(signature.Verify(payload, otherKey.ExportRSAPublicKey()));
        }

        [Fact]
        public void CanSendSignedActionToPeer()
        {
            Interconnect(MyActor, OtherActor);
            Assert.True(_network.SendAction(OtherId, MyId, MyAccount.Sign(new NoAction())));
        }

        [Fact]
        public void CanRenewAccountKeys()
        {
            var keyBefore = MyAccount.PublicKey;
            MyAccount.RenewKeys();
            var keyAfter = MyAccount.PublicKey;
            Assert.NotEqual(keyBefore, keyAfter);
        }

        [Fact]
        public void WhenRenewKeys_PeersAreUpdated()
        {
            Interconnect(MyActor, OtherActor);
            MyActor.RenewKeys();
            Assert.Equal(MyAccount.PublicKey, OtherAccount.GetPeer(MyId).PublicKey);
        }

        [Fact]
        public void WhenUpdatedWithInvalidSignature_ThrowsInvalidOperationException()
        {
            MyActor.Connect(OtherId);
            var otherAgent = _network.FindAgent(OtherId);
            Assert.Throws<InvalidOperationException>(
                () => _network.SendAction(MyId, OtherId, _cryptography.Sign(new NoAction())));
        }

        [Fact]
        public void WhenUpdatedWithUnconnectedAgent_ReturnsFalse()
        {
            Assert.False(_network.SendAction(MyId, OtherId, _cryptography.Sign(new NoAction())));
        }
    }
}