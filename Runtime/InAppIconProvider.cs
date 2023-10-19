using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace CodedInAppModule
{
    public static class InAppIconProvider
    {
        public static Dictionary<string, Texture2D> ProductIcons { get; } = new Dictionary<string, Texture2D>();
        private static int _targetIconsCount;
        public static bool Initialized;
        private static event Action OnLoadIconsComplete;


        /// <summary>
        /// Change this parameter, if your resources path has another folders like Resources/Icons/*.png.
        /// If your path like Resources/*.png, don`t change this parameter
        /// </summary>
        private const string ResourcesPath = "InAppIcons/";

        public static void LoadProductIcons(ProductCollection productCollection)
        {
            if (ProductIcons.Count == 0)
            {
                Debug.Log($"Loading product icons: {productCollection.all.Length} products to load");
                _targetIconsCount = productCollection.all.Length;
                foreach (Product product in productCollection.all)
                {
                    ResourceRequest resourceRequest =
                        Resources.LoadAsync<Texture2D>(ResourcesPath + product.definition.id);
                    resourceRequest.completed += HandleIconLoading;
                }
            }
            else
            {
                Debug.Log("All icons were loaded");
                OnLoadIconsComplete?.Invoke();
                Initialized = true;
            }
        }

        public static Texture2D GetIcon(string id)
        {
            if (ProductIcons.Count == 0)
            {
                Debug.LogError(
                    "Called StoreIconProvider.GetIcon before initializing! This is not a supported operation!");
                throw new InvalidOperationException(
                    "StoreIconProvider.GetIcon() cannot be called before calling StoreIconProvider.LoadProductItems(ProductCollection productCollection)");
            }

            if (ProductIcons.TryGetValue(id, out Texture2D icon))
            {
                return icon;
            }

            Debug.LogError($"Icon with id: {id} was not founded");
            return null;
        }

        private static void HandleIconLoading(AsyncOperation asyncOperation)
        {
            ResourceRequest resourceRequest = asyncOperation as ResourceRequest;
            if (resourceRequest.isDone)
            {
                ProductIcons.Add(resourceRequest.asset.name, resourceRequest.asset as Texture2D);
                if (ProductIcons.Count == _targetIconsCount)
                {
                    OnLoadIconsComplete?.Invoke();
                    Initialized = true;
                }
            }
            else
            {
                _targetIconsCount--;
            }
        }

        public static void SetOnLoadIconsCompleteAction(Action action)
        {
            OnLoadIconsComplete = action;
        }
    }
}