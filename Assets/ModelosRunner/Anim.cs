using UnityEngine;

public class PlayAnimation : MonoBehaviour
{
    void Start()
    {
        Animator animator = GetComponent<Animator>();
        animator.Play("YourAnimationClipName"); // Reemplaza con el nombre de tu clip
    }
}