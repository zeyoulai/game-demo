using System.Collections;
using UnityEngine;



public class Object_Buff : MonoBehaviour
{

    private Player_Stats statsToModify;

    [Header("Buff details")]
    [SerializeField] private BuffEffectData[] buffs;
    [SerializeField] private string buffName;
    [SerializeField] private float buffDuration = 4;


    [Header("Floaty movement")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatRange = .1f;
    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;

    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed)*floatRange;
        transform.position = startPosition + new Vector3(0, yOffset);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        statsToModify = collision.GetComponent<Player_Stats>();

        if (statsToModify.CanApplyBuffOf(buffName))
        {
            statsToModify.ApplyBuff(buffs, buffDuration,buffName);
            Destroy(gameObject);
        }

    }

    //private IEnumerator BuffCo(float duration)
    //{
    //    canBeUsed = false;
    //    sr.color = Color.clear;

    //    ApplyBuff(true);

    //    yield return new WaitForSeconds(duration);
    //    ApplyBuff(false);
    //    Destroy(gameObject);
    //}

    //private void ApplyBuff(bool apply)
    //{
    //    foreach (var buff in buffs)
    //    {
    //        if (apply)
    //            statsToModify.GetStatByType(buff.type).AddModifier(buff.value, buffName);
    //        else
    //            statsToModify.GetStatByType(buff.type).RemoveModifier(buffName);
    //    }
    //}
}


