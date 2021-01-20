using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int playerCount = 3;

    public List<Order> orders = new List<Order>();
    public List<Order.OrderType> availibleOrders = new List<Order.OrderType>();
    
    public List<PlayerUiManager> playerUIs = new List<PlayerUiManager>();
    public int score = 0;
    public float timer = 240f;

    [HideInInspector] public float onionToStinkyOnion = 160f;
    [HideInInspector] public float stinkyOnionToRat = 7f;
    [HideInInspector] public float trashbagToRat = 12f;
    [HideInInspector] public float ratToAnotherRatMin = 7f;
    [HideInInspector] public float ratToAnotherRatMax = 13f;

    [HideInInspector] public float fireToMoreFireMin = 5f;
    [HideInInspector] public float fireToMoreFireMax = 20f;
    [HideInInspector] public float fireExtinguish = 2f;

    [HideInInspector] public float onionToChoppedOnion = 5f;
    [HideInInspector] public float soupCookTime = 8f;

    [HideInInspector] public float serviceDishReturn = 10f;
    [HideInInspector] public float dishToCleanDish = 5f;

    [HideInInspector] public int maximumOrderAmount = 6;
    [HideInInspector] public int minimumOrderAmount = 3;

    public GameObject player;
    public GameObject table;
    public GameObject playerCanvas;

    private float targetOrderTimer = 20f;
    private float currentOrderTimer = 0f;

    private AudioManager audioManager;

    private const float SOUP_ORDER_TIMER = 60f;
    private const int SOUP_BASE_VALUE = 30;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
        currentOrderTimer = targetOrderTimer;
        availibleOrders = new List<Order.OrderType>() { Order.OrderType.OnionSoup};
        audioManager = GetComponent<AudioManager>();

        InitializePlayers();
    }
    
    private void Update()
    {
        timer -= Time.deltaTime;
        changeTimers();

        // Adding time to each order logic
        for(int orderIndex = 0; orderIndex < orders.Count; orderIndex++)
        {
            orders[orderIndex].currentOrderTimer += Time.deltaTime;
            foreach (PlayerUiManager ui in playerUIs)
                ui.orderContainer.GetChild(orderIndex).Find("OrderTicket").GetComponentInChildren<BarManager>().SetValue( 1 - (orders[orderIndex].currentOrderTimer / orders[orderIndex].targetOrderTimer));

            // If we fail the order, remove it from the order list, lower the index by one, then make us lose money.
            if(orders[orderIndex].currentOrderTimer >= orders[orderIndex].targetOrderTimer)
            {
                RemoveOrderAtIndex(orderIndex, true);
                orderIndex--;
            }
        }

        // SPawning orders logic.
        currentOrderTimer += Time.deltaTime;
        if(currentOrderTimer >= targetOrderTimer && orders.Count < 6)
        {
            currentOrderTimer = 0;
            AddNewOrder();
        }

        // Used to test scores
        if (Input.GetKeyDown(KeyCode.P))
            addScores(100);
        if (Input.GetKeyDown(KeyCode.O))
            AddNewOrder();
    }

    // USed to initialize the game, create players for every player and spawn in the players cards.
    private void InitializePlayers()
    {
        // Add in all the boads, players and cameras and connect them to themselves
        for(int playerIndex = 0; playerIndex < playerCount; playerIndex++)
        {
            Instantiate(table, Vector3.zero + Vector3.right * 6 * playerIndex, Quaternion.identity);
            GameObject currentPlayer = Instantiate(player, new Vector3(0, 2.5f, -1.36f) + Vector3.right * 6 * playerIndex, Quaternion.Euler(new Vector3(80, 0, 0)));
            Canvas currentCanvas = Instantiate(playerCanvas, Vector3.zero, Quaternion.identity).GetComponent<Canvas>();

            currentPlayer.GetComponent<Camera>().targetDisplay = playerIndex;
            
            currentCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            currentCanvas.worldCamera = currentPlayer.GetComponent<Camera>();

            if (playerCount > 1)
            {
                CardBehaviour rightPassCard = Instantiate(GetComponent<CardBank>().cards[8], new Vector3(1.85f, 0.55f, -1f) + Vector3.right * 6 * playerIndex, Quaternion.Euler(new Vector3(0, 180f, 0))).GetComponent<CardBehaviour>();
                CardBehaviour leftPassCard = Instantiate(GetComponent<CardBank>().cards[9], new Vector3(-1.85f, 0.55f, -1f) + Vector3.right * 6 * playerIndex, Quaternion.Euler(new Vector3(0, 180f, 0))).GetComponent<CardBehaviour>();
                rightPassCard.cardCounter = playerIndex;
                leftPassCard.cardCounter = playerIndex;
            }

            playerUIs.Add(currentCanvas.GetComponent<PlayerUiManager>());
        }
    }

    // Used to add a new order to the stack.
    private void AddNewOrder()
    {
        //Debug.Log("we added a new order");

        // Create the order
        Order orderToAdd = new Order();
        Order.OrderType orderToAddType = availibleOrders[Random.Range(0, availibleOrders.Count)];
        switch (orderToAddType)
        {
            case Order.OrderType.OnionSoup:
                orderToAdd = new Order(Order.OrderType.OnionSoup, SOUP_ORDER_TIMER, SOUP_BASE_VALUE);
                break;
            case Order.OrderType.TomatoSoup:
                orderToAdd = new Order(Order.OrderType.TomatoSoup, SOUP_ORDER_TIMER, SOUP_BASE_VALUE);
                break;
            case Order.OrderType.MushroomSoup:
                orderToAdd = new Order(Order.OrderType.MushroomSoup, SOUP_ORDER_TIMER, SOUP_BASE_VALUE);
                break;
            default:
                break;
        }

        // Add the order to the list
        orders.Add(orderToAdd);

        // Create the order slide in Ui for the players.
        foreach (PlayerUiManager ui in playerUIs)
            ui.AddNewOrderTicket(orderToAddType);
    }

    // Used to remove an order at this index.
    private void RemoveOrderAtIndex(int index, bool failure)
    {
        //Debug.Log("destroying order at index " + index);
        if (failure)
        {
            addScores(orders[index].orderValue * -1);
            audioManager.PlaySound(CardBank.instance.audioClips[13], false);
        }
        else
        {
            addScores(orders[index].orderValue + Mathf.RoundToInt(orders[index].orderValue * orders[index].currentOrderTimer / orders[index].targetOrderTimer));
            audioManager.PlaySound(CardBank.instance.audioClips[12], false);
        }

        foreach (PlayerUiManager ui in playerUIs)
            ui.RemoveOrderTicket(orders[index].myOrderType);

        orders.Remove(orders[index]);
    }

    // Used to serve the food, then compare to see if we have a match for one of our orders.
    public void Service(List<CardBehaviour> foodToServe)
    {
        Debug.Log("we have served food: ");
        Order.OrderType typeOfFoodServed = Order.OrderType.OnionSoup;
        bool orderMatch = true;

        // check to see if we match na food type.
        //Debug.Log(foodToServe.Count);
        if (foodToServe.Count == 3 && foodToServe[0].cardType == CardBehaviour.CardType.OnionCooked && foodToServe[1].cardType == CardBehaviour.CardType.OnionCooked && foodToServe[2].cardType == CardBehaviour.CardType.OnionCooked)
            typeOfFoodServed = Order.OrderType.OnionSoup;
        else
            orderMatch = false;

        // If we did, check to see if any of the active nroders are of this type.
        if (orderMatch)
        {
            for (int index = 0; index < orders.Count; index++)
            {
                // If so, remove the order and add us some money.
                if (orders[index].myOrderType == typeOfFoodServed)
                {
                    RemoveOrderAtIndex(index, false);
                    break;
                }
            }
        }
    }

    // Used to add scores to all player Uis
    private void addScores(int amount)
    {
        score += amount;
        foreach (PlayerUiManager ui in playerUIs)
            ui.scoreText.text = score + "$";
    }

    // USed to change the timer for all players Uis
    private void changeTimers()
    {
        string timeLeft = string.Format("{0}:{1:00}", Mathf.Floor(timer / 60), timer % 60);
        foreach (PlayerUiManager ui in playerUIs)
            ui.timerText.text = timeLeft;
    }

    // Used to pass a card to the right or left
    public void PassThisCard(bool left, CardBehaviour card, int playerIndex)
    {
        int newPlayerIndex = playerIndex;

        if (left)
            newPlayerIndex--;
        else
            newPlayerIndex++;

        if (newPlayerIndex < 0)
            newPlayerIndex = playerCount - 1;
        else if (newPlayerIndex > playerCount - 1)
            newPlayerIndex = 0;

        // Pass this card to the player in question.
        if(left)
            card.SlideThisObjectIn(new Vector3(1.85f, 0.55f, -0.5f)+ (Vector3.right * newPlayerIndex * 6f), false);
        else
            card.SlideThisObjectIn(new Vector3(-1.85f, 0.55f, -0.5f) + (Vector3.right * newPlayerIndex * 6f), true);

    }
}
