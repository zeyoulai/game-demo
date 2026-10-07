using UnityEngine;

public class Object_NPC : MonoBehaviour,IInteractable
{
    protected Transform player;
    protected UI ui;
    protected Player_QuestManager questManager;

    [Header("Quest Info")]
    [SerializeField] private string npcTargetQurstId;
    [SerializeField] protected RewardType rewardNpc;
    [Space]
    [SerializeField] private Transform npc;
    [SerializeField] private GameObject interactToolTip;
    private bool facingRight = true;

    [Header("Floaty ToolTip")]
    [SerializeField] private float floatSpeed = 8f;
    [SerializeField] private float floatRange = .1f;
    private Vector3 startPosition;

    protected virtual void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        startPosition = interactToolTip.transform.position;
        interactToolTip.SetActive(false);
    }

    protected virtual void Start()
    {
        questManager = Player.instance.questManager;
    }

    protected  virtual void Update()
    {
        HandleNPCFlip();
        HandleToolTipFloat();
    }

    private void HandleToolTipFloat()
    {
        if (interactToolTip.activeSelf)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
            interactToolTip.transform.position = startPosition + new Vector3(0,yOffset,0);
        }
    }

    private void HandleNPCFlip()
    {
        if (player == null || npc == null)
            return;
        if(npc.position.x > player.position.x && facingRight)
        {
            npc.transform.Rotate(0, 180, 0);
            facingRight = false;
        }else if(npc.position.x < player.position.x && !facingRight)
        {
            npc.transform.Rotate(0, 180, 0);
            facingRight = true;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") == false)
            return;

        player = collider.transform;
        interactToolTip.SetActive(true);

    }

    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") == false)
            return;

        interactToolTip.SetActive(false);

    }

    public virtual void Interact()
    {
        questManager.AddProgress(npcTargetQurstId);
        //questManager.TryGetRewardFrom(rewardNpc);
    }
}
