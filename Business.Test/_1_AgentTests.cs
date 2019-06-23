using Xunit;

namespace Business.Test
{
    public class AgentTests : NetworkTestBase
    {
        [Fact]
        public void CanFindAgentByName()
        {
            var agent = _network.FindAgent(AccountName);
            Assert.NotNull(agent);
            Assert.Equal(AccountName, agent.Name);
        }

        [Fact]
        public void GivenAccountDoesntExist_WhenFindAgent_GetNull()
        {
            Assert.Null(_network.FindAgent("XXX"));
        }
    }
}