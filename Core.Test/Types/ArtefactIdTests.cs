using Trustcoin.Core.Entities;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test.Types
{
    public class ArtefactIdTests : TestBase
    {
        [Theory]
        [InlineData("1", 1, "1:1")]
        [InlineData("1.2.3", 4, "1.2.3:4")]
        public void CanCreateIdFromAgentId(string agentIdStr, uint artefactNumber, string expectedArtefactId)
        {
            var agentId = (AgentId)agentIdStr;
            var artefactId = agentId / artefactNumber;
            Assert.Equal(expectedArtefactId, artefactId.ToString());
        }

        [Fact]
        public void WhenActorCreateArtefact_IdIsBasedOnAccountIdWithIncrementalNumber()
        {
            var a1 = MyActor.CreateArtefact("abc");
            var a2 = MyActor.CreateArtefact("abc");
            var b1 = OtherActor.CreateArtefact("abc");
            var b2 = OtherActor.CreateArtefact("abc");
            Assert.Equal(MyAccount.Id / 1, a1.Id);
            Assert.Equal(MyAccount.Id / 2, a2.Id);
            Assert.Equal(OtherAccount.Id / 1, b1.Id);
            Assert.Equal(OtherAccount.Id / 2, b2.Id);
        }

        [Fact]
        public void WhenCreateTransientArtefact_NumberIsLessThanIntMax()
        {
            var artefact = MyActor.CreateArtefact("abc");
            Assert.InRange(artefact.Id.Number, (uint)0, (uint)(int.MaxValue - 1));
        }

        [Fact]
        public void WhenCreateResilientArtefact_NumberIsAtLeastIntMax()
        {
            var artefact = MyActor.CreateArtefact("abc", true);
            Assert.InRange(artefact.Id.Number, (uint)int.MaxValue, uint.MaxValue);
        }

        [Fact]
        public void ThrowsWhenCreatingTooManyResilientArtefacts()
        {
            ((Account)MyAccount)._resilientArtefactCount = int.MaxValue - 1;
            MyActor.CreateArtefact("abc", true);
            Assert.Throws<OutOfBounds<uint>>(() => MyActor.CreateArtefact("abc", true));
        }

        [Fact]
        public void WhenCreatingManyTransientArtefacts_IdsAreReused()
        {
            var first = MyActor.CreateArtefact("abc");
            ((Account)MyAccount)._transientArtefactCount = int.MaxValue - 1;
            MyActor.CreateArtefact("abc");
            var last = MyActor.CreateArtefact("abc");

            Assert.Equal(first.Id.Number, last.Id.Number);
        }
    }
}
