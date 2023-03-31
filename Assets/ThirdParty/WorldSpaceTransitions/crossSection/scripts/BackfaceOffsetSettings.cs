using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace WorldSpaceTransitions
{
    public class BackfaceOffsetSettings : MonoBehaviour
    {

        public Slider my_slider;
        public InputField my_field;
        public Text MinTxt;
        public Text MaxTxt;
        public float backfaceOffset = 0f;
        public Material[] materials;

        void Start()
        {
            if (backfaceOffset > 0) Shader.SetGlobalFloat("_BackfaceExtrusion", backfaceOffset);
            if (my_slider) my_slider.onValueChanged.AddListener(delegate { UpdateValueFromFloat(my_slider.value); });
            if (my_field)
            {
                my_field.text = backfaceOffset.ToString();
                my_field.onValueChanged.AddListener(delegate { UpdateValueFromString(my_field.text); }); 
            }
            MinTxt.text = my_slider.minValue.ToString("0.00");
            MaxTxt.text = my_slider.maxValue.ToString("0.00");
            //UpdateValueFromFloat(my_slider.value);
            //my_slider.value = Shader.GetGlobalFloat("_BackfaceExtrusion");

        }

        void UpdateValueFromFloat(float value)
        {
            if (my_slider) { my_slider.value = value; }
            if (my_field) { my_field.text = value.ToString("0.00000"); }
            UpdateValue(value);
        }

        void UpdateValueFromString(string value)
        {
            if (my_slider) { my_slider.value = float.Parse(value); }
            if (my_field) { my_field.text = value; }
            UpdateValue(float.Parse(value));
        }

        void UpdateValue(float value)
        {
            Shader.SetGlobalFloat("_BackfaceExtrusion", value);
            foreach (Material m in materials) m.SetFloat("_BackfaceExtrusion", value);
        }

        void OnEnable()
        {
            Shader.SetGlobalFloat("_BackfaceExtrusion", my_slider.value);
        }

        void OnDisable()
        {
            Shader.SetGlobalFloat("_BackfaceExtrusion", 0);
        }

    }
}
