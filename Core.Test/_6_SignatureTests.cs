using Xunit;

namespace Trustcoin.Core.Test
{
    public class SignatureTests : TestBase
    {
        [Fact]
        public void CreatedAccountHasPublicKey()
        {
            Assert.NotEmpty(MyAccount.PublicKey);
        }

        [Fact]
        public void AgentHasSameKeyAsAccount()
        {
            Assert.Equal(MyAccount.PublicKey, _network.FindAgent(MyAccountName).PublicKey);
        }

        [Fact]
        public void DifferentAccountsHaveDifferentPublicKeys()
        {
            Assert.NotEqual(MyAccount.PublicKey, OtherAccount.PublicKey);
        }

        [Fact]
        public void CanCreateValidSignature()
        {
            var payload = "xyz";
            var key = "abc";
            var signature = new SimpleSignature(payload, key);

            Assert.True(signature.Verify(payload, key));
        }

        [Fact]
        public void CannotVerifySignatureWithWrongPayload()
        {
            var payload = "xyz";
            var key = "abc";
            var signature = new SimpleSignature(payload, key);

            Assert.False(signature.Verify("zzz", key));
        }

        [Fact]
        public void CannotVerifySignatureWithWrongKey()
        {
            var payload = "xyz";
            var key = "abc";
            var signature = new SimpleSignature(payload, key);

            Assert.False(signature.Verify(payload, "zzz"));
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
            MyAccount.Connect(OtherAccountName);
            OtherAccount.Connect(MyAccountName);

            MyAccount.RenewKeys();
            Assert.Equal(MyAccount.PublicKey, OtherAccount.GetPeer(MyAccountName).PublicKey);
        }
    }
}