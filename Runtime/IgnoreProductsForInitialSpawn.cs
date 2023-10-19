using System.Collections.Generic;
using UnityEngine;

namespace CodedInAppModule
{
    [CreateAssetMenu(order = 0, fileName = "IgnoreIconsLoadingContainer",
        menuName = "InAppPurchasingModule/IgnoreIconsLoadingContainer")]
    public class IgnoreProductsForInitialSpawn : ScriptableObject
    {
        [Header("Products that need not spawn")] [SerializeField]
        private List<string> _productsToIgnore;
        public List<string> ProductsToIgnore => _productsToIgnore;
    }
}