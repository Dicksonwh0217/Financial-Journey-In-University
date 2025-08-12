using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    Transform player;
    [SerializeField] float speed = 5f;
    [SerializeField] float pickUpDistance = 1.5f;
    [SerializeField] float ttl = 10f;
    public Item item;
    public int count = 1;

    private void Awake()
    {
        player = GameManager.instance.player.transform;
    }

    public void Set(Item item, int count)
    {
        this.item = item;
        this.count = count;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = item.icon;
    }

    private void Update()
    {
        ttl -= Time.deltaTime;
        if (ttl < 0)
        {
            Destroy(gameObject);
            return;
        }

        // Use 2D distance calculation
        Vector2 playerPos2D = new Vector2(player.position.x, player.position.y);
        Vector2 itemPos2D = new Vector2(transform.position.x, transform.position.y);
        float distance = Vector2.Distance(itemPos2D, playerPos2D);

        // Only move towards player if within pickup distance
        if (distance <= pickUpDistance)
        {
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            // Pick up when very close
            if (distance < 0.1f)
            {
                if (GameManager.instance.inventoryContainer != null)
                {
                    GameManager.instance.inventoryContainer.Add(item, count);
                }
                else
                {
                    Debug.LogWarning("No inventory container attached to the game manager");
                }
                Destroy(gameObject);
            }
        }
    }
}