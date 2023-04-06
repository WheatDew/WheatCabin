using Battlehub.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class TimelineTextPanel : MonoBehaviour
    {
        [SerializeField]
        public RectTransform m_textRoot = null;

        [SerializeField]
        private TimelineText m_textPrefab = null;

        private List<TimelineText> m_textList = new List<TimelineText>();

        private int m_linesCount;
        private int m_secondaryLinesCount;
        private int m_samples;

        public void SetGridParameters(int linesCount, int secondaryLinesCount, int samples)
        {
            m_linesCount = linesCount;
            m_secondaryLinesCount = secondaryLinesCount;
            m_samples = samples;

            int sqSecondaryLinesCount = m_secondaryLinesCount * m_secondaryLinesCount;
            int totalLinesCount = m_linesCount * m_secondaryLinesCount + 1;
            int delta = totalLinesCount - m_textRoot.childCount;

            if(delta > 0)
            {
                for(int i = 0; i < delta; ++i)
                {
                    TimelineText text = Instantiate(m_textPrefab, m_textRoot);
                    m_textList.Add(text);
                }
            }
            else
            {
                int lastChildIndex = m_textRoot.childCount - 1;
                for (int i = lastChildIndex; i >= lastChildIndex - delta; i--)
                {
                    Transform child = m_textRoot.GetChild(i);
                    m_textList.Remove(child.GetComponent<TimelineText>());
                    Destroy(child.gameObject);   
                }
            }

            for (int i = 0; i < m_textList.Count; ++i)
            {
                TimelineText text = m_textList[i];

                bool isSecondary = (i % m_secondaryLinesCount) != 0;
                bool isPrimary = !isSecondary && (i % sqSecondaryLinesCount) != 0;

                text.IsSecondary = isSecondary;
                text.IsPrimary = isPrimary;
            }
        }

        public void UpdateGraphics(float viewportSize, float contentSize, float scrollOffset, float scrollSize, float interval)
        {
        
            float kLines = m_secondaryLinesCount;
            
            float size = contentSize;
            size /= interval;
   
            float s1 = Mathf.Pow(kLines, Mathf.Ceil(Mathf.Log(interval * scrollSize, kLines)));
            float s0 = Mathf.Pow(kLines, Mathf.Ceil(Mathf.Log(interval * scrollSize, kLines)) - 1);

            size *= s1;

            m_textRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_textRoot);

            Vector2 position = m_textRoot.anchoredPosition;
            float mod = (((contentSize / m_linesCount) / interval) * s1);
            position.x = -((contentSize - viewportSize) * scrollOffset) % mod;
            m_textRoot.anchoredPosition = position;

            float seconarySpace = size / m_textList.Count;
            float primarySpace = seconarySpace * m_secondaryLinesCount;

            float roundTo = Mathf.Pow(kLines, Mathf.Floor(Mathf.Log(s1, kLines)));
            float offset = Mathf.FloorToInt(m_linesCount * interval * (1 - scrollSize) * scrollOffset / roundTo) * roundTo;

            const int maxDigits = 5;

            int maxIntervalDigits = Mathf.RoundToInt(MathHelper.CountOfDigits(Mathf.CeilToInt(m_linesCount * interval / m_samples)));
            int sampleDigits = Mathf.RoundToInt(MathHelper.CountOfDigits(m_samples));

            string intervalFormat = "0";
            string sampleFormat = "1:D" + sampleDigits;

            string format;
            if(maxDigits - maxIntervalDigits >= sampleDigits)
            {    
                format = "{" + intervalFormat + "}:{" + sampleFormat + "}";
            }
            else
            {
                format = "{" + intervalFormat + "}";
            }
            
            for (int i = 0; i < m_textList.Count; ++i)
            {
                TimelineText text = m_textList[i];
                text.ForceMeshUpdate();
            }

            int pad = Mathf.Max(maxIntervalDigits, maxDigits + 1);
            for (int i = 0; i < m_textList.Count; ++i)
            {
                TimelineText text = m_textList[i];

                int t = Mathf.RoundToInt(offset + i * s0);
                int intervalNumber = t / m_samples;
                int sampleNumber = t % m_samples;

                string txt = string.Format(format, intervalNumber, sampleNumber).PadRight(pad, ' ');
                text.Refresh(primarySpace, seconarySpace, txt);
                text.Text = txt;
            }
        }
    }

}
