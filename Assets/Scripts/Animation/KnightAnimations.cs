using UnityEngine;

public class KnightAnimations : MonoBehaviour
{
    const string POINT_TRIGGER = "TrPoint";
    const string WALK_TRIGGER = "TrWalk";
    const string STOP_WALK_TRIGGER = "TrStopWalk";

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

    [ContextMenu("Walk")]
    public void Walk()
    {
        //Rotate character to desired tile
        if (animator != null)
        {
            animator.SetTrigger(WALK_TRIGGER);
        }
    }

    [ContextMenu("StopWalk")]
    public void StopWalk()
    {
        //Rotate character to desired tile
        if (animator != null)
        {
            animator.SetTrigger(STOP_WALK_TRIGGER);
        }
    }
}
