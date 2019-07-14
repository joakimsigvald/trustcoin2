using Trustcoin.Core.Entities;
using Trustcoin.Core.Infrastructure;
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
            Assert.Same(a.Self, _lookupService.FindById("1.1"));
            Assert.Same(b.Self, _lookupService.FindById("1.2"));
            Assert.Same(c.Self, _lookupService.FindById("1.3"));
            Assert.Same(aa.Self, _lookupService.FindById("1.1.1"));
            Assert.Same(ab.Self, _lookupService.FindById("1.1.2"));
            Assert.Same(ac.Self, _lookupService.FindById("1.1.3"));
            Assert.Same(ba.Self, _lookupService.FindById("1.2.1"));
            Assert.Same(bb.Self, _lookupService.FindById("1.2.2"));
            Assert.Same(bc.Self, _lookupService.FindById("1.2.3"));
            Assert.Same(ca.Self, _lookupService.FindById("1.3.1"));
            Assert.Same(cb.Self, _lookupService.FindById("1.3.2"));
            Assert.Same(cc.Self, _lookupService.FindById("1.3.3"));
        }

        [Fact]
        public void CanFindAgentByName()
        {
            Assert.Same(a.Self, _lookupService.FindByName("A"));
            Assert.Same(b.Self, _lookupService.FindByName("B"));
            Assert.Same(c.Self, _lookupService.FindByName("C"));
            Assert.Same(aa.Self, _lookupService.FindByName("AA"));
            Assert.Same(ab.Self, _lookupService.FindByName("AB"));
            Assert.Same(ac.Self, _lookupService.FindByName("AC"));
            Assert.Same(ba.Self, _lookupService.FindByName("BA"));
            Assert.Same(bb.Self, _lookupService.FindByName("BB"));
            Assert.Same(bc.Self, _lookupService.FindByName("BC"));
            Assert.Same(ca.Self, _lookupService.FindByName("CA"));
            Assert.Same(cb.Self, _lookupService.FindByName("CB"));
            Assert.Same(cc.Self, _lookupService.FindByName("CC"));
        }

        [Theory]
        [InlineData("1", "1", 0)]
        [InlineData("1", "2", 1)]
        [InlineData("1", "3", 2)]
        [InlineData("1", "1.1", 1)]
        [InlineData("1.2", "1", 1)]
        [InlineData("1", "1.2.3", 2)]
        [InlineData("1.1.2", "1.2.2", 3)]
        [InlineData("2", "1.3.3", 3)]
        [InlineData("1.1.1", "1.3.3", 4)]
        public void CanGetDistance(string sourceId, string targetId, int expectedDistance)
        {
            var source = _lookupService.FindById(sourceId);
            var target = _lookupService.FindById(targetId);

            Assert.Equal(expectedDistance, source.GetDistance(target));
        }

        private IAccount CreateAccount(IAccount parent, string name) 
            => GetActor(parent).CreateAccount(name);

        private IActor GetActor(IAccount account)
            => new Actor(_network, account, _transactionFactory);
    }
}