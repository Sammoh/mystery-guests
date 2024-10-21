using System.Linq;
using Demo.Scripts;
using LobbyRelaySample;
using UnityEngine;
using UnityEngine.UI;

public class PopUp_Instructions : PopUpUI
{
    [SerializeField] private DemoCarouselView _carouselView;
    private DemoData[] items;
    
    private int _bannerCount
    {
        get => _instructions_v2.Length;
        set{}
    }

    [SerializeField] private Button _cleanupButton;

    private bool _isSetup;

    // add the instructions to from a list of string.
    // private string[] _instructions = new[]
    // {
    //     "Someone here is a Mischief!",
    //     "Identify which player among you is up to no good.",
    //     "Use the 'Case File' provided to mark off clues and solve the case.",
    //     "One of each Character, Motive, and Clue card are missing. Collect each of the other 'Case File' cards to figure out which ones are missing.", 
    //     "Action cards will help you reveal the truth, or hide your intentions.",
    //     "The Mischief will try to throw you off the trail. Don't let them get away with it!",
    //     "Once per round, each player may suggest a solution to accuse another player.",
    //     "At the end of each round, suggestions are gathered to accuse a player.",
    //     "If the majority of players agree, the accused player is eliminated.",
    //     "If the majority of players disagree, the mischief eliminates a player, and the next round begins.",
    // };

    private string[] _instructions_v2 =
    {
        "Every player will be given a chance to solve the puzzle and be the first to win the game.",
        "Innocent players can work together. Votes that are incorrect will be added to the common accusation.",
        "The Mischievous Guest must also figure out their own crime in order to get away with it.",
        "If none of the suggestions are correct then at the end of the round a player is eliminated by the Mischievous Guest",
        "Be careful who you tell information to. They may use it against you or try to beat you to the solution.",
        "Use clue cards to find out the solution. Action cards are also given to help validate a suspicious person.",
    };
    
    // todo add a mask to the content. 
    // get initial content images from the resource folder
    // create a carousel with the images and text
    // need to add a button to the carousel to allow the player to tap for next. 
    
    private void Start()
    {
        Setup();
        _cleanupButton.onClick.AddListener(Button_Close);
    }

    private void Setup()
    {
        if (_isSetup)
            return;

        // Pulls images from the resource folder and sets up the carousel
        items = Enumerable.Range(0, _bannerCount)
            .Select(i =>
            {
                var spriteResourceKey = $"instructions_{i+1:D2}";
                // var spriteResourceKey = $"TempArtBanner";
                // var text = $"Demo Banner {i:D2}";
                var text = _instructions_v2[i];
                return new DemoData(spriteResourceKey, text, () => Debug.Log($"Clicked: {text}"));
            })
            .ToArray();
        _carouselView.Setup(items);
        _isSetup = true;
    }
    
    public void Button_Close()
    {
        ClearPopup();
    }
    
    public void Button_Open()
    {
        ShowPopup();
    }

}
