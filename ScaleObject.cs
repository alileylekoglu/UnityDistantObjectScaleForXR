using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using System;

public class ScaleObject : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Vector3 originalScale;
    private XRBaseInteractor grabInteractor;
    private List<InputDevice> devices = new List<InputDevice>();
    private bool scaleModeActive = false;
    private const float ScaleSpeed = 0.1f; // Adjust this value to make the scaling faster or slower
    private float cumulativeScale = 1f; // Added this new variable

    public float minScale;
    public float maxScale;
    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(StartScaling);
        grabInteractable.selectExited.AddListener(EndScaling);
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(StartScaling);
        grabInteractable.selectExited.RemoveListener(EndScaling);
    }

    void StartScaling(SelectEnterEventArgs args)
    {
        grabInteractor = args.interactor;
        cumulativeScale = 1f; // Reset cumulativeScale every time scaling starts
    }


    void EndScaling(SelectExitEventArgs args)
    {
        if (scaleModeActive)
        {
            originalScale = transform.localScale;
            scaleModeActive = false;
        }
    }
    void Update()
    {
        if (grabInteractable.isSelected)
        {
            // Handle scale on primary button press
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, devices);
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed) && isPressed)
                {
                    scaleModeActive = true;
                    grabInteractable.trackPosition = false; // stop tracking position
                    grabInteractable.trackRotation = false; // stop tracking rotation
                }
            }

            // Disable scaling on secondary button press
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, devices);
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressed) && isPressed)
                {
                    scaleModeActive = false;
                    grabInteractable.trackPosition = true; // resume tracking position
                    grabInteractable.trackRotation = true; // resume tracking rotation
                }
            }

            if (scaleModeActive)
            {
                for (int i = 0; i < devices.Count; i++)
                {
                    if (devices[i].TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxis))
                    {
                        float scaleFactor = 1 + (primary2DAxis.y * ScaleSpeed);
                        scaleFactor = Mathf.Round(scaleFactor * 10f) / 10f; // Round to nearest tenth

                        cumulativeScale *= scaleFactor; // Accumulate the scale factor
                        cumulativeScale = Mathf.Clamp(cumulativeScale, minScale, maxScale); // Limit scale to range 0.1 to 1

                        transform.localScale = originalScale * cumulativeScale; // Use cumulativeScale instead of scaleFactor
                    }
                }
            }
        }
    }


}
