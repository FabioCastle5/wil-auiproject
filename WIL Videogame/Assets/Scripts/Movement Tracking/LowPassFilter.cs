using UnityEngine;
using System.Collections;

public class LowPassFilter {
	
	private float smoothingFactor;
	private float smoothedValue;

	public LowPassFilter(float factor) {
		smoothedValue = 0f;
		smoothingFactor = factor;
	}

	public float Step(float sensorValue)
	{
		smoothedValue = smoothingFactor * sensorValue + (1 - smoothingFactor) * smoothedValue;
		return smoothedValue;
	}
}
