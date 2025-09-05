using UnityEngine;

public class BillPanelController : MonoBehaviour
{
    [SerializeField] GameObject billPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (billPanel.activeInHierarchy == false)
            {
                Open();
            }
            else
            {
                Close();
            }
        }
    }

    public void Open()
    {
        billPanel.SetActive(true);
    }

    public void Close()
    {
        billPanel.SetActive(false);
    }

    public GameObject GetBillPanel()
    {
        return billPanel;
    }
}