using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChalkManager : MonoBehaviour
{
    [SerializeField] private Image chalkBar;
    [SerializeField] public float chalkAmount;
    [SerializeField] private float maxChalk = 20f;
    [SerializeField] private bool regenOn;
    private int regenAmount = 5;

    void Start()
    {
        chalkAmount = maxChalk;
        StartCoroutine(HealingLoop());

    }
    void Update()
    {
        
    }
    public void ReduceChalk(float reduce)
    {
        chalkAmount -= reduce;
        chalkAmount = Mathf.Clamp(chalkAmount, 0, maxChalk);
        chalkBar.fillAmount = chalkAmount / maxChalk;
    }

    public void ReplenishChalk(float replenish)
    {
        chalkAmount += replenish;
        chalkAmount = Mathf.Clamp(chalkAmount, 0, maxChalk);
        chalkBar.fillAmount = chalkAmount / maxChalk;
    }

    public bool isEmpty()
    {
        return chalkAmount == 0;
    }

    private IEnumerator HealingLoop()
    {
        while (regenOn)
        {
            yield return new WaitForSeconds(1f);
            ReplenishChalk(regenAmount);
        }
    }
}
