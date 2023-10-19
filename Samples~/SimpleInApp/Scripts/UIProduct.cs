using System;
using CodedInAppModule;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace CodedInAppModule.Sample
{
    [RequireComponent(typeof(Button))]
    public class UIProduct : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _icon;
        private Button _button;

        public delegate void PurchaseEvent(Product model, Action onComplete, Action onFailed);

        private event PurchaseEvent OnPurchase;
        private Product _model;

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Purchase);
        }


        public void Setup(Product product, PurchaseEvent purchaseEvent)
        {
            OnPurchase = purchaseEvent;
            _model = product;
            _nameText.text = product.metadata.localizedTitle;
            // _descriptionText.text = product.metadata.localizedDescription;
            _priceText.text = product.metadata.localizedPriceString;
            Texture2D texture = InAppIconProvider.GetIcon(product.definition.id);
            if (texture != null)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2f);
                _icon.sprite = sprite;
            }
            else
            {
                Debug.LogError($"No sprite found for {product.definition.id}!");
            }
        }

        private void Purchase()
        {
            _button.enabled = false;
            OnPurchase?.Invoke(_model, HandlePurchaseComplete, HandlePurchaseFailed);
        }

        private void HandlePurchaseComplete()
        {
            _button.enabled = true;
        }

        private void HandlePurchaseFailed()
        {
            _button.enabled = true;
        }
    }
}