using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMouseRayCaster : MonoBehaviour
{
    public CardBehaviour attachedCard;

    [SerializeField]private CardBehaviour focusedCard;
    [SerializeField] LayerMask interactableMask;
    [SerializeField] LayerMask tableMask;

    private bool cursorLeftUp = true;
    private bool cursorRightUp = true;
    private AudioManager audioManager;

    const float RAY_LENGTH = 10f;
    const float ON_TABLE_CARD_OFFSET = 0.55f;
    const float SELECTED_CARD_OFFSET = 0.8f;

    private void Start()
    {
        audioManager = GetComponent<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        HighlightInteractables();
        PickUpCard();
        MoveAttachedCardWithMouse();
        DropCard();
        InteractWithCard();
        CursorIsUpCheck();
    }

    //USed to Highlight cards that our ray hits 
    private void HighlightInteractables()
    {
        // Create the ray at our mouse point.
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;

        // If the ray hits an interactable, set it as the focused card and make it glow, if it misses rremvoe the glow off our previously highlighted interactable if it exists.
        if (Physics.Raycast(ray, out rayHit, RAY_LENGTH, interactableMask))
        {
            if (rayHit.transform.GetComponent<CardBehaviour>() != focusedCard && rayHit.transform.GetComponent<CardBehaviour>() != attachedCard && rayHit.transform.GetComponent<CardBehaviour>() != null)
            {
                if (focusedCard != null)
                    focusedCard.MouseLeft();
                focusedCard = rayHit.transform.GetComponent<CardBehaviour>();
                focusedCard.MouseHovering();
            }
        }
        else
        {
            if (focusedCard != null)
                focusedCard.MouseLeft();
            focusedCard = null;
        }

    }

    // Used to pickup a card and attahc it to our cursor.
    private void PickUpCard()
    {
        // If the mouse button is down, pick up this card.
        if (attachedCard == null)
        {
            if (Input.GetAxisRaw("Primary Click") == 1 && cursorLeftUp)
            {
                cursorLeftUp = false;
                if (focusedCard != null && !focusedCard.cardLocked)
                {
                    if (focusedCard.cardType != CardBehaviour.CardType.PassLeft && focusedCard.cardType != CardBehaviour.CardType.PassRight)
                    {
                        attachedCard = focusedCard;
                        attachedCard.gameObject.layer = 2;
                        attachedCard.GetComponent<Animator>().SetBool("OnAnotherObject", false);
                        attachedCard.MouseAttach();
                        focusedCard = null;
                        audioManager.PlaySound(CardBank.instance.audioClips[0], false);
                    }
                }
                else
                {
                    //Debug.Log("we clicked on nothing");
                }
            }
        }
    }

    // Used to move the attached card with the mouse.
    private void MoveAttachedCardWithMouse()
    {
        // If we have an attached card, set it's location to the mouse position
        if (attachedCard != null)
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit, RAY_LENGTH, tableMask))
            {
                attachedCard.transform.position = new Vector3(rayHit.point.x, SELECTED_CARD_OFFSET, rayHit.point.z);
            }
        }
    }

    // Used to drop a card at the specified location. If we are highlighting another card, set us to interact with that card.
    private void DropCard()
    {
        if(attachedCard != null)
        {
            if (Input.GetAxisRaw("Primary Click") == 1 && cursorLeftUp)
            {
                cursorLeftUp = false;
                if (focusedCard != null)
                {
                    //Debug.Log("we are trying to interact the card " + attachedCard.cardType + " with another");

                    if (focusedCard.CardInteraction(attachedCard))
                    {
                        //Debug.Log("successful interaction between " + attachedCard.cardType + " and " + focusedCard.cardType);

                        attachedCard.MouseLeft();
                        //attachedCard.gameObject.layer = 8;
                        attachedCard = null;
                    }
                    else
                        attachedCard.GetComponent<Animator>().SetTrigger("InvalidSelection");
                }
                else
                {
                    //Debug.Log("Dropping the card here");
                    Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                    RaycastHit rayHit;

                    if (Physics.Raycast(ray, out rayHit, RAY_LENGTH, tableMask))
                    {
                        attachedCard.transform.position = new Vector3(rayHit.point.x, ON_TABLE_CARD_OFFSET, rayHit.point.z);
                        attachedCard.MouseLeft();
                        attachedCard.gameObject.layer = 8;
                        attachedCard = null;
                    }
                }
                audioManager.PlaySound(CardBank.instance.audioClips[1], false);
            }
        }
    }

    //USed to interact with a card 
    private void InteractWithCard()
    {
        // We can only interact with cards if we do not have a card atatched to the mouse.
        if (attachedCard == null && focusedCard != null)
        {
            if (Input.GetAxisRaw("Secondary Click") == 1 && cursorRightUp)
            {
                cursorRightUp = false;
                if(!focusedCard.CursorInteraction())
                    focusedCard.GetComponent<Animator>().SetTrigger("InvalidSelection");
            }
            // check to see if we should end interaction with the card we were interacting with.
            else if (Input.GetAxisRaw("Secondary Click") == 0 && !cursorRightUp)
            {
                focusedCard.beingInteractedWith = false;
                switch (focusedCard.cardType)
                {
                    case CardBehaviour.CardType.OnionBase:
                        focusedCard.audioManager.StopSound();
                        break;
                    case CardBehaviour.CardType.PlateDirty:
                        focusedCard.audioManager.StopSound();
                        break;
                    default:
                        break;
                }
            }
        }
    }

    // Used to checkl to see if the cursor is up. If so then we set cursor up to true;
    private void CursorIsUpCheck()
    {
        // This cursor up lock is used to ensure that we only check the frame the mosue button was clicked.
        if (Input.GetAxisRaw("Primary Click") == 0 && !cursorLeftUp)
            cursorLeftUp = true;
        if (Input.GetAxisRaw("Secondary Click") == 0 && !cursorRightUp)
            cursorRightUp = true;
    }
}
