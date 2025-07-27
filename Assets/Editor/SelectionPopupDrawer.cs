#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using EditorExtention;

[CustomPropertyDrawer(typeof(SelectionPopupAttribute))]
public class SelectionPopupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (SelectionPopupAttribute)attribute;
        object target = property.serializedObject.targetObject;
        Type targetType = target.GetType();

        // Главная замена: теперь получаем SelectedItem[]
        var items = ResolveSelectedItems(target, attr.ValueSourceName);

        if (items == null || items.Length == 0)
        {
            EditorGUI.LabelField(position, label.text, "<Нет доступных значений>");
            return;
        }

        // Найдём текущий выбранный по Name
        int currentIndex = Array.FindIndex(items, itm => itm.Name == property.stringValue);

        if (!string.IsNullOrEmpty(attr.Placeholder))
        {
            // Отдельно рисуем label и кнопку с placeholder

            var labelWidth = EditorGUIUtility.labelWidth;
            var labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            var buttonRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

            EditorGUI.LabelField(labelRect, label);

            if (GUI.Button(buttonRect, attr.Placeholder, EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();

                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];

                    if (!item.IsActive)
                    {
                        menu.AddDisabledItem(new GUIContent(item.DisplayName));
                        continue;
                    }

                    menu.AddItem(new GUIContent(item.DisplayName), item.IsSelected, () =>
                    {
                        property.serializedObject.Update();
                        property.stringValue = item.Name;
                        property.serializedObject.ApplyModifiedProperties();
                        CallCallback(attr, targetType, target, property.stringValue);
                    });
                }

                menu.DropDown(buttonRect);
            }
        }
        else
        {
            // Обычное поведение — EditorGUI.Popup. Преобразуем SelectedItem[] в массив отображаемых имён.
            string[] displayNames = items.Select(it => it.DisplayName).ToArray();

            // Find index of current
            int curIndex = Mathf.Max(0, Array.FindIndex(items, itm => itm.Name == property.stringValue));

            // Только активные элементы могут быть выбраны
            int[] activeIndices = items.Select((it, idx) => new { it, idx })
                                       .Where(x => x.it.IsActive)
                                       .Select(x => x.idx)
                                       .ToArray();

            int newIndex = EditorGUI.Popup(position, label.text, curIndex, displayNames);

            // Если выбранный элемент неактивен — отмена
            if (newIndex != curIndex && activeIndices.Contains(newIndex))
            {
                property.stringValue = items[newIndex].Name;
                CallCallback(attr, targetType, target, property.stringValue);
            }
        }
    }

    private void CallCallback(SelectionPopupAttribute attr, Type targetType, object target, string value)
    {
        if (!string.IsNullOrEmpty(attr.CallbackName))
        {
            var method = targetType.GetMethod(attr.CallbackName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                var param = method.GetParameters();
                if (param.Length == 1 && param[0].ParameterType == typeof(string))
                    method.Invoke(target, new object[] { value });
                else
                    method.Invoke(target, null);
            }
            else
            {
                Debug.LogWarning($"SelectionPopup callback '{attr.CallbackName}' not found on {targetType.Name}");
            }
        }
    }

    /// <summary>
    /// Получает SelectedItem[] через свойство/поле/метод источника по имени.
    /// </summary>
    private SelectItem[] ResolveSelectedItems(object target, string sourceName)
    {
        Type type = target.GetType();

        // Field
        FieldInfo field = type.GetField(sourceName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (field != null)
        {
            object value = field.GetValue(target);
            if (value is SelectItem[] arr) return arr;
            if (value is IEnumerable<SelectItem> en) return en.ToArray();
        }

        // Property
        PropertyInfo prop = type.GetProperty(sourceName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (prop != null)
        {
            object value = prop.GetValue(target, null);
            if (value is SelectItem[] arr) return arr;
            if (value is IEnumerable<SelectItem> en) return en.ToArray();
        }

        // Method (без параметров)
        MethodInfo method = type.GetMethod(sourceName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (method != null && method.GetParameters().Length == 0)
        {
            object value = method.Invoke(target, null);
            if (value is SelectItem[] arr) return arr;
            if (value is IEnumerable<SelectItem> en) return en.ToArray();
        }

        Debug.LogError($"SelectionPopup: источник '{sourceName}' не найден или не возвращает массив SelectedItem/list.");
        return new SelectItem[0];
    }
}
#endif
