using UnityEngine;

public class Skill_SwordThrow : Skill_Base
{
    private SkillObject_Sword currentSword;
    private float currentThrowPower;

    [Header("Regular Sword Upgrade")]
    [Range(0, 10)]
    [SerializeField] private float regulatThrowPower = 5;
    [SerializeField] private GameObject swordPrefab;

    [Header("Peirce Sword Upgrade")]
    [SerializeField] private GameObject peirceSwordPrefab;
    public int amountToPeirce = 2;
    [Range(0, 10)]
    [SerializeField] private float peirceThrowPower = 5;

    [Header("Spin Sword Upgrade")]
    [SerializeField] private GameObject spinSwordPrefab;
    public int maxDistance = 5;
    public float attacksPerSecound = 6;
    public float maxSpinDuration = 4;
    [Range(0, 10)]
    [SerializeField] private float spinThrowPower = 5;

    [Header("Bounce Sword Upgrade")]
    [SerializeField] private GameObject bounceSwordPrefab;
    public int bounceCount;
    public float bounceSpeed;
    [Range(0, 10)]
    [SerializeField] private float bounceThrowPower = 5;

    [Header("Trajectory prediction")]
    [SerializeField] private GameObject predictionDot;
    [SerializeField] private int numberOfDots = 20;
    [SerializeField] private float spaceBetweenDots = 0.05f;
    private float swordGravity;
    private Transform[] dots;
    private Vector2 confirmDirection;

    protected override void Awake()
    {
        base.Awake();
        swordGravity = swordPrefab.GetComponent<Rigidbody2D>().gravityScale;
        dots = GenerateDots();
    }

    public override bool CanUseSkill()
    {
        UpdataThrowPower();
        if (currentSword != null)
        {
            currentSword.GetSwordBackToPlayer();
            return false;
        }


        return base.CanUseSkill();
    }

    public void ThrowSword()
    {
        GameObject swordPrefab = GetSwordPrefab();
        GameObject newSword = Instantiate(swordPrefab, dots[1].position, Quaternion.identity);
        currentSword = newSword.GetComponent<SkillObject_Sword>();
        currentSword.SetupSword(this, GetThrowPower());

        SetSkillOnCooldown();

    }

    private GameObject GetSwordPrefab()
    {
        if (Unlocked(SkillUpgradeType.SwordThrow))
            return swordPrefab;
        if (Unlocked(SkillUpgradeType.SwordThrow_Pierce))
            return peirceSwordPrefab;
        if (Unlocked(SkillUpgradeType.SwordThrow_Spin))
            return spinSwordPrefab;
        if(Unlocked(SkillUpgradeType.SwordThrow_Bounce))
            return bounceSwordPrefab;
        Debug.Log("No valid sword upgrade selected!");
        return null;
    }

    private void UpdataThrowPower()
    {
        switch (upgradeType)
        {
            case SkillUpgradeType.SwordThrow:
                currentThrowPower = regulatThrowPower;
                return;
            case SkillUpgradeType.SwordThrow_Pierce:
                currentThrowPower = peirceThrowPower;
                return;
            case SkillUpgradeType.SwordThrow_Spin:
                currentThrowPower = spinThrowPower;
                return;
            case SkillUpgradeType.SwordThrow_Bounce:
                currentThrowPower = bounceThrowPower;
                return;
        }
    }

    private Vector2 GetThrowPower() => confirmDirection * (currentThrowPower * 10);

    public void PredictTrajectory(Vector2 direction)
    {
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].position = GetTrajectoryPoint(direction, i * spaceBetweenDots);
        }
    }

    private Vector2 GetTrajectoryPoint(Vector2 direction, float t)
    {
        float scaledThrowPower = currentThrowPower * 10;
        Vector2 initialVeclocity = direction * scaledThrowPower;

        Vector2 gravityEffect = 0.5f * Physics2D.gravity * swordGravity * (t * t);

        Vector2 predictedPoint = (initialVeclocity * t) + gravityEffect;

        Vector2 playerPosition = transform.root.position;

        return playerPosition + predictedPoint;
    }

    public void confirmTrajectory(Vector2 direction) => confirmDirection = direction;

    public void EnableDots(bool enable)
    {

        foreach (Transform transform in dots)
        {
            transform.gameObject.SetActive(enable);
        }
    }

    private Transform[] GenerateDots()
    {
        Transform[] newDots = new Transform[numberOfDots];

        for (int i = 0; i < numberOfDots; i++)
        {
            newDots[i] = Instantiate(predictionDot, transform.position, Quaternion.identity, transform).transform;
            newDots[i].gameObject.SetActive(false);
        }

        return newDots;
    }
}
