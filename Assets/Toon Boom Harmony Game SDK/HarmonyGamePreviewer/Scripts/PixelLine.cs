using UnityEngine;

namespace ToonBoom.Harmony.Previewer
{
    public class PixelLine : MonoBehaviour
    {
        public float LineWidth = 10.0f;

        protected void Update()
        {
            Camera mainCamera = Camera.main;

            //  Reproject pixel distance to world.
            float zOffset = mainCamera.WorldToScreenPoint(new Vector3(0.0f, 0.0f, 0.0f)).z;
            Vector3 devPoint = new Vector3(mainCamera.pixelWidth * 0.5f, mainCamera.pixelHeight * 0.5f, zOffset);
            Vector3 devPoint1 = new Vector3(devPoint.x + LineWidth, devPoint.y, devPoint.z);

            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(devPoint);
            Vector3 worldPoint1 = mainCamera.ScreenToWorldPoint(devPoint1);

            float worldWidth = Vector3.Distance(worldPoint, worldPoint1);

            //  Update LineRenderer with new width.
            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.startWidth = worldWidth;
            lineRenderer.endWidth = worldWidth;
        }
    }
}