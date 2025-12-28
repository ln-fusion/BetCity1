using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEvent
{
    public EventType actionType;


}
public enum EventType
{
    Battle,//战斗
    Shop,//商店
    Warehouse,//仓库
    Dialogue,//事件
    Home,//休息
}
