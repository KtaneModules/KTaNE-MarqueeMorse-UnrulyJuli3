using UnityEngine;

public class MarqueeLight : MonoBehaviour
{
	public Renderer LightRenderer;
	public Material OnMaterial;
	public Material OffMaterial;
	public void Set(bool on)
	{
		LightRenderer.material = on ? OnMaterial : OffMaterial;
	}
}
