using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZenUI
{
    public class ButtonUI : MonoBehaviour
    {
        [field:SerializeField] public Button Button {get;private set;}
        [field:SerializeField] public Image Image {get;private set;}
        [field:SerializeField] public TextMeshProUGUI Text {get;private set;}

        public void SetShader(Material material)
        {
            Image.material = material;
        }

        private void OnValidate()
        {
            if(!Button)
                {
                Button = GetComponent<Button>();
                }
            
            if(!Image)
                {
                Image = GetComponent<Image>();
                }
            
            if(!Text)
                {
                Text = GetComponentInChildren<TextMeshProUGUI>();
                }
        }
    }
}
