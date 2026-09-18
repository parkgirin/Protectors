using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Back_Box : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        // if (StageManager.Instance.DestroyAll) collision.gameObject.SetActive(false);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Do not silently remove enemies: the stage needs the kill event.
            enemy.Die();
            return;
        }
        collision.gameObject.SetActive(false);
    }
}
