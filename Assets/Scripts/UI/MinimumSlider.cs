using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MinimumSlider : Slider
{
	public MaximumSlider maxSlider;

	protected override void Set(float input, bool sendCallback)
	{	

		if (input >= maxSlider.value)
		{
			return;
		}

		base.Set(input, sendCallback);
	}
}
