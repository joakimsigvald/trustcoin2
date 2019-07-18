using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test.Types
{
    public class ArtefactIdTests
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
    }
}
