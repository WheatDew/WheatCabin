using Battlehub.UIControls;
using System;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor.UI
{
    public class SimpleProceduralExample 
    {
        //Build UI with absolute positioning
        [Procedural]
        public static void BuildUI(AutoUI autoUI)
        {          
            var (image, _) = autoUI.Image(false);
            image.rectTransform.TopLeft(new Vector2(5, 0), new Vector2(185, 185));
            
            var (label, _) = autoUI.Label(false);
            label.rectTransform.TopLeft(new Vector2(5, -200), new Vector2(185, 185));
            
            image.sprite = Resources.Load<Sprite>("pizza-100");
            label.text = "Pine trees are evergreen, coniferous resinous trees (or, rarely, shrubs) growing 3–80 m (10–260 ft) tall, with the majority of species reaching 15–45 m (50–150 ft) tall. The smallest are Siberian dwarf pine and Potosi pinyon, and the tallest is an 81.79 m (268.35 ft) tall ponderosa pine.";
            label.alignment = TextAlignmentOptions.TopJustified;
            label.overflowMode = TextOverflowModes.Overflow;
        }
        
        /*
        //Build UI using Layout elements
        [Procedural]        
        public static void BuildUI(AutoUI autoUI)
        {
            var (verticalGroup, _) = autoUI.BeginVerticalLayout();
            
            verticalGroup.childForceExpandWidth = false;
            verticalGroup.childForceExpandHeight = false;
            verticalGroup.spacing = 1;
            verticalGroup.padding = new RectOffset(5, 5, 5, 5);

            var (image, imageLayout) = autoUI.Image();
            imageLayout.preferredWidth = 200;
            imageLayout.preferredHeight = 200;
            imageLayout.flexibleHeight = 0;

            var (label, labelLayout) = autoUI.Label();
            labelLayout.flexibleHeight = 0;
            labelLayout.preferredHeight = 30;

            image.sprite = Resources.Load<Sprite>("pizza-100");
            label.text = "Pine trees are evergreen, coniferous resinous trees (or, rarely, shrubs) growing 3–80 m (10–260 ft) tall, with the majority of species reaching 15–45 m (50–150 ft) tall. The smallest are Siberian dwarf pine and Potosi pinyon, and the tallest is an 81.79 m (268.35 ft) tall ponderosa pine.";
            label.alignment = TextAlignmentOptions.TopJustified;
            label.overflowMode = TextOverflowModes.Overflow;
         
            autoUI.EndVerticalLayout();
        }
        */

        [DialogCancelAction("Close")]
        public void OnClose()
        {
            Debug.Log("Close");
        }
    }

}
