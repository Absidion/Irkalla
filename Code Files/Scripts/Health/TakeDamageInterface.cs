using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface AITakeDamageInterface  {

    void TakeDamage(int playerNumber, int damage, Status[] statusEffects, int exteriorMultiplier);
}
