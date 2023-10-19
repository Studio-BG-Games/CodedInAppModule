using System;
using CodedInAppModule;
using UnityEngine;
using UnityEngine.Purchasing;

namespace CodedInAppModule.Sample
{
    public class ShrekProduct : UIProduct
    {
        [SerializeField] private InAppPurchasingManager _inAppPurchasingManager;

        private void Awake()
        {
            _inAppPurchasingManager.OnInitializedInApp += OnOnInitializedInApp;
        }

        private void OnOnInitializedInApp()
        {
            InitShrek();
        }

        private void InitShrek()
        {
            Product shrekProduct = _inAppPurchasingManager.GetProductByID("shrek");

            if (shrekProduct == null)
            {
                throw new Exception("Shrek not found exception");
            }
            else
            {
                Setup(shrekProduct, OnPurchase);
            }
        }

        private void OnPurchase(Product model, Action oncomplete, Action onPurchaseFailed)
        {
            _inAppPurchasingManager.HandlePurchase(
                model,
                onHandlePurchase: () => Debug.Log("Handle purchase"),
                oncomplete,
                onFailed: onPurchaseFailed);
        }
    }
}