using UnityEngine;

public class KnightAnimations : MonoBehaviour
{
    const string POINT_TRIGGER = "TrPoint";

    [SerializeField] private Animator animator;

    [ContextMenu("PointAt")]
    public void PointAt()
    {
        //Rotate character to desired tile
        if (animator != null)
        {
            animator.SetTrigger(POINT_TRIGGER);
        }
    }
}
