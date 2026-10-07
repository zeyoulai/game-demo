using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UI_TreeConnectDetails
{
    public UI_TreeConnectHandler childNode;
    public NodeDirectionType direction;
    [Range(100f,350f)]public float length;
    [Range(-50f,50f)] public float rotation;
}

public class UI_TreeConnectHandler : MonoBehaviour
{
    private RectTransform rect => GetComponent<RectTransform>();
    [SerializeField] private UI_TreeConnectDetails[] connectionDetails;
    [SerializeField] private UI_TreeConnection[] connections;

    private Image connectionImage;
    private Color originalColor;
    private bool originalColorCached;

    private void Awake()
    {
        if (connectionImage != null)
        {
            originalColor = connectionImage.color;
            originalColorCached = true;
        }
    }


    public UI_TreeNode[] GetChildrenNodes()
    {
        List<UI_TreeNode> childrenToReturn = new List<UI_TreeNode>();

        foreach (var node in connectionDetails)
        {
            if (node.childNode != null)
                childrenToReturn.Add(node.childNode.GetComponent<UI_TreeNode>());
        }
        return childrenToReturn.ToArray();
    }



    public void UpdateConnections()
    {
        for (int i = 0; i < connectionDetails.Length; i++)
        {
            var detail = connectionDetails[i];
            var connection = connections[i];

            Vector2 targetPosition = connection.GetConnectionPoint(rect);
            Image connectionImage = connection.GetConnecttionImage();

            connection.DirectionConnection(detail.direction,detail.length,detail.rotation);

            if (detail.childNode == null)
                continue;

            detail.childNode.SetPosition(targetPosition);
            detail.childNode.SetConnectionImage(connectionImage);
            detail.childNode.transform.SetAsLastSibling();

        }
    }

    public void UpdateAllConnections()
    {
        UpdateConnections();
        foreach (var node in connectionDetails)
        {
            if(node.childNode == null) continue;
            //node.childNode.UpdateAllConnections();
            node.childNode.UpdateConnections();
        }
    }

    public void UnlockConnectionImage(bool unlocked)
    {
        if (connectionImage == null)
            return;
        connectionImage.color = unlocked?Color.white:originalColor;
    }

    public void SetConnectionImage(Image image)
    {
        connectionImage = image;

        if (connectionImage != null && originalColorCached == false)
        {
            originalColor = connectionImage.color;
            originalColorCached = true;
        }
    }

    public void SetPosition(Vector2 position) => rect.anchoredPosition = position;


    private void OnValidate()
    {

        if (connectionDetails.Length != connections.Length)
        {
            Debug.Log("amount of details should be same as amount of connections");
            return;
        }
        UpdateConnections();
    }

}
