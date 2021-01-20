using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUiManager : MonoBehaviour
{
    [HideInInspector] public Text scoreText;
    [HideInInspector] public Text timerText;

    [HideInInspector] public RectTransform orderContainer;
    public GameObject orderSlip;

    private const float ORDER_WIDTH = 250f;

    // Start is called before the first frame update
    void Start()
    {
        scoreText = transform.Find("ScoreCounter").GetComponent<Text>();
        timerText = transform.Find("RoundTimer").GetComponent<Text>();
        orderContainer = transform.Find("OrderPanel").GetComponent<RectTransform>();

        scoreText.text = "0$";
        timerText.text = "0";
        orderContainer.sizeDelta = new Vector2(0, orderContainer.sizeDelta.y);
    }

    //USed to add a new order ticket to our ui.
    public void AddNewOrderTicket(Order.OrderType orderType)
    {
        GameObject newOrder = Instantiate(orderSlip, orderContainer);

        orderContainer.sizeDelta = new Vector2(orderContainer.childCount * ORDER_WIDTH, orderContainer.sizeDelta.y);
        newOrder.transform.Find("OrderTicket").GetComponentInChildren<BarManager>().Initialize(1, 0, 1);

        switch (orderType)
        {
            case Order.OrderType.OnionSoup:
                newOrder.transform.Find("OrderTicket").GetComponentInChildren<Text>().text = "Onion Soup";
                break;
            case Order.OrderType.TomatoSoup:
                newOrder.transform.Find("OrderTicket").GetComponentInChildren<Text>().text = "Tomato Soup";
                break;
            case Order.OrderType.MushroomSoup:
                newOrder.transform.Find("OrderTicket").GetComponentInChildren<Text>().text = "Mushroom Soup";
                break;
            default:
                break;
        }
    }

    // USed to remove an order of this type with the lowest countdown
    public void RemoveOrderTicket(Order.OrderType orderType)
    {
        for(int childIndex = 0; childIndex < orderContainer.childCount; childIndex ++)
        {
            bool orderDestroyed = false;
            switch (orderType)
            {
                case Order.OrderType.OnionSoup:
                    if (orderContainer.GetChild(childIndex).Find("OrderTicket").GetComponentInChildren<Text>().text == "Onion Soup")
                    {
                        //Debug.Log("deleting first onion soup");
                        orderContainer.sizeDelta = new Vector2((orderContainer.childCount - 1) * ORDER_WIDTH, orderContainer.sizeDelta.y);
                        Destroy(orderContainer.GetChild(childIndex).gameObject);
                        orderDestroyed = true;
                    }
                    break;
                case Order.OrderType.TomatoSoup:
                    if (orderContainer.GetChild(childIndex).Find("OrderTicket").GetComponentInChildren<Text>().text == "Tomato Soup")
                    {
                        //Debug.Log("deelting first tomato soup");
                        orderContainer.sizeDelta = new Vector2((orderContainer.childCount - 1) * ORDER_WIDTH, orderContainer.sizeDelta.y);
                        Destroy(orderContainer.GetChild(childIndex).gameObject);
                        orderDestroyed = true;
                    }
                    break;
                case Order.OrderType.MushroomSoup:
                    if (orderContainer.GetChild(childIndex).Find("OrderTicket").GetComponentInChildren<Text>().text == "Mushroom Soup")
                    {
                        //Debug.Log("deleting first mushroom soup");
                        orderContainer.sizeDelta = new Vector2((orderContainer.childCount - 1) * ORDER_WIDTH, orderContainer.sizeDelta.y);
                        Destroy(orderContainer.GetChild(childIndex).gameObject);
                        orderDestroyed = true;
                    }
                    break;
                default:
                    break;
            }

            if (orderDestroyed)
            {
                break;
            }
        }
    }
}
