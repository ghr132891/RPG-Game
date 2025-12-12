using UnityEngine;

public class Skill_SwordThrow : Skill_Base
{
    private SkillObject_Sword currentSword;

    [Header("Regular Sword Upgrade")]
    [Range(0, 10)]
    [SerializeField] private float throwPower = 4f;
    [SerializeField] private GameObject swordPrefab;

    [Header("Pierce Sword Upgrade")]
    [SerializeField] private GameObject pierceSwordPrefab;
    public int amountToPierce = 2;

    [Header("Spin Sword Upgrade")]
    [SerializeField] private GameObject spinSwordPrefab;
    public int maxDistance = 5;
    public float attackPerSecond = 6;
    public float maxSpinDuration = 5;


    [Header("Traiectory prediction")]
    [SerializeField] private GameObject predictionDot;
    [SerializeField] private int numberOfDots = 20;
    [SerializeField] private float spaceBetweenDots = .05f;
    private float swordGravity;
    private Transform[] dots;
    private Vector2 confirmedDirection;


    public void ConfirmTrajectory(Vector2 direction) => confirmedDirection = direction;


    protected override void Awake()
    {
        base.Awake();
        swordGravity = swordPrefab.GetComponent<Rigidbody2D>().gravityScale;
        dots = GenerateDots();

    }

    public override bool CanUseSkill()
    {
        if (currentSword != null)
        {
            currentSword.GetSwordBackToPlayer();
            return false;
        }

        return base.CanUseSkill();


    }

    private GameObject GetSwordPrefab()
    {
        if (Unlocked(SkillUpgradeType.SwordThrow))
            return swordPrefab;

        if (Unlocked(SkillUpgradeType.SwordThrow_Pierce))
            return pierceSwordPrefab;

        if(Unlocked(SkillUpgradeType.SwordThrow_Spin))
            return spinSwordPrefab;

        Debug.Log("No valied sword upgrade selected.");
        return null;


    }

    public void ThrowSword()
    {
        GameObject swordPrefab = GetSwordPrefab();

        GameObject newSword = Instantiate(swordPrefab, dots[1].position, Quaternion.identity);

        currentSword = newSword.GetComponent<SkillObject_Sword>();
        currentSword.SetupSword(this, GetThrowPower());


    }

    private Vector2 GetThrowPower() => confirmedDirection * (throwPower * 10);

    public void PredictTrajectory(Vector2 direction)
    {
        for (int i = 0; i < numberOfDots; ++i)
        {
            dots[i].position = GetTrajectoryPoint(direction, i * spaceBetweenDots);
        }
    }

    private Vector2 GetTrajectoryPoint(Vector2 direction, float t)
    {
        float scaledThrowPower = throwPower * 10;

        // This gives us the initial velocity - the starting speed and direction of the throw.
        Vector2 initialVelocity = direction * scaledThrowPower;

        Vector2 gravityEffect = .5f * Physics2D.gravity * swordGravity * (t * t);

        Vector2 predictedPoint = (initialVelocity * t) + gravityEffect;

        Vector2 playerPosition = player.transform.root.position;

        return playerPosition + predictedPoint;

    }

    public void EnableDots(bool enable)
    {
        foreach (Transform t in dots)
            t.gameObject.SetActive(enable);

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