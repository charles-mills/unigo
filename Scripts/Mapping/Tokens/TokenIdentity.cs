using UnityEngine;

namespace Gameplay
{
    public enum BrandType
    {
        None,
        Nike,
        Adidas,
        Dominos,
        Microsoft
    }

    public class TokenIdentity : MonoBehaviour
    {
        public static BrandType SelectedBrand = BrandType.None;
        public BrandType brand;
    }
}