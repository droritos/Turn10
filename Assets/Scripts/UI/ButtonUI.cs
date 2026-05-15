using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZenUI
{
    public class ButtonUI : MonoBehaviour
    {
        [field:SerializeField] public Button Button {get;protected set;}
        [field:SerializeField] public Image Image {get;protected set;}
        [field:SerializeField] public TextMeshProUGUI Text {get;protected set;}

        public void SetShader(Material material)
        {
            Image.material = material;
        }

        protected virtual void OnValidate()
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
