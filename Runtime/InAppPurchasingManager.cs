using System;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

namespace CodedInAppModule
{
    public class InAppPurchasingManager : MonoBehaviour, IStoreListener
    {
        [Header("Scriptable Object that has info about products that need to be ignored")]
        [SerializeField] private IgnoreProductsForInitialSpawn _ignoreProductsForInitialSpawn;
        [Header("Loading overlay that use at purchase operations")]
        [SerializeField] private GameObject _loadingOverlay;
        private List<string> _productsToIgnoreInitialSpawn = new List<string>();
        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
        public event Action OnPurchaseComplete;
        public event Action OnPurchaseFail;
        public event Action OnInitializedInApp;

        private async void Awake()
        {
            _productsToIgnoreInitialSpawn = _ignoreProductsForInitialSpawn.ProductsToIgnore;
            InitializationOptions initializationOptions = new InitializationOptions()
#if UNITY_EDITOR
                .SetEnvironmentName("test");
#else
                .SetEnvironmentName("production");
#endif
            await UnityServices.InitializeAsync(initializationOptions);
            ResourceRequest request = Resources.LoadAsync<TextAsset>("IAPProductCatalog");
            request.completed += HandleIAPLoaded;
        }

        private void HandleIAPLoaded(AsyncOperation asyncOperation)
        {
            ResourceRequest request = asyncOperation as ResourceRequest;
            string json = (request.asset as TextAsset)?.text;
            ProductCatalog productCatalog = JsonUtility.FromJson<ProductCatalog>(json);
#if UNITY_ANDROID
            ConfigurationBuilder configurationBuilder = ConfigurationBuilder.Instance(
                StandardPurchasingModule.Instance(AppStore.GooglePlay));
#elif UNITY_IOS
             ConfigurationBuilder configurationBuilder = ConfigurationBuilder.Instance(
                StandardPurchasingModule.Instance(AppStore.AppleAppStore));
#else
            StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
            StandardPurchasingModule.Instance().useFakeStoreAlways = true;
            ConfigurationBuilder configurationBuilder = ConfigurationBuilder.Instance(
                StandardPurchasingModule.Instance(AppStore.NotSpecified));
#endif
            foreach (var item in productCatalog.allProducts)
            {
                configurationBuilder.AddProduct(item.id, item.type);
            }

            UnityPurchasing.Initialize(this, configurationBuilder);
        }

        public List<Product> GetNonIgnoredProducts()
        {
            List<Product> products = new List<Product>();
            foreach (Product product in _storeController.products.all)
            {
                if (CheckForNonIgnoredProduct(product.definition.id))
                {
                    products.Add(product);
                }
            }

            return products;
        }

        public Product GetProductByID(string id)
        {
            foreach (Product product in _storeController.products.all)
            {
                if (product.definition.id == id)
                {
                    return product;
                }
            }

            Debug.LogWarning($"Product with name {id} was not found");
            return null;
        }

        private bool CheckForNonIgnoredProduct(string id)
        {
            foreach (string ignoreProduct in _productsToIgnoreInitialSpawn)
            {
                if (id == ignoreProduct)
                {
                    return false;
                }
            }

            return true;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"Initializing is failed with error: {error}.");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"Initializing is failed with error: {error}.\n Message: {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            _loadingOverlay.SetActive(false);
            OnPurchaseComplete?.Invoke();
            OnPurchaseComplete = null;
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogWarning($"Failed to purchase {product.definition.id} because {failureReason}");
            _loadingOverlay.SetActive(false);
            OnPurchaseFail?.Invoke();
            OnPurchaseFail = null;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensionProvider = extensions;
            InAppIconProvider.LoadProductIcons(controller.products);
            InAppIconProvider.SetOnLoadIconsCompleteAction(() =>
            {
                OnInitializedInApp?.Invoke();
                OnInitializedInApp = null;
            });
        }

        public void HandlePurchase(Product product, Action onHandlePurchase, Action onComplete, Action onFailed)
        {
            _loadingOverlay.SetActive(true);
            onHandlePurchase?.Invoke();
            OnPurchaseComplete = onComplete;
            OnPurchaseFail = onFailed;
            _storeController.InitiatePurchase(product);
        }
    }
}