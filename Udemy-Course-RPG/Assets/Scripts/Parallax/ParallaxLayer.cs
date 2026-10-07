using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    [SerializeField] private Transform backgroud;
    [SerializeField] private float parallaxMultiplier;
    [SerializeField] private float imageWidthOffset = 10f;

    private float imageFullWidth;
    private float imageHalfWidth;


    public void CalculateImageWidth()
    {
        imageFullWidth = backgroud.GetComponent<SpriteRenderer>().bounds.size.x;
        imageHalfWidth = imageFullWidth / 2;
    }

    public void Move(float distanceToMove)
    {
        backgroud.position += Vector3.right * (distanceToMove*parallaxMultiplier);
    }

    public void LoopBackground(float cameraLeftEdge, float cameraRightEdge)
    {
        float imageRightEdge = (backgroud.position.x + imageHalfWidth) - imageWidthOffset;
        float imageLeftEdge = (backgroud.position.x - imageHalfWidth)+imageWidthOffset;

        if (imageRightEdge < cameraLeftEdge)
            backgroud.position += Vector3.right * imageFullWidth;
        else if(imageLeftEdge > cameraRightEdge)
            backgroud.position += Vector3.right * -imageFullWidth;
    }

}
