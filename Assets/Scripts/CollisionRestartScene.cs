using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionRestartScene : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "CustomPlayer")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
