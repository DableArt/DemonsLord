using System;
using System.Collections.Generic;
using UnityEngine;

public class MinimapMarker : MonoBehaviour
{
    private static readonly HashSet<MinimapMarker> Registered = new HashSet<MinimapMarker>();

    [SerializeField] private MinimapMarkerType markerType;
    [SerializeField] private string customIconId;
    [SerializeField] private bool rotateWithTransform = false;

    public static event Action<MinimapMarker, bool> MarkerRegistrationChanged;

    public static IEnumerable<MinimapMarker> RegisteredMarkers => Registered;

    public MinimapMarkerType MarkerType => markerType;
    public string CustomIconId => customIconId;
    public bool RotateWithTransform => rotateWithTransform;

    private void OnEnable()
    {
        Registered.Add(this);
        MarkerRegistrationChanged?.Invoke(this, true);

        if (MinimapController.Instance != null)
        {
            MinimapController.Instance.RegisterMarker(this);
        }
    }

    private void OnDisable()
    {
        Registered.Remove(this);
        MarkerRegistrationChanged?.Invoke(this, false);

        if (MinimapController.Instance != null)
        {
            MinimapController.Instance.UnregisterMarker(this);
        }
    }
}
