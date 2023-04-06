using Battlehub.RTCommon;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class AboutViewModel : MonoBehaviour
    {
        [Binding]
        public string Text
        {
            get
            {
                return $"Runtime Editor v{RTEVersion.Version} created by Battlehub@outlook.com {System.Environment.NewLine} Telegram: https://t.me/battlehub";
            }
        }
    }
}

