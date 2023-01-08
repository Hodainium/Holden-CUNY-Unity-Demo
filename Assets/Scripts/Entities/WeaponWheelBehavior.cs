using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponWheelBehavior : MonoBehaviour
{
    public int Id;
    private Animator _anim;
    public string ItemName;
    public TextMeshProUGUI ItemText;
    public Image SelectedItem;
    public bool selected = false;
    public Sprite Icon;

    private void Start()
    {
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if(selected)
        {
            SelectedItem.sprite = Icon;
            ItemText.text = ItemName;
        }
    }

    public void Selected()
    {
        selected = true;
    }

    public void DeSelected()
    {
        selected = false;
    }

    public void HoverEnter()
    {
        _anim.SetBool("Hover", true);
        ItemText.text = ItemName;
    }

    public void HoverExit()
    {
        _anim.SetBool("Hover", false);
        ItemText.text = "";
    }
}

