using UnityEngine;

public class Object_Checkpoint : MonoBehaviour, ISaveable
{
    [SerializeField] private string checkpointId;
    [SerializeField] private Transform respwanPoint;

    public bool isActive { get; private set; }
    private Animator anim;
    private AudioSource fireAudioSource;

    private void Awake()
    {
        EnsureComponents();
    }

    private void EnsureComponents()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>(true);

        if (fireAudioSource == null)
            fireAudioSource = GetComponent<AudioSource>();
    }
    public string GetCheckpointId()=>checkpointId;
    public Vector3 GetPosition()=> respwanPoint==null?transform.position : respwanPoint.position;



    public void ActivateCheckpoint(bool activate)
    {
        EnsureComponents();

        isActive = activate;
        if (anim != null)
            anim.SetBool("isActive", activate);

        if(fireAudioSource == null)
            return;

        if(isActive && fireAudioSource.isPlaying == false)
            fireAudioSource.Play();
        if(isActive == false)
            fireAudioSource.Stop();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        ActivateCheckpoint(true);
    }

    public void LoadData(GameData data)
    {
        bool active = data.unlockedCheckpoints.TryGetValue(checkpointId, out active);
        ActivateCheckpoint(active);
    }

    public void SaveData(ref GameData data)
    {
        if (isActive == false)
            return;
        if (data.unlockedCheckpoints.ContainsKey(checkpointId) == false)
            data.unlockedCheckpoints.Add(checkpointId, true);
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(checkpointId))
        {
            checkpointId = System.Guid.NewGuid().ToString();
        }
#endif
    }
}
