using System.Collections;
using UnityEngine;

public class Entity_VFX : MonoBehaviour
{
    protected SpriteRenderer sr;
    private Entity entity;

    [Header("Image Echo VFX")]
    [Range(0.01f, 0.2f)]
    [SerializeField] private float imageEchoInterval = .05f;
    [SerializeField] private GameObject imageEchoPrefab;
    private Coroutine imageEchoCo;

    [Header("On Taking Damage VFX")]
    [SerializeField] private Material onDamageMaterial;
    [SerializeField] private float onDamageVFXDuration = 0.2f;
    private Material originalMaterial;
    private Coroutine onDamageVFXCoroutine;

    [Header("On Doing Damage VFX")]
    [SerializeField] private Color hitVFXColor = Color.white;
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject critHitVFX;

    [Header("Element Colors")]
    [SerializeField] private Color chillVfx = Color.cyan;
    [SerializeField] private Color burnVfx = Color.red;
    [SerializeField] private Color eletrcifyVfx = Color.yellow;
    private Color originalHitVfxColor;


    protected virtual void Awake()
    {
        entity = GetComponent<Entity>();
        sr = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = sr.material;
        originalHitVfxColor = hitVFXColor;
    }

    private void Start()
    {

    }

    public void DoImageEchoEffect(float duration)
    {
        StopImageEchoEffect();
        imageEchoCo = StartCoroutine(ImageEchoEffectCo(duration));
    }

    public void StopImageEchoEffect()
    {
        if (imageEchoCo != null)
            StopCoroutine(imageEchoCo);

    }

    private IEnumerator ImageEchoEffectCo(float duration)
    {
        float timeTracker = 0;
        while (timeTracker < duration)
        {
            CreateImage();
            yield return new WaitForSeconds(imageEchoInterval);
            timeTracker += imageEchoInterval;
        }
    }

    private void CreateImage()
    {
        Vector3 position = entity.anim.transform.position;
        float scale = entity.anim.transform.localScale.x;

        GameObject imageEcho = Instantiate(imageEchoPrefab, position, transform.rotation);

        imageEcho.transform.localScale = new Vector3(scale, scale, scale);  
        imageEcho.GetComponentInChildren<SpriteRenderer>().sprite = sr.sprite;
    }


    public void PlayOnStatusVfx(float duration,ElementType element)
    {
        if (element == ElementType.Ice)
            StartCoroutine(PlayStatusVfxCo(duration, chillVfx));
        if (element == ElementType.Fire)
            StartCoroutine(PlayStatusVfxCo(duration, burnVfx));
        if (element == ElementType.Lightning)
            StartCoroutine(PlayStatusVfxCo(duration, eletrcifyVfx));
    }

    public void StopAllVfx()
    {
        StopAllCoroutines();
        sr.color = Color.white;
        sr.material = originalMaterial;
    }

    private IEnumerator PlayStatusVfxCo(float duration,Color effectColor)
    {
        float tickInterval = .25f;
        float timerHasPassed = 0;

        Color lightColor = effectColor * 1.2f;
        Color darkColor = effectColor * .8f;

        bool toggle = false;

        while(timerHasPassed < duration)
        {
            sr.color = toggle ? lightColor : darkColor;
            toggle = !toggle;

            yield return new WaitForSeconds(tickInterval);
            timerHasPassed += tickInterval;
        }

        sr.color = Color.white;
    }

    public void CreateOnHitVFX(Transform target,bool isCrit,ElementType element)
    {
        GameObject hitPrefab = isCrit ? critHitVFX : hitVFX;
        GameObject vfx = Instantiate(hitPrefab, target.position, Quaternion.identity);
        //vfx.GetComponentInChildren<SpriteRenderer>().color = GetElementColor(element); //攻击的攻击特效白色的就行了

        if (entity.facingDir == -1 && isCrit) //这一步其实可以在这个vfx预制体里面做，更解耦
            vfx.transform.Rotate(0, 180, 0);

    }

    public Color GetElementColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Ice:return chillVfx;
            case ElementType.Fire:return burnVfx;
            case ElementType.Lightning:return eletrcifyVfx;
            default: return Color.white;
        }
    }

    public void PlayOnDamageVFX()
    {
        if (onDamageVFXCoroutine != null)
            StopCoroutine(onDamageVFXCoroutine);
        onDamageVFXCoroutine = StartCoroutine(OnDamageVfxCo());
    }

    private IEnumerator OnDamageVfxCo()
    {
        sr.material = onDamageMaterial;
        yield return new WaitForSeconds(onDamageVFXDuration);
        sr.material = originalMaterial;
    }

}
