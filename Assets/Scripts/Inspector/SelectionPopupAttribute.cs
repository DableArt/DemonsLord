using System;
using UnityEngine;

namespace EditorExtention
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SelectionPopupAttribute : PropertyAttribute
    {
        public string ValueSourceName { get; }
        public string DisplaySourceName { get; }
        public string CallbackName { get; }
        public string Placeholder { get; }

        public SelectionPopupAttribute(
            string valueSourceName,
            string displaySourceName = null,
            string callbackName = null,
            string placeholder = null)
        {
            ValueSourceName = valueSourceName;
            DisplaySourceName = displaySourceName;
            CallbackName = callbackName;
            Placeholder = placeholder;
        }
    }

    public readonly struct SelectItem
    {
        public readonly string Name;
        public readonly string DisplayName;
        public readonly bool IsSelected;
        public readonly bool IsActive;

        public SelectItem(string name, string displayName, bool isSelected, bool isActive)
        {
            Name = name;
            DisplayName = displayName;
            IsSelected = isSelected;
            IsActive = isActive;
        }
    }
}