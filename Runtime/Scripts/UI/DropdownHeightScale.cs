using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownHeightScale : MonoBehaviour
{
    public bool isWrapper = true;
    [SerializeField]
    public RectTransform dropdown;
    private RectTransform self;
    [SerializeField]
    private TMP_Dropdown tmpDropdown;

    public enum WrapperType
    {
        wrapper, dropdownScroller
    };

    public WrapperType type = WrapperType.wrapper;


    void Start()
    {
        self = GetComponent<RectTransform>();

        if (type == WrapperType.wrapper)
        {
            var size = tmpDropdown.options.Count > 0 ? dropdown.sizeDelta.y * tmpDropdown.options.Count : 200;
            self.sizeDelta = new Vector2(self.sizeDelta.x, size);
        }
        else if (type == WrapperType.dropdownScroller)
        {
            float size;
            if (tmpDropdown.options.Count > 0)
            {
                size = dropdown.sizeDelta.y * tmpDropdown.options.Count;
                if (tmpDropdown.options.Count > 3)
                {
                    size = dropdown.sizeDelta.y * 3;
                }
            }
            else
            {
                size = 200;
            }
            self.sizeDelta = new Vector2(self.sizeDelta.x, size);
        }
    }

}

