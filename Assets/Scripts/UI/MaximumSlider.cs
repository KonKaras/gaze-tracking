using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MaximumSlider : Slider
{
	public MinimumSlider minSlider;

	protected override void Set(float input, bool sendCallback)
	{

		if (input <= minSlider.value)
		{
			return;
		}

		base.Set(input, sendCallback);
	}
}
