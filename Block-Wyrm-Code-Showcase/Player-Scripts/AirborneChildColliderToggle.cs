using UnityEngine;

public class AirborneGroundedChildColliderToggle : MonoBehaviour
{
    /*
    If player collided with 2 or more colliders of a falling block, that block wouldn't be able to push player into ground, unable to kill player. 
    Now the player's OG collider is smaller and leaves InstaKill Collider outside, meaning player doesnt has be pushed into ground to be killed anymore,
    To fix the new issue that came with this, thats the fact that player can jump and hit their heads to instantly die while in air. 
    I added a new collider that is only activated while player is in air that blocks the collision of instakill.  
        New problem.
        Two sides? to the problem, 
            Player can walk into a 1 block gap, player dies when jumping in the gap.
            Player can't walk to 1 block gaps, player doesn't die When crushed by a 1 block gap. and gets stuck.
        Possible solutions, 
            Player can walk into a 1 block gap, but doesn't get insta killed if he is grounded
                Problem, Half of player is blocked from view and looks bad.
                This may also later cause problems because player get stuck while in air, but seems unlikely.
    I think i solved it, I just made the base ground collider on player bigger so he can't walk into gaps anymore, now i feel dumb for spending 2 hours on this
            
    */
    [Header("References")]
    [SerializeField] private PlayerController2D_Mobile playerController;

    [Header("Collider enabled while player is in air")]
    [SerializeField] private Collider2D airborneCollider;


    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController2D_Mobile>();

        ApplyState();
    }

    private void Update()
    {
        ApplyState();
    }

    private void ApplyState()
    {
        if (playerController == null)
            return;

        bool isGrounded = playerController.IsGrounded;
        bool isAirborne = !isGrounded;

        if (airborneCollider != null)
            airborneCollider.enabled = isAirborne;

    }
}