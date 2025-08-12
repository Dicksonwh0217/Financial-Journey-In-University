using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockAgentDialogue : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TMPro.TextMeshProUGUI dialogueText;
    [SerializeField] Text agentNameText;

    [Header("Agent Settings")]
    [SerializeField] string agentName = "Bank Agent";

    [Header("Dialogue Messages")]
    [SerializeField]
    string[] greetingMessages = {
        "Welcome to the Stock Market! I'm here to help you trade stocks.",
        "Hello there! Ready to make some investments today?",
        "Good to see you! The market is looking quite active today.",
        "Welcome back! How can I assist you with your trading today?"
    };

    [SerializeField]
    string[] idleMessages = {
        "Take your time to browse the available stocks.",
        "Click on any stock to buy, or drag your owned stocks here to sell.",
        "The market prices update regularly, so keep an eye on the trends!",
        "Remember: Left click to buy/sell all, right click for partial transactions."
    };

    [SerializeField]
    string[] buySuccessMessages = {
        "Excellent choice! Your stock has been added to your portfolio.",
        "Transaction successful! You now own shares of {0}.",
        "Great investment! {0} has been purchased successfully.",
        "Well done! Your {0} stocks are now in your portfolio."
    };

    [SerializeField]
    string[] buyFailMessages = {
        "Sorry, you don't have enough money for this purchase.",
        "Insufficient funds! You need more money to buy this stock.",
        "Not enough RM in your account for this transaction.",
        "Your balance is too low for this purchase. Come back when you have more money!"
    };

    [SerializeField]
    string[] sellSuccessMessages = {
        "Stock sold successfully! The money has been added to your account.",
        "Great timing! You've sold your {0} stocks for RM{1}.",
        "Transaction complete! You earned RM{1} from selling {0}.",
        "Well traded! Your {0} stocks have been sold for RM{1}."
    };

    [SerializeField]
    string[] sellFailMessages = {
        "You need to select a stock from your inventory to sell.",
        "Please drag a stock from your inventory here to sell it.",
        "No stock selected! Choose a stock from your portfolio first.",
        "You must be holding a stock to sell it here."
    };

    [SerializeField]
    string[] marketClosedMessages = {
        "Sorry, the stock market is currently closed.",
        "The market is closed right now. Please come back during trading hours.",
        "Trading is not available at this time. The market is closed.",
        "The stock exchange is currently closed. Try again later."
    };

    [SerializeField]
    string[] farewellMessages = {
        "Thank you for trading with us! Come back anytime.",
        "Good luck with your investments! See you next time.",
        "Happy trading! Your portfolio is looking good.",
        "Take care! Remember to diversify your investments."
    };

    private void Start()
    {
        if (agentNameText != null)
        {
            agentNameText.text = agentName;
        }

        ShowGreeting();
    }

    public void ShowGreeting()
    {
        ShowRandomMessage(greetingMessages);
    }

    public void ShowIdle()
    {
        ShowRandomMessage(idleMessages);
    }

    public void ShowBuySuccess(string stockName)
    {
        string message = GetRandomMessage(buySuccessMessages);
        message = message.Replace("{0}", stockName);
        ShowMessage(message);
    }

    public void ShowBuyFail()
    {
        ShowRandomMessage(buyFailMessages);
    }

    public void ShowSellSuccess(string stockName, float amount)
    {
        string message = GetRandomMessage(sellSuccessMessages);
        message = message.Replace("{0}", stockName);
        message = message.Replace("{1}", amount.ToString("F2"));
        ShowMessage(message);
    }

    public void ShowSellFail()
    {
        ShowRandomMessage(sellFailMessages);
    }

    public void ShowMarketClosed()
    {
        ShowRandomMessage(marketClosedMessages);
    }

    public void ShowFarewell()
    {
        ShowRandomMessage(farewellMessages);
    }

    private void ShowRandomMessage(string[] messages)
    {
        if (messages.Length > 0)
        {
            string message = messages[Random.Range(0, messages.Length)];
            ShowMessage(message);
        }
    }

    private string GetRandomMessage(string[] messages)
    {
        if (messages.Length > 0)
        {
            return messages[Random.Range(0, messages.Length)];
        }
        return "";
    }

    private void ShowMessage(string message)
    {
        if (dialogueText != null)
        {
            dialogueText.text = message;
        }
    }

    // Coroutine to show idle messages periodically
    private IEnumerator IdleMessageCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f); // Wait 10 seconds
            ShowIdle();
        }
    }

    public void StartIdleMessages()
    {
        StartCoroutine(IdleMessageCoroutine());
    }

    public void StopIdleMessages()
    {
        StopAllCoroutines();
    }
}