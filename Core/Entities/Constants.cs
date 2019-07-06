using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public static class Constants
    {
        public static readonly SignedWeight BaseTrust = (SignedWeight)0f;
        public static readonly Weight EndorcementFactor = (Weight)1f;
        public static readonly Weight BaseRelationWeight = (Weight)0.1f;
        public static readonly Weight DoubleEndorceFactor = (Weight)0.1f;
        public static readonly Weight CounterfeitArtefactFactor = (Weight)0.5f;
        public static readonly Weight DestroyOthersArtefactFactor = (Weight)0.2f;
    }
}
