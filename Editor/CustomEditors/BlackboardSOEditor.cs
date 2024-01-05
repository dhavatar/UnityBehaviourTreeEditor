using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheKiwiCoder
{
    [CustomEditor(typeof(BlackboardSO))]
    public class BlackboardSOEditor : Editor
    {
        public VisualTreeAsset m_InspectorXML;
        public VisualTreeAsset m_BlackboardKeyXML;

        private ListView listView;
        private TextField newKeyTextField;
        private PopupField<Type> newKeyTypeField;
        private Button createButton;
        private BlackboardSO context;

        public override VisualElement CreateInspectorGUI()
        {
            context = target as BlackboardSO;
            VisualElement container = new VisualElement();
            
            // Load from default reference
            m_InspectorXML.CloneTree(container);

            listView = container.Q<ListView>("ListView_Keys");
            newKeyTextField = container.Q<TextField>("TextField_KeyName");
            VisualElement popupContainer = container.Q<VisualElement>("PopupField_Placeholder");
            createButton = container.Q<Button>("Button_KeyCreate");

            listView.makeItem = () => m_BlackboardKeyXML.CloneTree();
            listView.bindItem = BindListViewItems;

            newKeyTypeField = new PopupField<Type>();
            newKeyTypeField.label = "Type";
            newKeyTypeField.formatListItemCallback = FormatItem;
            newKeyTypeField.formatSelectedValueCallback = FormatItem;

            var types = TypeCache.GetTypesDerivedFrom<BlackboardKey>();
            foreach (var type in types)
            {
                if (type.IsGenericType)
                {
                    continue;
                }
                newKeyTypeField.choices.Add(type);
            }

            newKeyTypeField.choices.Sort((x,y) => x.Name.CompareTo(y.Name));
            if (newKeyTypeField.value == null)
            {
                newKeyTypeField.value = newKeyTypeField.choices[0];
            }

            popupContainer.Clear();
            popupContainer.Add(newKeyTypeField);

            // TextField
            newKeyTextField.RegisterCallback<ChangeEvent<string>>((evt) => {
                ValidateButton();
            });

            // Button
            createButton.clicked -= CreateNewKey;
            createButton.clicked += CreateNewKey;

            ValidateButton();

            return container;
        }

        private void BindListViewItems(VisualElement element, int index)
        {
            var keyNameLabel = element.Q<Label>("KeyName");
            var keyValueField = element.Q<PropertyField>("KeyValue");
            var renameField = element.Q<TextField>("RenameField");

            var keyNamePropPath = $"blackboard.keys.Array.data[{index}].name";
            var keyValuePropPath = $"blackboard.keys.Array.data[{index}].value";

            keyNameLabel.BindProperty(serializedObject.FindProperty(keyNamePropPath));
            keyNameLabel.AddManipulator(new ContextualMenuManipulator((evt) => {
                evt.menu.AppendAction("Delete", (x) => context.Blackboard.keys.RemoveAt(index), DropdownMenuAction.AlwaysEnabled);
            }));
            keyNameLabel.RegisterCallback<MouseDownEvent>((evt) => {
                if (evt.clickCount == 2)
                {
                    renameField.value = keyNameLabel.text;
                    renameField.style.display = DisplayStyle.Flex;
                    renameField.Focus();

                    keyValueField.style.display = DisplayStyle.None;
                    keyNameLabel.style.display = DisplayStyle.None;
                }
            });

            renameField.BindProperty(serializedObject.FindProperty(keyNamePropPath));
            renameField.RegisterCallback<BlurEvent>((evt) => {
                keyValueField.style.display = DisplayStyle.Flex;
                keyNameLabel.style.display = DisplayStyle.Flex;
                renameField.style.display = DisplayStyle.None;
            });

            keyValueField.label = "";
            keyValueField.BindProperty(serializedObject.FindProperty(keyValuePropPath));
            keyValueField.AddManipulator(new ContextualMenuManipulator((evt) => {
                evt.menu.AppendAction("Delete", (x) => context.Blackboard.keys.RemoveAt(index), DropdownMenuAction.AlwaysEnabled);
            }));
        }

        private string FormatItem(Type arg)
        {
            if (arg == null)
            {
                return "(null)";
            }
            else
            {
                return arg.Name.Replace("Key", "");
            }
        }

        private void ValidateButton()
        {
            // Disable the create button if trying to create a non-unique key
            bool isValidKeyText = ValidateKeyText(newKeyTextField.text);
            createButton.SetEnabled(isValidKeyText);
        }

        private bool ValidateKeyText(string text)
        {
            if (text == "")
            {
                return false;
            }

            bool keyExists = context.Blackboard.Find(newKeyTextField.text) != null;
            return !keyExists;
        }

        private void CreateNewKey()
        {
            BlackboardKey key = BlackboardKey.CreateKey(newKeyTypeField.value);
            if (key != null)
            {
                key.name = newKeyTextField.value;
                context.Blackboard.keys.Add(key);
            }
            else
            {
                Debug.LogError($"Failed to create blackboard key, invalid type:{newKeyTypeField.value}");
            }

            ValidateButton();
        }
    }
}
