using Trustcoin.Core.Entities;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class SearchTests : TestBase
    {
        private readonly IAccount a;
        private readonly IAccount b;
        private readonly IAccount c;
        private readonly IAccount aa;
        private readonly IAccount ab;
        private readonly IAccount ac;
        private readonly IAccount ba;
        private readonly IAccount bb;
        private readonly IAccount bc;
        private readonly IAccount ca;
        private readonly IAccount cb;
        private readonly IAccount cc;
        private readonly ILookupService _lookupService;

        public SearchTests( )
        {
            a = MyActor.CreateAccount("A");
            b = MyActor.CreateAccount("B");
            c = MyActor.CreateAccount("C");
            aa = CreateAccount(a, "AA");
            ab = CreateAccount(a, "AB");
            ac = CreateAccount(a, "AC");
            ba = CreateAccount(b, "BA");
            bb = CreateAccount(b, "BB");
            bc = CreateAccount(b, "BC");
            ca = CreateAccount(c, "CA");
            cb = CreateAccount(c, "CB");
            cc = CreateAccount(c, "CC");
            _lookupService = _network.GetLookupService();
        }

        [Fact]
        public void CanFindAgentById()
        {
            Assert.Same(a.Self, _lookupService.Find((AgentId)"1.1"));
            Assert.Same(b.Self, _lookupService.Find((AgentId)"1.2"));
            Assert.Same(c.Self, _lookupService.Find((AgentId)"1.3"));
            Assert.Same(aa.Self, _lookupService.Find((AgentId)"1.1.1"));
            Assert.Same(ab.Self, _lookupService.Find((AgentId)"1.1.2"));
            Assert.Same(ac.Self, _lookupService.Find((AgentId)"1.1.3"));
            Assert.Same(ba.Self, _lookupService.Find((AgentId)"1.2.1"));
            Assert.Same(bb.Self, _lookupService.Find((AgentId)"1.2.2"));
            Assert.Same(bc.Self, _lookupService.Find((AgentId)"1.2.3"));
            Assert.Same(ca.Self, _lookupService.Find((AgentId)"1.3.1"));
            Assert.Same(cb.Self, _lookupService.Find((AgentId)"1.3.2"));
            Assert.Same(cc.Self, _lookupService.Find((AgentId)"1.3.3"));
        }

        [Fact]
        public void CanFindAgentByName()
        {
            Assert.Same(a.Self, _lookupService.Find("A"));
            Assert.Same(b.Self, _lookupService.Find("B"));
            Assert.Same(c.Self, _lookupService.Find("C"));
            Assert.Same(aa.Self, _lookupService.Find("AA"));
            Assert.Same(ab.Self, _lookupService.Find("AB"));
            Assert.Same(ac.Self, _lookupService.Find("AC"));
            Assert.Same(ba.Self, _lookupService.Find("BA"));
            Assert.Same(bb.Self, _lookupService.Find("BB"));
            Assert.Same(bc.Self, _lookupService.Find("BC"));
            Assert.Same(ca.Self, _lookupService.Find("CA"));
            Assert.Same(cb.Self, _lookupService.Find("CB"));
            Assert.Same(cc.Self, _lookupService.Find("CC"));
        }

        private IAccount CreateAccount(IAccount parent, string name) 
            => GetActor(parent).CreateAccount(name);

        private IActor GetActor(IAccount account)
            => new Actor(_network, account, _transactionFactory);
    }
}