using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public static class Constants
    {
        public static readonly Weight EndorcementTrustFactor = (Weight)1f;
        public static readonly Weight ArtefactEndorcementTrustFactor = (Weight)0.01f;
        public static readonly Weight MakeCounterfeitArtefactDistrustFactor = (Weight)0.5f;
        public static readonly Weight HoldCounterfeitArtefactDistrustFactor = (Weight)0.1f;
        public static readonly Weight EndorceCounterfeitArtefactDistrustFactor = (Weight)0.1f;
        public static readonly Weight DestroyOthersArtefactDistrustFactor = (Weight)0.2f;

        public const float ArtefactMoneyFactor = 0.01f;
    }
}
