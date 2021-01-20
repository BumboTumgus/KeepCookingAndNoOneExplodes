using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public enum OrderType { OnionSoup, TomatoSoup, MushroomSoup}
    public OrderType myOrderType;

    public float currentOrderTimer = 0f;
    public float targetOrderTimer = 45f;

    public int orderValue = 30;

    public Order()
    {
    }

    public Order(OrderType MyOrderType, float TargetOrderTimer, int OrderValue)
    {
        myOrderType = MyOrderType;
        targetOrderTimer = TargetOrderTimer;
        orderValue = OrderValue;
    }
}
