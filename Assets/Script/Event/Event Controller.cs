using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class MoveEvent : UnityEvent<Vector2>{}
public class LookEvent : UnityEvent<Vector2>{}
public class AttackGunEvent : UnityEvent{}
public class ThrowEvent : UnityEvent{}
public class BuffEvent : UnityEvent<ItemNetworkSerializable,ServerRpcParams>{}
public class DamageEvent : UnityEvent<float>{}

