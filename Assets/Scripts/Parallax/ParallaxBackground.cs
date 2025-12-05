using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;
    private float lastMainCameraPositiomX;
    private float cameraHalfWidth;

    [SerializeField] private ParallaxLayer[] backgroundLayers;

    private void Awake()
    {
        mainCamera = Camera.main;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        CalculateBackgroundWidth();
    }

    private void FixedUpdate()
    {
        float currentCameraPositionX = mainCamera.transform.position .x;
        float distanceToMove = currentCameraPositionX - lastMainCameraPositiomX;
        lastMainCameraPositiomX = currentCameraPositionX;

        float cameraLeftEdge = currentCameraPositionX - cameraHalfWidth;
        float cameraRightEdge = currentCameraPositionX + cameraHalfWidth;

        foreach(ParallaxLayer layer in backgroundLayers)
        {
            layer.Move(distanceToMove);
            layer.LoopBackground(cameraLeftEdge, cameraRightEdge);
        }

    }

    private void CalculateBackgroundWidth()
    {
        foreach (ParallaxLayer layer in backgroundLayers)
        {
            layer.CalculateImageWidth();

        }
    }

}
