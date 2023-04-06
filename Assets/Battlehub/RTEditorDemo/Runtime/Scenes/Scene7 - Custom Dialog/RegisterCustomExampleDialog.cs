using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene7
{
	[MenuDefinition]
	public class RegisterCustomExampleDialog : EditorExtension
	{
		[SerializeField]	
		private GameObject m_prefab = null;

		protected override void OnInit()
		{
			base.OnInit();
				
			IWindowManager wm = IOC.Resolve<IWindowManager>();
			wm.RegisterWindow("Custom Example Dialog", null, null, m_prefab, true);
		}

		[MenuCommand("MenuWindow/Custom Dialog", "")]
		public void Open()
		{
			IWindowManager wm = IOC.Resolve<IWindowManager>();
			wm.CreateDialogWindow("Custom Example Dialog", "Custom Dialog",
				(sender, args) => { Debug.Log("On OK"); },
				(sender, args) => { Debug.Log("On Cancel"); });
		}
	}
}



