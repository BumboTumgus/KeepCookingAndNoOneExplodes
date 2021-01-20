using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBehaviour : MonoBehaviour
{
    public enum CardType { OnionBase, OnionStinky, OnionBag, Garbage, GarbageFull, Dumpster, Trashbag, Rat, RatTrap, StorageCloset, CuttingBoard, OnionSliced, Pot, Stove, OnionCooked, PotWithCookedFood, BurntFood, PotWithBurntFood,
    Fire, FireExtinguisher, Service, Sink, Plate, PlateDirty, PassLeft, PassRight}
    public CardType cardType;

    public bool cardLocked = false;
    [HideInInspector] public bool beingInteractedWith = false;

    public List<CardBehaviour> connectedCards = new List<CardBehaviour>();
    [HideInInspector] public AudioManager audioManager;

    private Animator anim;

    private Color hoveredColor = new Color(242f / 255f, 255f / 255f, 43f / 255f, 150f / 255f);
    private Color selectedColor = new Color(43f / 255f, 221f / 255f, 255f / 255f, 150f / 255f);

    private float urgency = 0;

    [SerializeField] private BarManager connectedProgressBar;
    private float progress = 0;
    [SerializeField] private float progressTarget = 0;

    [HideInInspector] public float cardCounter = 0;
    private float cardTargetCounter = 0;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        if (cardType == CardType.OnionStinky || cardType == CardType.Trashbag || cardType == CardType.GarbageFull || cardType == CardType.Rat || cardType == CardType.Fire)
            anim.SetBool("DealWithMe", true);
        if (cardType == CardType.Rat)
            cardTargetCounter = Random.Range(GameManager.instance.ratToAnotherRatMin, GameManager.instance.ratToAnotherRatMax);
        if (cardType == CardType.Fire)
        {
            cardTargetCounter = Random.Range(GameManager.instance.fireToMoreFireMin, GameManager.instance.fireToMoreFireMax);
            progressTarget = GameManager.instance.fireExtinguish;
        }
    }

    private void Start()
    {
        // If we have  a bar canvas, we know there will be a bar inside for us to initialize
        if (transform.Find("BarCanvas"))
        {
            connectedProgressBar = transform.Find("BarCanvas").GetComponentInChildren<BarManager>();
            connectedProgressBar.gameObject.SetActive(false);

            switch (cardType)
            {
                case CardType.OnionBase:
                    connectedProgressBar.Initialize(GameManager.instance.onionToChoppedOnion, 0, 0);
                    break;
                case CardType.Pot:
                    connectedProgressBar.Initialize(progressTarget, 0, 0);
                    break;
                case CardType.Fire:
                    connectedProgressBar.Initialize(GameManager.instance.fireExtinguish, 0, 0);
                    break;
                case CardType.PlateDirty:
                    connectedProgressBar.Initialize(GameManager.instance.dishToCleanDish, 0, 0);
                    break;
                default:
                    break;
            }
        }

        audioManager = GetComponent<AudioManager>();
    }

    private void Update()
    {
        switch (cardType)
        {
            case CardType.OnionBase:
                if (!beingInteractedWith)
                {
                    cardCounter += Time.deltaTime;
                    urgency = cardCounter / GameManager.instance.onionToStinkyOnion + 0.2f;
                    anim.SetFloat("Urgency", urgency);

                    if (cardCounter >= GameManager.instance.onionToStinkyOnion && !cardLocked)
                    {
                        cardCounter = 0;
                        TransformThisObject(CardType.OnionStinky);
                    }
                }
                // If the card is locked, dont show update the bar
                else if (!cardLocked)
                {
                    progress += Time.deltaTime;

                    connectedProgressBar.gameObject.SetActive(true);
                    connectedProgressBar.SetValue(progress);
                    if (progress >= GameManager.instance.onionToChoppedOnion)
                    {
                        cardCounter = 0;
                        progress = 0;
                        TransformThisObject(CardType.OnionSliced);
                    }
                }
                break;
            case CardType.OnionSliced:
                cardCounter += Time.deltaTime;
                urgency = cardCounter / GameManager.instance.onionToStinkyOnion + 0.2f;
                anim.SetFloat("Urgency", urgency);

                if (cardCounter >= GameManager.instance.onionToStinkyOnion && !cardLocked)
                {
                    cardCounter = 0;
                    TransformThisObject(CardType.OnionStinky);
                }
                break;
            case CardType.OnionStinky:
                cardCounter += Time.deltaTime;
                urgency = cardCounter / GameManager.instance.onionToStinkyOnion;
                anim.SetFloat("Urgency", urgency);

                if (cardCounter >= GameManager.instance.stinkyOnionToRat && !cardLocked)
                {
                    cardCounter = 0;
                    TransformThisObject(CardType.Rat);
                    audioManager.PlaySound(CardBank.instance.audioClips[18], false);
                }
                break;
            case CardType.Trashbag:
                cardCounter += Time.deltaTime;
                urgency = cardCounter / GameManager.instance.onionToStinkyOnion;
                anim.SetFloat("Urgency", urgency);

                if (cardCounter >= GameManager.instance.trashbagToRat && !cardLocked)
                {
                    cardCounter = 0;
                    GameObject rat = Instantiate(CardBank.instance.cards[3], transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                    audioManager.PlaySound(CardBank.instance.audioClips[18], false);
                    rat.GetComponent<CardBehaviour>().SpawnThisObjectFromDeck();
                }
                break;
            case CardType.Rat:
                cardCounter += Time.deltaTime;
                urgency = cardCounter / GameManager.instance.onionToStinkyOnion;
                anim.SetFloat("Urgency", urgency);

                if (cardCounter >= cardTargetCounter && !cardLocked)
                {
                    cardCounter = 0;
                    GameObject rat = Instantiate(CardBank.instance.cards[3], transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                    audioManager.PlaySound(CardBank.instance.audioClips[18], false);
                    rat.GetComponent<CardBehaviour>().SpawnThisObjectFromDeck();
                }
                break;
            case CardType.Pot:
                if(beingInteractedWith && cardCounter > 0)
                {
                    progress += Time.deltaTime;

                    connectedProgressBar.gameObject.SetActive(true);
                    connectedProgressBar.SetValue(progress);
                    if (progress >= progressTarget)
                    {
                        progress = 0;
                        progressTarget = GameManager.instance.soupCookTime / 2;
                        anim.SetBool("DealWithMe", true);
                        anim.SetTrigger("TransformToNewCard");
                        cardType = CardType.PotWithCookedFood;

                        foreach(CardBehaviour card in connectedCards)
                        {
                            if (card.cardType == CardType.OnionSliced)
                                card.TransformThisObject(CardType.OnionCooked);
                            // set all the food in the pot to cooked
                        }
                        connectedProgressBar.gameObject.SetActive(false);
                        Debug.Log("this is done cooking");
                    }
                }
                break;
            case CardType.PotWithCookedFood:
                if (beingInteractedWith)
                {
                    progress += Time.deltaTime;

                    urgency = progress / progressTarget;
                    anim.SetFloat("Urgency", urgency);

                    if(progress >= progressTarget)
                    {
                        progress = 0;
                        anim.SetTrigger("TransformToNewCard");
                        cardType = CardType.PotWithBurntFood;

                        for (int index = 0; index < 2; index++)
                        {
                            GameObject fire = Instantiate(CardBank.instance.cards[5], transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                            fire.GetComponent<CardBehaviour>().SpawnThisObjectFromDeck();
                            audioManager.PlaySound(CardBank.instance.audioClips[9], false);
                        }

                        foreach (CardBehaviour card in connectedCards)
                        {
                            if (card.cardType == CardType.OnionCooked)
                                card.TransformThisObject(CardType.BurntFood);
                            // set all the food in the pot to cooked
                        }
                    }
                }
                break;
            case CardType.PotWithBurntFood:
                if(beingInteractedWith)
                {
                    progress += Time.deltaTime;

                    urgency = progress / progressTarget;
                    anim.SetFloat("Urgency", urgency);

                    if (progress >= progressTarget)
                    {
                        progress = 0;

                        GameObject fire = Instantiate(CardBank.instance.cards[5], transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                        fire.GetComponent<CardBehaviour>().SpawnThisObjectFromDeck();
                        audioManager.PlaySound(CardBank.instance.audioClips[9], false);
                    }

                }
                break;
            case CardType.Fire:
                if(beingInteractedWith)
                {
                    //Debug.Log("this fire is adding to its progress bar, current reading: " + progress + " / " + progressTarget);
                    progress += Time.deltaTime;
                    connectedProgressBar.SetValue(progress);

                    if(progress >= progressTarget)
                    {
                        cardLocked = true;
                        ThrowAwayThisObject();
                    }
                }
                else
                {
                    cardCounter += Time.deltaTime;

                    urgency = cardCounter / cardTargetCounter;
                    anim.SetFloat("Urgency", urgency);

                    if (cardCounter >= cardTargetCounter)
                    {
                        cardCounter = 0;
                        GameObject fire = Instantiate(CardBank.instance.cards[5], transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                        fire.GetComponent<CardBehaviour>().SpawnThisObjectFromDeck();
                        audioManager.PlaySound(CardBank.instance.audioClips[9], false);
                    }
                }
                break;
            case CardType.Service:
                if(cardCounter > 0)
                {
                    progress += Time.deltaTime;

                    if(progress >= GameManager.instance.serviceDishReturn)
                    {
                        progress = 0;
                        cardCounter--;
                        GameObject plate = Instantiate(CardBank.instance.cards[7], transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                        plate.GetComponent<CardBehaviour>().SpawnThisObjectFromDeck();
                        audioManager.PlaySound(CardBank.instance.audioClips[15], false);
                    }
                }
                break;
            case CardType.PlateDirty:
                if (!cardLocked && beingInteractedWith)
                {
                    progress += Time.deltaTime;

                    connectedProgressBar.gameObject.SetActive(true);
                    connectedProgressBar.SetValue(progress);

                    if (progress >= GameManager.instance.dishToCleanDish)
                    {
                        connectedProgressBar.gameObject.SetActive(false);
                        TransformThisObject(CardType.Plate);
                    }
                }
                break;
            default:
                break;
        }
    }

    // Called When the cursor hovers over this card. if the card is not locked, show the outline.
    public void MouseHovering()
    {
        //Debug.Log(gameObject.name + " has been highlighted");
        if(!cardLocked)
        {
            GetComponentInChildren<MeshRenderer>().material.SetFloat("_Outline", 0.1f);
            GetComponentInChildren<MeshRenderer>().material.SetColor("_OutlineColor", hoveredColor);
        }
    }

    // Called when the mouse leaves this card, reset our material to the standard one.
    public void MouseLeft()
    {
        //Debug.Log(gameObject.name + " has been un highlithed");
        GetComponentInChildren<MeshRenderer>().material.SetFloat("_Outline", 0.0f);
        if(cardType == CardType.Fire)
        {
            //Debug.Log("this is fire we left we shoudl set it as not being interacted with;");
            beingInteractedWith = false;
            audioManager.PlaySound(CardBank.instance.audioClips[8], true);
        }
        else if(cardType == CardType.OnionBase || cardType == CardType.PlateDirty)
        {
            beingInteractedWith = false;
            audioManager.StopSound();
        }
    }

    // Called when this object is selected to be attached to a player's cursor
    public void MouseAttach()
    {
        GetComponentInChildren<MeshRenderer>().material.SetFloat("_Outline", 0.1f);
        GetComponentInChildren<MeshRenderer>().material.SetColor("_OutlineColor", selectedColor);

        if (connectedCards.Count > 0)
        {
            // Here we need to check if this object has any connected cards to it, if so we need to interact with those cards based on what type of card we are.
            switch (cardType)
            {
                case CardType.OnionBase:
                    connectedCards[0].cardLocked = false;
                    break;
                case CardType.OnionStinky:
                    connectedCards[0].cardLocked = false;
                    break;
                case CardType.Rat:
                    connectedCards[0].cardLocked = false;
                    break;
                case CardType.Pot:
                    RemoveCardLockOfType(CardType.Stove);
                    RemoveConnectedCardOfType(CardType.Stove);
                    beingInteractedWith = false;
                    audioManager.StopSound();
                    break;
                case CardType.PotWithCookedFood:
                    RemoveCardLockOfType(CardType.Stove);
                    RemoveConnectedCardOfType(CardType.Stove);
                    anim.SetBool("DealWithMe", false);
                    beingInteractedWith = false;
                    audioManager.StopSound();
                    break;
                case CardType.PotWithBurntFood:
                    RemoveCardLockOfType(CardType.Stove);
                    RemoveConnectedCardOfType(CardType.Stove);
                    anim.SetBool("DealWithMe", false);
                    beingInteractedWith = false;
                    audioManager.StopSound();
                    break;
                case CardType.PlateDirty:
                    RemoveCardLockOfType(CardType.Sink);
                    RemoveConnectedCardOfType(CardType.Sink);
                    break;
                case CardType.Plate:
                    RemoveCardLockOfType(CardType.Sink);
                    RemoveConnectedCardOfType(CardType.Sink);
                    break;
                default:
                    break;
            }
        }
    }

    // Used to interact this card with another. return a bool if the card interaction was a success or not.
    public bool CardInteraction(CardBehaviour cardOnMouse)
    {
        //Debug.Log(" ia m the card: " + cardType + " and " + cardOnMouse.cardType + " is inbteracting with me");
        bool successfulInteraction = false;

        switch (cardType)
        {
            case CardType.OnionBase:
                break;
            case CardType.OnionStinky:
                break;
            case CardType.OnionBag:
                break;
            case CardType.Garbage:
                if (cardOnMouse.cardType == CardType.OnionBase || cardOnMouse.cardType == CardType.OnionStinky || cardOnMouse.cardType == CardType.OnionSliced || cardOnMouse.cardType == CardType.RatTrap || cardOnMouse.cardType == CardType.FireExtinguisher)
                {
                    cardOnMouse.transform.position = transform.position + Vector3.up * 0.05f;
                    cardOnMouse.ThrowAwayThisObject();

                    // Add a count to our counter, if we past the maximum amount for a garbage, switch us to a full garbage.
                    cardCounter++;
                    if (cardCounter >= 5)
                    {
                        cardCounter = 0;
                        TransformThisObject(CardType.GarbageFull);
                    }

                    audioManager.PlaySound(CardBank.instance.audioClips[6], false);
                    successfulInteraction = true;
                }
                // If we have a pot with soemthing in it, remvoe all suitable objects and bin them.
                if ((cardOnMouse.cardType == CardType.Pot || cardOnMouse.cardType == CardType.PotWithCookedFood || (cardOnMouse.cardType == CardType.PotWithBurntFood) && cardOnMouse.cardCounter > 0) || cardOnMouse.cardType == CardType.Plate)
                {
                    cardOnMouse.cardCounter = 0;
                    List<CardBehaviour> cardsToRemove = new List<CardBehaviour>();

                    foreach(CardBehaviour card in cardOnMouse.connectedCards)
                        if (card.cardType == CardType.OnionSliced || card.cardType == CardType.OnionCooked || card.cardType == CardType.BurntFood)
                            cardsToRemove.Add(card);

                    // Add a count to our counter, if we past the maximum amount for a garbage, switch us to a full garbage.
                    cardCounter += cardsToRemove.Count;
                    if (cardCounter >= 5)
                    {
                        cardCounter = 0;
                        TransformThisObject(CardType.GarbageFull);
                    }

                    foreach (CardBehaviour card in cardsToRemove)
                    {
                        //Debug.Log(" we have removed " + card.gameObject.name + " from our list");
                        cardOnMouse.connectedCards.Remove(card);

                        card.transform.parent = null;
                        card.transform.position = transform.position + Vector3.up * 0.05f;
                        card.ThrowAwayThisObject();
                    }

                    if(cardOnMouse.cardType == CardType.Pot || cardOnMouse.cardType == CardType.PotWithCookedFood || cardOnMouse.cardType == CardType.PotWithBurntFood)
                        cardOnMouse.cardType = CardType.Pot;
                    cardOnMouse.progressTarget = 0;
                    cardOnMouse.progress = 0;
                    audioManager.PlaySound(CardBank.instance.audioClips[6], false);
                }
                break;
            case CardType.Dumpster:
                if (cardOnMouse.cardType == CardType.Trashbag)
                {
                    cardOnMouse.transform.position = transform.position + Vector3.up * 0.05f;
                    cardOnMouse.ThrowAwayThisObject();
                    audioManager.PlaySound(CardBank.instance.audioClips[6], false);

                    successfulInteraction = true;
                }
                break;
            case CardType.RatTrap:
                if (cardOnMouse.cardType == CardType.Rat)
                {
                    cardOnMouse.transform.position = transform.position + Vector3.up * 0.05f;
                    cardOnMouse.ThrowAwayThisObject();
                    ThrowAwayThisObject();
                    audioManager.PlaySound(CardBank.instance.audioClips[16], false);

                    successfulInteraction = true;
                }
                break;
            case CardType.Rat:
                if (cardOnMouse.cardType == CardType.RatTrap)
                {
                    cardOnMouse.transform.position = transform.position + Vector3.up * 0.05f;
                    cardOnMouse.ThrowAwayThisObject();
                    ThrowAwayThisObject();
                    audioManager.PlaySound(CardBank.instance.audioClips[16], false);

                    successfulInteraction = true;
                }
                break;
            case CardType.CuttingBoard:
                if(cardOnMouse.cardType == CardType.OnionBase)
                {
                    cardOnMouse.transform.position = transform.position + Vector3.up * 0.05f;
                    cardOnMouse.anim.SetBool("OnAnotherObject", true);
                    cardOnMouse.anim.SetTrigger("PlacedOnOtherCard");
                    cardOnMouse.gameObject.layer = 8;
                    cardOnMouse.connectedCards.Add(this);

                    cardLocked = true;
                    successfulInteraction = true;
                }
                break;
            case CardType.Stove:
                if(cardOnMouse.cardType == CardType.Pot)
                {
                    cardOnMouse.transform.position = transform.position + Vector3.up * 0.05f;
                    cardOnMouse.anim.SetBool("OnAnotherObject", true);
                    cardOnMouse.anim.SetTrigger("PlacedOnOtherCard");
                    cardOnMouse.gameObject.layer = 8;
                    cardOnMouse.connectedCards.Add(this);
                    cardOnMouse.beingInteractedWith = true;
                    cardOnMouse.audioManager.PlaySound(CardBank.instance.audioClips[11], true);

                    cardLocked = true;
                    successfulInteraction = true;
                }
                break;
            case CardType.Pot:
                if(cardOnMouse.cardType == CardType.OnionSliced && cardCounter < 3)
                {
                    cardCounter++;
                    progressTarget = GameManager.instance.soupCookTime;
                    if (cardCounter > 1)
                        progressTarget += GameManager.instance.soupCookTime * 0.5f * (cardCounter - 1);

                    connectedProgressBar.Initialize(progressTarget, 0, progress);

                    cardOnMouse.connectedCards.Add(this);
                    connectedCards.Add(cardOnMouse);

                    cardOnMouse.anim.SetTrigger("PlacedInOtherCard");
                    cardOnMouse.PlaceThisObjectInAnotherObject(this);

                    cardOnMouse.cardLocked = true;
                    successfulInteraction = true;
                }
                break;
            case CardType.PotWithCookedFood:
                if(cardOnMouse.cardType == CardType.OnionSliced && cardCounter < 3)
                {
                    cardCounter++;
                    progress = 0;

                    progressTarget = GameManager.instance.soupCookTime;
                    if (cardCounter > 1)
                        progressTarget += GameManager.instance.soupCookTime * 0.5f * (cardCounter - 1);

                    //Debug.Log("our progress target before we take away anything is: " + progressTarget);

                    int cookedFoodItems = 0;
                    foreach(CardBehaviour card in connectedCards)
                        if (card.cardType == CardType.OnionCooked)
                            cookedFoodItems++;

                   // Debug.Log("our cooked food count is: " + cookedFoodItems);

                    if (cookedFoodItems > 0)
                        progressTarget -= GameManager.instance.soupCookTime;
                    //Debug.Log("our progress timer after the fuirst food take away is " + progressTarget);
                    if (cookedFoodItems > 1)
                        progressTarget -= GameManager.instance.soupCookTime * 0.5f * (cookedFoodItems - 1);
                    //Debug.Log("our final progress timer after the second food take away is " + progressTarget);

                    connectedProgressBar.Initialize(progressTarget, 0, progress);

                    cardOnMouse.connectedCards.Add(this);
                    connectedCards.Add(cardOnMouse);

                    cardOnMouse.anim.SetTrigger("PlacedInOtherCard");
                    cardOnMouse.PlaceThisObjectInAnotherObject(this);

                    cardOnMouse.cardLocked = true;
                    successfulInteraction = true;

                    cardType = CardType.Pot;
                    anim.SetBool("DealWithMe", false);
                }
                break;
            case CardType.Fire:
                if (cardOnMouse.cardType == CardType.FireExtinguisher)
                {
                    beingInteractedWith = true;
                    connectedProgressBar.gameObject.SetActive(true);
                    audioManager.PlaySound(CardBank.instance.audioClips[10], true);
                }
                break;
            case CardType.Plate:
                if (cardOnMouse.cardType == CardType.PotWithCookedFood && connectedCards.Count + cardOnMouse.connectedCards.Count <= 3)
                {
                    //Debug.Log("transfering food over");
                    cardOnMouse.cardType = CardType.Pot;
                    List<CardBehaviour> cardsToRemove = new List<CardBehaviour>();

                    foreach(CardBehaviour card in cardOnMouse.connectedCards)
                    {
                        cardCounter++;
                        connectedCards.Add(card);
                        cardsToRemove.Add(card);
                        card.PlaceThisObjectInAnotherObject(this);
                    }

                    foreach (CardBehaviour card in cardsToRemove)
                        cardOnMouse.connectedCards.Remove(card);
                    cardOnMouse.cardCounter = 0;

                }
                break;
            case CardType.Service:
                if (cardOnMouse.cardType == CardType.Plate && cardOnMouse.cardCounter > 0)
                {
                    cardCounter++;
                    cardOnMouse.transform.position = transform.position + Vector3.up * 0.05f;
                    GameManager.instance.Service(cardOnMouse.connectedCards);
                    cardOnMouse.ThrowAwayThisObject();

                    successfulInteraction = true;
                }
                break;
            case CardType.Sink:
                if (cardOnMouse.cardType == CardType.PlateDirty)
                {
                    cardOnMouse.transform.position = transform.position + Vector3.up * 0.05f;
                    cardOnMouse.anim.SetBool("OnAnotherObject", true);
                    cardOnMouse.anim.SetTrigger("PlacedOnOtherCard");
                    cardOnMouse.gameObject.layer = 8;
                    cardOnMouse.connectedCards.Add(this);

                    cardLocked = true;
                    successfulInteraction = true;
                }
                break;
            case CardType.PassLeft:
                if (cardOnMouse.cardType != CardType.Service && cardOnMouse.cardType != CardType.Stove && cardOnMouse.cardType != CardType.Sink &&
                    cardOnMouse.cardType != CardType.Dumpster && cardOnMouse.cardType != CardType.Garbage && cardOnMouse.cardType != CardType.GarbageFull &&
                    cardOnMouse.cardType != CardType.OnionBag && cardOnMouse.cardType != CardType.CuttingBoard &&
                    cardOnMouse.cardType != CardType.StorageCloset && cardOnMouse.cardType != CardType.Fire && cardOnMouse.cardType != CardType.Rat
                    )
                {
                    GameManager.instance.PassThisCard(true, cardOnMouse, Mathf.RoundToInt(cardCounter));
                    cardLocked = true;
                    cardOnMouse.gameObject.layer = 8;
                    successfulInteraction = true;
                }
                break;
            case CardType.PassRight:
                if (cardOnMouse.cardType != CardType.Service && cardOnMouse.cardType != CardType.Stove && cardOnMouse.cardType != CardType.Sink &&
                    cardOnMouse.cardType != CardType.Dumpster && cardOnMouse.cardType != CardType.Garbage && cardOnMouse.cardType != CardType.GarbageFull &&
                    cardOnMouse.cardType != CardType.OnionBag && cardOnMouse.cardType != CardType.CuttingBoard &&
                    cardOnMouse.cardType != CardType.StorageCloset && cardOnMouse.cardType != CardType.Fire && cardOnMouse.cardType != CardType.Rat
                    )
                {
                    GameManager.instance.PassThisCard(false, cardOnMouse, Mathf.RoundToInt(cardCounter));
                    cardLocked = true;
                    cardOnMouse.gameObject.layer = 8;
                    successfulInteraction = true;
                }
                break;
            default:
                break;
        }
        return successfulInteraction;
    }

    // used when the cursor interacts with this card
    public bool CursorInteraction()
    {
        bool successfulInteraction = false;
        //Debug.Log(" we are interacting with " + cardType);

        switch (cardType)
        {
            // If we interact with the opnion bag, spawn an onion.
            case CardType.OnionBag:
                successfulInteraction = true;
                if (Random.Range(0, 100) > 20)
                {
                    GameObject onion = Instantiate(CardBank.instance.cards[0], transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                    onion.GetComponent<CardBehaviour>().SpawnThisObjectFromDeck();
                }
                else
                {
                    GameObject onion = Instantiate(CardBank.instance.cards[1], transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                    onion.GetComponent<CardBehaviour>().SpawnThisObjectFromDeck();
                }

                break;
            // If we interact with the full garbage, create a garbage bag that must be removed and put in the dumpster and reset the garbage
            case CardType.GarbageFull:
                successfulInteraction = true;
                GameObject garbageBag = Instantiate(CardBank.instance.cards[2], transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                garbageBag.GetComponent<CardBehaviour>().SpawnThisObjectFromDeck();

                audioManager.PlaySound(CardBank.instance.audioClips[5], false);
                TransformThisObject(CardType.Garbage);

                break;
            // If we interact with the storage closet, spawn a rat trap.
            case CardType.StorageCloset:
                successfulInteraction = true;
                if (Random.Range(0, 100) > 20)
                {
                    GameObject ratTrap = Instantiate(CardBank.instance.cards[4], transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                    ratTrap.GetComponent<CardBehaviour>().SpawnThisObjectFromDeck();
                }
                else
                {
                    GameObject fireExtinguisher = Instantiate(CardBank.instance.cards[6], transform.position, Quaternion.Euler(new Vector3(0, 180, 0)));
                    fireExtinguisher.GetComponent<CardBehaviour>().SpawnThisObjectFromDeck();
                }

                break;
            case CardType.OnionBase:
                if (connectedCards.Count > 0 && connectedCards[0].cardType == CardType.CuttingBoard)
                {
                    beingInteractedWith = true;
                    successfulInteraction = true;
                    audioManager.PlaySound(CardBank.instance.audioClips[7], true);
                }
                break;
            case CardType.PlateDirty:
                if (connectedCards.Count > 0 && connectedCards[0].cardType == CardType.Sink)
                {
                    beingInteractedWith = true;
                    successfulInteraction = true;
                    audioManager.PlaySound(CardBank.instance.audioClips[14], true);
                }
                break;
            default:
                break;
        }
        return successfulInteraction;
    }

    // Used to call the garbage coroutine on this item.
    public void ThrowAwayThisObject()
    {
        StartCoroutine(ThrownInGarbage());
    }

    // Used when an object is spawned from a deck of cards
    public void SpawnThisObjectFromDeck()
    {
        StartCoroutine(SpawnedFromDeck());
    }

    // USed when we want to transform this card into a new card
    public void TransformThisObject(CardType newCard)
    {
        StartCoroutine(TransformCard(newCard));
    }

    // USed when i want to place a card within another card and parent it.
    public void PlaceThisObjectInAnotherObject(CardBehaviour otherCard)
    {
        StartCoroutine(PlaceCardInNewCard(otherCard));
    }
    
    // Used to make this card slide to a new location when we pass it off.
    public void SlideThisObjectIn(Vector3 position, bool fromLeft)
    {
        StartCoroutine(SlideCardIn(position, fromLeft));
    }

    // Used to remove the card lock off a certain card type in my connected cards.
    private void RemoveCardLockOfType(CardType selectedType)
    {
        if (connectedCards.Count > 0)
        {
            foreach (CardBehaviour card in connectedCards)
            {
                if (card.cardType == selectedType)
                {
                    card.cardLocked = false;
                    break;
                }
            }
        }
    }

    // USed to remvoe a connected card of this particular type
    private void RemoveConnectedCardOfType(CardType selectedType)
    {
        CardBehaviour cardToRemove = null;

        foreach(CardBehaviour card in connectedCards)
        {
            if(card.cardType == selectedType)
            {
                cardToRemove = card;
                break;
            }
        }
        if(cardToRemove)
            connectedCards.Remove(cardToRemove);
    }
    
    // Used when this card is thrown in the garbage
    private IEnumerator ThrownInGarbage()
    {
        anim.SetTrigger("Garbage");
        if(connectedProgressBar)
            connectedProgressBar.gameObject.SetActive(false);
        gameObject.layer = 2;
        cardLocked = true;
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }

    // The animation that is played when we are spawned from a deck.
    private IEnumerator SpawnedFromDeck()
    {
        anim.SetTrigger("SpawnedFromDeck");
        cardLocked = true;
        float currentTimer = 0f;
        float targetTimer = 0.5f;
        Vector3 targetPosition = transform.position + Vector3.forward * 0.45f + Vector3.right * Random.Range(-0.3f, 0.3f);

        yield return new WaitForEndOfFrame();

        if(cardType == CardType.Fire)
            audioManager.PlaySound(CardBank.instance.audioClips[8], true);
        else if(cardType == CardType.Rat)
            audioManager.PlaySound(CardBank.instance.audioClips[17], true);
        else
            audioManager.PlaySound(CardBank.instance.audioClips[2], false);

        while (currentTimer < targetTimer)
        {
            currentTimer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.1f);
            yield return new WaitForEndOfFrame();
        }

        transform.position = targetPosition;
        cardLocked = false;
    }

    // Plays the transform animation and switches this card into a new card
    private IEnumerator TransformCard(CardType newCard)
    {
        anim.SetTrigger("TransformToNewCard");
        audioManager.PlaySound(CardBank.instance.audioClips[3], false);
        cardLocked = true;

        if (connectedProgressBar)
            connectedProgressBar.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        cardType = newCard;
        switch (newCard)
        {
            case CardType.OnionStinky:
                GetComponentInChildren<MeshRenderer>().material = CardBank.instance.cardMats[0];
                anim.SetBool("DealWithMe", true);
                break;
            case CardType.Garbage:
                GetComponentInChildren<MeshRenderer>().material = CardBank.instance.cardMats[2];
                anim.SetBool("DealWithMe", false);
                break;
            case CardType.GarbageFull:
                GetComponentInChildren<MeshRenderer>().material = CardBank.instance.cardMats[1];
                anim.SetBool("DealWithMe", true);
                break;
            case CardType.Rat:
                GetComponentInChildren<MeshRenderer>().material = CardBank.instance.cardMats[3];
                cardTargetCounter = Random.Range(GameManager.instance.ratToAnotherRatMin, GameManager.instance.ratToAnotherRatMax);
                anim.SetBool("DealWithMe", true);
                audioManager.PlaySound(CardBank.instance.audioClips[17], true);
                break;
            case CardType.OnionSliced:
                GetComponentInChildren<MeshRenderer>().material = CardBank.instance.cardMats[4];
                break;
            case CardType.OnionCooked:
                GetComponentInChildren<MeshRenderer>().material = CardBank.instance.cardMats[5];
                break;
            case CardType.BurntFood:
                GetComponentInChildren<MeshRenderer>().material = CardBank.instance.cardMats[6];
                break;
            case CardType.Plate:
                GetComponentInChildren<MeshRenderer>().material = CardBank.instance.cardMats[7];
                break;
            default:
                break;
        }
        yield return new WaitForSeconds(0.5f);

        cardLocked = false;
    }

    // Places this card in another card, once placed the card is locked and cannot be removed until thrown out.
    private IEnumerator PlaceCardInNewCard(CardBehaviour otherCard)
    {
        transform.parent = otherCard.transform.Find("Card_Art");
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = new Vector3(4, 1000, 2.5f);
        Vector3 targetPosition = (Vector3.down * 1 * (otherCard.cardCounter - 1)) + (Vector3.forward * 0.3f * otherCard.cardCounter);
        Vector3 startingPosition = targetPosition + Vector3.forward * 0.75f;

        float currentTimer = 0f;
        float targetTimer = 1f;
        transform.localPosition = startingPosition;

        while(currentTimer < targetTimer)
        {
            currentTimer += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, 0.07f);
            yield return new WaitForEndOfFrame();
        }

        yield return null;
        transform.localPosition = targetPosition;
    }

    //SLides this card in to the target location from the target direction.
    private IEnumerator SlideCardIn(Vector3 position, bool fromLeft)
    {
        Vector3 targetPosition = position;

        cardLocked = true;
        float currentTimer = 0f;
        float targetTimer = 0.5f;

        if(fromLeft)
            transform.position = position + Vector3.right * -0.5f;
        else
            transform.position = position + Vector3.right * 0.5f;

        while (currentTimer < targetTimer)
        {
            currentTimer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.07f);
            yield return new WaitForEndOfFrame();
        }

        transform.position = targetPosition;
        cardLocked = false;
    }
}
