using Battlehub.RTCommon;
using Battlehub.RTEditor.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ColorEditor : PropertyEditor<Color>
    {
        [SerializeField]
        private Image MainColor = null;

        [SerializeField]
        private RectTransform Alpha = null;

        [SerializeField]
        private Button BtnSelect = null;

        protected override void SetInputField(Color value)
        {
            if(HasMixedValues())
            {
                MainColor.color = new Color(0, 0, 0, 0);
                Alpha.gameObject.SetActive(false);
            }
            else
            {
                Color color = value;
                color.a = 1.0f;
                MainColor.color = color;
                Alpha.transform.localScale = new Vector3(value.a, 1, 1);
                Alpha.gameObject.SetActive(true);
            }   
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            BtnSelect.onClick.AddListener(OnSelect);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (BtnSelect != null)
            {
                BtnSelect.onClick.RemoveListener(OnSelect);
            }
        }

        private void OnSelect()
        {
            ILocalization localization = IOC.Resolve<ILocalization>();
            string memberInfoTypeName = localization.GetString("ID_RTEditor_PE_TypeName_" + MemberInfoType.Name, MemberInfoType.Name);
            string select = localization.GetString("ID_RTEditor_PE_ColorEditor_Select", "Select") + " ";

            SelectColorDialog colorSelector = null;
            SelectColorViewModel colorSelectorViewModel = null;
            Transform dialogTransform = IOC.Resolve<IWindowManager>().CreateDialogWindow(RuntimeWindowType.SelectColor.ToString(), select + memberInfoTypeName,
                (sender, args) =>
                {
                    if(colorSelector != null)
                    {
                        //Legacy
                        OnColorSelected(colorSelector.SelectedColor);
                    }
                    
                    if(colorSelectorViewModel != null)
                    {
                        OnColorSelected(colorSelectorViewModel.SelectedColor);
                    }
                    
                }, 
                (sender, args) => { }, false);

            //Legacy
            colorSelector = dialogTransform.GetComponentInChildren<SelectColorDialog>();
            if(colorSelector != null)
            {
                colorSelector.SelectedColor = GetValue();
            }

            colorSelectorViewModel = dialogTransform.GetComponentInChildren<SelectColorViewModel>();
            if(colorSelectorViewModel != null)
            {
                colorSelectorViewModel.SelectedColor = GetValue();
            }
        }

        private void OnColorSelected(Color selectedColor)
        {
            BeginEdit();
            SetValue(selectedColor);
            EndEdit();
            SetInputField(selectedColor);
        }
    }

}
