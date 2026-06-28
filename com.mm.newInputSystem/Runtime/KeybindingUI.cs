using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class KeybindingUI : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform keybindingContainer;
    [SerializeField] private GameObject keybindingItemPrefab;
    [SerializeField] private GameObject subtitlePrefab;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    //private Dictionary<string, InputAction> actionMap = new Dictionary<string, InputAction>();
    private string SaveFileName = "InputBindings.json";//"Keybindings.json";

    private void Start()
    {
        SaveFileName = Path.Combine( Application.persistentDataPath, SaveFileName );
        LoadKeybindings();
        InitializeKeybindingUI();
        saveButton.onClick.AddListener( SaveKeybindings );
        loadButton.onClick.AddListener( LoadKeybindings );
    }
    private void InitializeKeybindingUI()
    {
        foreach ( var map in inputActions.actionMaps )
        {
            // Create and add a subtitle for the action map
            var subtitle = Instantiate( subtitlePrefab, keybindingContainer );
            var subtitleText = subtitle.GetComponent<TextMeshProUGUI>();
            subtitleText.text = map.name;

            foreach ( var action in map.actions )
            {
                for ( int i = 0 ; i < action.bindings.Count ; i++ )
                {
                    var binding = action.bindings[ i ];

                    // Check if it's part of a composite binding
                    if ( binding.isComposite )
                    {
                        // Add each part of the composite
                        for ( int j = i + 1 ; j < action.bindings.Count && action.bindings[ j ].isPartOfComposite ; j++ )
                        {
                            var keybindingItem = Instantiate( keybindingItemPrefab, keybindingContainer );
                            var actionNameText = keybindingItem.transform.Find( "ActionName" ).GetComponent<TextMeshProUGUI>();
                            var bindingButton = keybindingItem.transform.Find( "BindingButton" ).GetComponent<Button>();

                            actionNameText.text = $"{action.name} ({action.bindings[ j ].name})";
                            bindingButton.GetComponentInChildren<TextMeshProUGUI>().text = action.bindings[ j ].ToDisplayString();

                            int bindingIndex = j; // Capture the index for closure
                            bindingButton.onClick.AddListener( () => StartRebinding( action, bindingIndex, bindingButton ) );
                        }
                    }
                    else if ( !binding.isPartOfComposite )
                    {
                        // Handle non-composite bindings
                        var keybindingItem = Instantiate( keybindingItemPrefab, keybindingContainer );
                        var actionNameText = keybindingItem.transform.Find( "ActionName" ).GetComponent<TextMeshProUGUI>();
                        var bindingButton = keybindingItem.transform.Find( "BindingButton" ).GetComponent<Button>();

                        actionNameText.text = action.name;
                        bindingButton.GetComponentInChildren<TextMeshProUGUI>().text = binding.ToDisplayString();

                        int bindingIndex = i; // Capture the index for closure
                        bindingButton.onClick.AddListener( () => StartRebinding( action, bindingIndex, bindingButton ) );
                    }
                }
            }
        }
    }

    private void StartRebinding( InputAction action, int bindingIndex, Button bindingButton )
    {
        action.Disable();
        bindingButton.GetComponentInChildren<Text>().text = "Press a key...";

        action.PerformInteractiveRebinding( bindingIndex )
            .OnComplete( operation =>
            {
                action.Enable();
                bindingButton.GetComponentInChildren<Text>().text = action.bindings[ bindingIndex ].ToDisplayString();
                operation.Dispose();
            } )
            .Start();
    }

    private void SaveKeybindings()
    {
        var saveData = new BindingSaveData();

        foreach ( var map in inputActions.actionMaps )
        {
            foreach ( var action in map.actions )
            {
                for ( int i = 0 ; i < action.bindings.Count ; i++ )
                {
                    var binding = action.bindings[ i ];
                    saveData.Bindings.Add( new BindingEntry
                    {
                        ActionName = action.name,
                        BindingIndex = i,
                        Path = binding.effectivePath
                    } );
                }
            }
        }

        File.WriteAllText( SaveFileName, JsonUtility.ToJson( saveData ) );
        Debug.Log( "Keybindings saved at "+ SaveFileName );
    }

    private void LoadKeybindings()
    {
        if ( !File.Exists( SaveFileName ) )
        {
            Debug.LogWarning( "No keybindings file found. Loading defaults." );
            CreateDefaultKeybindings();
            return;
        }

        // Read saved bindings
        var savedBindings = JsonUtility.FromJson<BindingSaveData>( File.ReadAllText( SaveFileName ) );

        foreach ( var map in inputActions.actionMaps )
        {
            foreach ( var action in map.actions )
            {
                for ( int i = 0 ; i < action.bindings.Count ; i++ )
                {
                    var savedBinding = savedBindings.Bindings.Find( b => b.ActionName == action.name && b.BindingIndex == i );
                    if ( savedBinding != null )
                    {
                        action.ApplyBindingOverride( i, savedBinding.Path );
                    }
                }
            }
        }

        Debug.Log( "Keybindings loaded from " + SaveFileName );
    }


    private void CreateDefaultKeybindings()
    {
        var defaultBindings = new List<string>();

        foreach ( var map in inputActions.actionMaps )
        {
            foreach ( var action in map.actions )
            {
                foreach ( var binding in action.bindings )
                {
                    defaultBindings.Add( JsonUtility.ToJson( binding ) );
                }
            }
        }

        File.WriteAllText( SaveFileName, JsonUtility.ToJson( defaultBindings ) );
        Debug.Log( "Default keybindings created and saved." );
    }
}
// Class to store binding data
[System.Serializable]
public class BindingSaveData
{
    public List<BindingEntry> Bindings = new List<BindingEntry>();
}

[System.Serializable]
public class BindingEntry
{
    public string ActionName;
    public int BindingIndex;
    public string Path; // Binding path like "<Keyboard>/space"
}