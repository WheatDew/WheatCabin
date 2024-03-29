using UnityEngine;
using System.Collections;

public class ResetPositionOnDiactivated : MonoBehaviour
{

  public EffectSettings EffectSettings;

  void Start()
  {
    EffectSettings.EffectDeactivated += EffectSettings_EffectDeactivated;
  }

  void EffectSettings_EffectDeactivated(object sender, System.EventArgs e)
  {
    transform.localPosition = Vector3.zero;
  }
}
