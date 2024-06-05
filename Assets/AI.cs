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

    public GameObject menuCanvas;
    public Button btnStartNewStory;
    public Button btnContinueStory;
    public Button btnExit;

    public GameObject mainCanvas;
    public TMP_Text story;
    public Button option1;
    public Button option2;
    public Button btnMainMenu;
    public Button btnRereadStory;

    public GameObject rereadCanvas;
    public Button btnReturnToMain;
    public TMP_Text rereadStoryText;

    public GameObject newStoryCanvas;
    public TMP_InputField inputPrompts;
    public TMP_InputField inputTitle;
    public Button nsBtnOK;
    public Button nsBtnCancel;

    public GameObject continueStoryCanvas;
    public TMP_Dropdown dropdownOldStories;
    public Button csBtnOK;
    public Button csBtnCancel;

    private string selectedTitle;

    private const string storyPath = "Assets\\Stories";
    
    void Start()
    {

        menuCanvas.SetActive(true);
        mainCanvas.SetActive(false);
        newStoryCanvas.SetActive(false);
        continueStoryCanvas.SetActive(false);
        rereadCanvas.SetActive(false);


        //menu canvas
        btnStartNewStory.onClick.AddListener(() => {
            menuCanvas.SetActive(false);
            newStoryCanvas.SetActive(true);
        });
        btnContinueStory.onClick.AddListener(() =>
        {
            menuCanvas.SetActive(false);
            continueStoryCanvas.SetActive(true);
            loadOldStories();
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

            //TODO - make the rereadCanvas display the old chapters and decisions
            rereadStoryText.text = messages.ToString();
       
        });

        nsBtnCancel.onClick.AddListener(() =>
        {
            newStoryCanvas.SetActive(false);
            menuCanvas.SetActive(true);
        });

        btnMainMenu.onClick.AddListener(() =>
        {
            mainCanvas.SetActive(false);
            menuCanvas.SetActive(true);
        });

        btnRereadStory.onClick.AddListener(() =>
        {
            rereadCanvas.SetActive(true);
            mainCanvas.SetActive(false);
        });


        //continue story canvas
        csBtnOK.onClick.AddListener(() =>
        {
            continueStoryCanvas.SetActive(false);

            List<ChatMessage> oldMessages = loadFromFile(selectedTitle);

            mainCanvas.SetActive(true);
            // Load the story to continue from where it left off
            restartGame(selectedTitle, oldMessages);




        });

        csBtnCancel.onClick.AddListener(() =>
        {
            continueStoryCanvas.SetActive(false);
            menuCanvas.SetActive(true);
        });

        //reread canvas
        btnReturnToMain.onClick.AddListener(() =>
        {
            rereadCanvas.SetActive(false);
            menuCanvas.SetActive(false);
            mainCanvas.SetActive(true);
        });


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
            MaxTokens = 150,
            Messages = messages
        });

        // Get the response message
        var responseMessage = chatResult.Choices[0].Message;
        messages.Add(responseMessage);

        // Display the story text in the text box
        story.text = responseMessage.TextContent;

        //Saving to file
        saveToFile(title, messages);

        // Reactivating the buttons
        option1.interactable = true;
        option2.interactable = true;

    }

    private void startGame(String title, List<String> prompts)
    {
        api = new OpenAIAPI("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");

        ChatMessage response1 = new ChatMessage(ChatMessageRole.User, "I choose option 1");
        ChatMessage response2 = new ChatMessage(ChatMessageRole.User, "I choose option 2");

        //ensuring there are no duplicate listeners
        option1.onClick.RemoveAllListeners();
        option2.onClick.RemoveAllListeners();

        startConversation(prompts, title);
        option1.onClick.AddListener(() => getResponse(response1, title));
        option2.onClick.AddListener(() => getResponse(response2, title));
    }

    private void restartConversation(List<ChatMessage> oldMessasges)
    {

        messages = oldMessasges;
        messages.Add(new ChatMessage(ChatMessageRole.User, "Based on the previous messages, continue the story."));

        story.text = "Click any button to start.";

    }

    private void restartGame(String title, List<ChatMessage> oldMessages)
    {
        api = new OpenAIAPI("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");

        ChatMessage response1 = new ChatMessage(ChatMessageRole.User, "I choose option 1");
        ChatMessage response2 = new ChatMessage(ChatMessageRole.User, "I choose option 2");

        //ensuring there are no duplicate listeners
        option1.onClick.RemoveAllListeners();
        option2.onClick.RemoveAllListeners();

        restartConversation(oldMessages);
        option1.onClick.AddListener(() => getResponse(response1, title));
        option2.onClick.AddListener(() => getResponse(response2, title));
    }

    private void saveToFile(string fileName, List<ChatMessage> messageList)
    {
        string filePath = Path.Combine(storyPath, fileName + ".txt");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var message in messageList)
            {
                writer.WriteLine($"{message.Role}:{message.TextContent}");
            }
        }
    }

    private List<ChatMessage> loadFromFile(string fileName)
    {
        List<ChatMessage> messageList = new List<ChatMessage>();

        string filePath = Path.Combine(storyPath, fileName + ".txt");

        // Check if the file exists
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] splitLine = line.Split(':');
                    if (splitLine.Length == 2)
                    {
                        ChatMessageRole role = (ChatMessageRole)Enum.Parse(typeof(ChatMessageRole), splitLine[0]);
                        string content = splitLine[1];
                        messageList.Add(new ChatMessage(role, content));
                    }
                }
            }
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }

        return messageList;
    }
    /*
    private void loadOldStories()
    {
        // Clear existing options in the Dropdown
        dropdownOldStories.ClearOptions();

        // Get the list of story files from the save directory
        string[] storyFiles = Directory.GetFiles(storyPath, "*.txt");

        List<string> options = new List<string>();

        // Create an option for each story file
        foreach (string storyFile in storyFiles)
        {
            // Extract the title from the file name (without extension)
            string title = Path.GetFileNameWithoutExtension(storyFile);

            // Add the title to the options list
            options.Add(title);
        }

        // Set the options for the Dropdown
        dropdownOldStories.AddOptions(options);

        // Add a listener to the Dropdown to handle selection
        dropdownOldStories.onValueChanged.AddListener(delegate
        {
            SelectStory(dropdownOldStories.options[dropdownOldStories.value].text);
        });
    }
    */

    private void loadOldStories()
    {
        // Clear existing options in the Dropdown
        dropdownOldStories.ClearOptions();

        // Get the list of story files from the save directory
        string[] storyFiles = Directory.GetFiles(storyPath, "*.txt");

        List<string> options = new List<string>();

        // Create an option for each story file
        foreach (string storyFile in storyFiles)
        {
            // Extract the title from the file name (without extension)
            string title = Path.GetFileNameWithoutExtension(storyFile);

            // Add the title to the options list
            options.Add(title);
        }

        // Set the options for the Dropdown
        dropdownOldStories.AddOptions(options);

    }


    private void SelectStory(string title)
    {
        selectedTitle = title;
    }

}