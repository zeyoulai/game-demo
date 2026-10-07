using UnityEngine;

public class Object_Chest : MonoBehaviour, IDamagable
{
    private Animator anim => GetComponentInChildren<Animator>();
    private Rigidbody2D rb => GetComponentInChildren<Rigidbody2D>();
    private Entity_VFX vfx => GetComponent<Entity_VFX>();
    private Entity_DropManager dropManager => GetComponent<Entity_DropManager>();

    [Header("Open Details")]
    [SerializeField] private Vector2 knockback;
    [SerializeField] private bool canDropItems = true; 

    public bool TakeDamage(float damage,float elementalDamage,ElementType element, Transform transform)
    {
        if(canDropItems == false) return false;

        canDropItems = false;
        dropManager?.DropItems();
        vfx?.PlayOnDamageVFX();
        anim.SetBool("chestOpen", true);
        rb.linearVelocity = knockback;

        rb.angularVelocity = Random.Range(-200, 200);

        //Drop items
        return true;
    }

}
