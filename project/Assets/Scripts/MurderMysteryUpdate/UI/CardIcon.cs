using System;
using CardGame.GameData.Cards;
using CardGame.Textures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MurderMystery
{
    public class CardIcon : MonoBehaviour
    {
        public Image icon;
        public TMP_Text name;
        public Button button;

        private Action<BaseCard> _onclicked;
        private BaseCard _card;

        public void InitIcon(BaseCard card, Action<BaseCard> onComplete)
        {
            var texture  = TextureCollectionReader.Readers["Avatars"].Textures[card.Avatar].GetPreview();
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            _card = card;
            _onclicked = onComplete;
            name.text = card.Name;
            icon.sprite = sprite;
            icon.preserveAspect = true;
            button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _onclicked?.Invoke(_card);
        }
    }
}