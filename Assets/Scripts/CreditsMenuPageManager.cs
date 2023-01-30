using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMenuPageManager : MonoBehaviour
{
    [SerializeField] GameObject _creditsPage1Objects;
    [SerializeField] GameObject _creditsPage2Objects;

    private void OnEnable()
    {
        _creditsPage1Objects.SetActive(true);
        _creditsPage2Objects.SetActive(false);
    }

    public void OnClickNextPage()
    {
        _creditsPage1Objects.SetActive(false);
        _creditsPage2Objects.SetActive(true);
    }

    public void OnClickPreviousPage()
    {
        _creditsPage1Objects.SetActive(true);
        _creditsPage2Objects.SetActive(false);
    }
}
