using System;
using System.Collections.Generic;
using CodedInAppModule;
using UnityEngine;
using UnityEngine.Purchasing;

namespace CodedInAppModule.Sample
{
    public class UIProductSpawner : MonoBehaviour
    {
        [SerializeField] private InAppPurchasingManager _purchasingManager;
        [SerializeField] private UIProduct _uiProductPrefab;
        [SerializeField] private RectTransform _contentToSpawn;

        private void Awake()
        {
            _purchasingManager.OnInitializedInApp += OnOnInitializedInApp;
        }

        private void OnOnInitializedInApp()
        {
            SpawnUIProducts();
        }

        private void SpawnUIProducts()
        {
            foreach (Product product in _purchasingManager.GetNonIgnoredProducts())
            {
                UIProduct uiProduct = Instantiate(_uiProductPrefab, _contentToSpawn);
                uiProduct.Setup(product, PurchaseEvent);
            }
        }

        private void PurchaseEvent(Product model, Action oncomplete, Action onPurchaseFailed)
        {
            _purchasingManager.HandlePurchase(
                model,
                onHandlePurchase: () => Debug.Log("Handle purchase"),
                oncomplete,
                onFailed: onPurchaseFailed);
        }
    }
}