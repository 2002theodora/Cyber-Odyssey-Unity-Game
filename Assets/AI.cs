using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenAI_API;
using OpenAI_API.Chat;
using System;
using OpenAI_API.Models;
using Unity.VisualScripting;

public class AI : MonoBehaviour
{
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    private List<ChatMessage> option1List;
    private List<ChatMessage> option2List;

    public GameObject menuCanvas;
    public Button btnStartNewStory;
    public Button btnContinueStory;
    public Button btnExit;

    public GameObject mainCanvas;
    public TMP_Text story;
    public Button option1;
    public Button option2;
    public Button btnMainMenu; // TODO
    public Button btnRereadStory; //TODO

    public GameObject rereadCanvas;

    public GameObject newStoryCanvas;
    public TMP_InputField inputPrompts;
    public TMP_InputField inputTitle;
    public Button nsBtnOK;
    public Button nsBtnCancel;

    public GameObject continueStoryCanvas;
    public ScrollRect scrollOldStories;
    public Button csBtnOK;
    public Button csBtnCancel;

    void Start()
    {
        /*api = new OpenAIAPI("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");

        ChatMessage response1 = new ChatMessage(ChatMessageRole.User, "I choose option 1");
        ChatMessage response2 = new ChatMessage(ChatMessageRole.User, "I choose option 2");

        startConversation();
        option1.onClick.AddListener(() => getResponse(response1));
        option2.onClick.AddListener(() => getResponse(response2));*/

        menuCanvas.SetActive(true);
        mainCanvas.SetActive(false);
        newStoryCanvas.SetActive(false);
        continueStoryCanvas.SetActive(false);


        //menu canvas
        btnStartNewStory.onClick.AddListener(() => {
            menuCanvas.SetActive(false);
            newStoryCanvas.SetActive(true);
        });
        btnContinueStory.onClick.AddListener(() =>
        {
            menuCanvas.SetActive(false);
            continueStoryCanvas.SetActive(true);
        });
        btnExit.onClick.AddListener(() =>
        {
            Application.Quit();
        });


        //new story canvas
        nsBtnOK.onClick.AddListener(() =>
        {
            newStoryCanvas.SetActive(false);
            mainCanvas.SetActive(true);

            string title = inputTitle.text;
            List<String> prompts = new List<string>(inputPrompts.text.Split(','));

            startGame(title, prompts);
       
        });

        nsBtnCancel.onClick.AddListener(() =>
        {
            newStoryCanvas.SetActive(false);
            menuCanvas.SetActive(true);
        });


        //continue story canvas
        csBtnOK.onClick.AddListener(() =>
        {
            continueStoryCanvas.SetActive(false);
            mainCanvas.SetActive(true);
            //startGame();
        });

        csBtnCancel.onClick.AddListener(() =>
        {
            continueStoryCanvas.SetActive(false);
            menuCanvas.SetActive(true);
        });
    }

    private void startConversation(List<string> prompts, String title)
    {

        messages = new List<ChatMessage>
        {
            new ChatMessage(ChatMessageRole.System, "You are writing a story where each chapter has two decisions. " +
           "You do not breack character under any circumstance. You start by describing the location or room we are in. " +
           "Each chapter needs to have at most 4 sentences. The sentences are short, concise and complete. " +
           "After each chapter,you generate two decisions for progressing the story and you wait for my decision. " +
           "After i select the decision i want to make, you generate the next chapter based on the previous chapters and decisions." +
           "The story must include the follwowing prompts: " + string.Join(", ", prompts) + " and match the following title: " + title)
        };

        story.text = "Click any button to start.";

    }

    private void getOption1()
    {
        option1List = new List<ChatMessage>
        {
            new ChatMessage(ChatMessageRole.System, "Based on what is written above, write the first choice.")
        };
    }

    private void getOption2()
    {
        option2List = new List<ChatMessage>
        {
            new ChatMessage(ChatMessageRole.System, "Based on what is written above, write the second choice.")
        };
    }
    /*
    //private async void getResponse()
    //{

    //// Create chat messages for the choices provided by the chatbot
    //ChatMessage choice1 = new ChatMessage(ChatMessageRole.System, "Based on the chapter you wrote, generate the first decision.");

    //ChatMessage choice2 = new ChatMessage(ChatMessageRole.System, "Based on the chapter you wrote, generate the second decision");

    //// Add the choice messages to the messages list
    //messages.Add(choice1);
    //messages.Add(choice2);

    //var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
    //{
    //    Model = Model.ChatGPTTurbo,
    //    Temperature = 0.9,
    //    MaxTokens = 100,
    //    Messages = messages
    //});

    //// Extract the story text from the chatbot's response
    //string storyText = chatResult.Choices[0].Message.TextContent;

    //// Display the story text in the text box
    //story.text = storyText;

    //string choice1Text = chatResult.Choices[1].Message.TextContent;
    //string choice2Text = chatResult.Choices[2].Message.TextContent;

    //// Display the choices on the buttons
    //option1.GetComponentInChildren<TextMeshProUGUI>().text = choice1Text;
    //option2.GetComponentInChildren<TextMeshProUGUI>().text = choice2Text;

    //}
    */

    private async void getResponse(ChatMessage optionChoice, String title)
    {
        //Deactivating the buttons
        option1.interactable = false;
        option2.interactable = false;

        messages.Add(optionChoice);

        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.9,
            MaxTokens = 100,
            Messages = messages
        });

        // Get the response message
        var responseMessage = chatResult.Choices[0].Message;
        messages.Add(responseMessage);

        // Display the story text in the text box
        story.text = responseMessage.TextContent;

        //Saving to file
        //saveToFile(title, messages);

        // Reactivating the buttons
        option1.interactable = true;
        option2.interactable = true;

    }

    private void startGame(String title, List<String> prompts)
    {
        api = new OpenAIAPI("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");


        ChatMessage response1 = new ChatMessage(ChatMessageRole.User, "I choose option 1");
        ChatMessage response2 = new ChatMessage(ChatMessageRole.User, "I choose option 2");

        startConversation(prompts, title);
        option1.onClick.AddListener(() => getResponse(response1, title));
        option2.onClick.AddListener(() => getResponse(response2, title));
    }

    /*
    private void startNewStory()
    {
        menuCanvas.SetActive(false);
        newStoryCanvas.SetActive(true);
    }

    private void continueStory()
    {
        menuCanvas.SetActive(false);
        continueStoryCanvas.SetActive(true);
    }

    private void exit()
    {
        Application.Quit();
    }

    private void nsOK()
    {
        newStoryCanvas.SetActive(false);
        mainCanvas.SetActive(true);
        string title=inputTitle.ToString();
        startGame();
    }

    private void nsCancel()
    {
        newStoryCanvas.SetActive(false);
        menuCanvas.SetActive(true);
    }

    private void csOK()
    {
        continueStoryCanvas.SetActive(false);
        mainCanvas.SetActive(true);
        startGame();
    }

    private void csCancel()
    {
        continueStoryCanvas.SetActive(false);
        menuCanvas.SetActive(true);
    }
    */

    private void saveToFile(string fileName, List<ChatMessage> messageList)
    {
        using (StreamWriter writer = new StreamWriter(fileName))
        {
            foreach (var message in messageList)
            {
                writer.WriteLine($"{message.Role}|{message.TextContent}");
            }
        }
    }

    private List<ChatMessage> loadFromFile(string fileName)
    {
        List<ChatMessage> messageList = new List<ChatMessage>();

        using (StreamReader reader = new StreamReader(fileName))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split('|');
                if (parts.Length == 2)
                {
                    ChatMessageRole role = (ChatMessageRole)Enum.Parse(typeof(ChatMessageRole), parts[0]);
                    string content = parts[1];
                    messageList.Add(new ChatMessage(role, content));
                }
            }
        }

        return messageList;
    }
}


