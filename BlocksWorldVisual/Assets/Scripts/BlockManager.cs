// Created by John Wiesner 2020 for an AI university assignment
// 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UI;

/// <summary>
/// Class that manages the generation of blocks from fril code. This also controls the error screen.
/// </summary>
public class BlockManager : MonoBehaviour
{
	private const string _defOn = "on";
	private const string _defClear = "clear";
	private const string _defTable = "Table";
	private const string _isOn = " is on ";
	private const string _nothingStacked = " X has nothing stacked on top:";
	private const string _xIsOnYPlaceholder = "((on a b)(on b Table)(clear a))";
	private const string _yIsOnXPlaceholder = "((on b a)(on Table b)(clear a))";

	[Header("Input Fields")]
	[SerializeField] private TMP_InputField _onPredicate = null;
	[SerializeField] private TMP_InputField _clearPredicate = null;
	[SerializeField] private TMP_InputField _tableVarible = null;
	[SerializeField] private TMP_InputField _frilInput = null;

	[Header("Titles")]
	[SerializeField] private TMP_Text _errorText = null;
	[SerializeField] private TMP_Text _isOnText = null;
	[SerializeField] private TMP_Text _clearText = null;

	[Header("Game Objects and prefabs")]
	[SerializeField] private GameObject _errorMenu = null;
	[SerializeField] private GameObject _blockPrefab = null;
	[SerializeField] private GameObject _towerPrefab = null;
	[SerializeField] private GameObject _towers = null;
	
	private Dictionary<string, Block> _graph = null;    // Stores the blocks in graph form
	private Dictionary<string, string> _frilPhrasesParsed = null;
	private TMP_Text _frilInputPh = null;
	// Parse strings
	private string _onTable = _defOn;
	private string _table = _defTable;
	private string _isClear = _defClear;
	private string[] _parsedFril = null;

	private string _encounteredError = "";		// TODO: Should use exceptions but this is just a quick solution
	private List<List<string>> _cleanCode = new List<List<string>>();

	// States
	private bool _xOnYMode = true;

	private void Awake()
	{
		_frilInputPh = _frilInput.placeholder.GetComponent<TMP_Text>();
	}

	void Start()
    {
		SetErrorMenuState(false);
		UpdateTexts();
		GenerateBlocks(true);
	}

	/// <summary>
	/// Parses a fril statement, Generates blocks, and alerts user of any errors
	/// </summary>
	/// <param name="alertUser"></param>
	public void GenerateBlocks(bool alertUser = false)
	{
		_encounteredError = "";
		ClearBlocks();
		SetPredicateAndVarNames();
		_parsedFril = _frilInput.text.ContainsInfo() ? FrilToString(_frilInput.text) : FrilToString(_frilInputPh.text);

		//print("Parsed Fril: \n" + _parsedFril.ArrayToString());
		if(!_encounteredError.ContainsInfo())
			_cleanCode = FrilCommandsToListList(_parsedFril);

		if (!_encounteredError.ContainsInfo())
			_graph = CreateGraph(_cleanCode);

		if (!_encounteredError.ContainsInfo())
			CreateBlocks(_graph);

		if (alertUser)
		{
			if (!_encounteredError.ContainsInfo())
			{
				Debug.Log("Parsed Fril");
			}
			else
			{
				_errorText.text = _encounteredError;
				SetErrorMenuState(true);
				print(_encounteredError);
				Debug.Log("Invalid Fril Expression");
			}
		}
	}

	/// <summary>
	/// Hides or shows the error menu. Also clears the error message when hiding
	/// </summary>
	/// <param name="isActive"></param>
	public void SetErrorMenuState(bool isActive)
	{
		_errorMenu.SetActive(isActive);
		if(!isActive)
			_encounteredError = "";
	}

	/// <summary>
	/// Update the predicate and varible names
	/// </summary>
	private void SetPredicateAndVarNames()
	{
		_onTable = _onPredicate.text.ContainsInfo() ? _onPredicate.text : _defOn;
		_table = _tableVarible.text.ContainsInfo() ? _tableVarible.text : _defTable;
		_isClear = _isClear = _clearPredicate.text.ContainsInfo() ? _isClear = _clearPredicate.text : _defClear;
	}

	/// <summary>
	/// Create the actual blocks shown on screen
	/// </summary>
	/// <param name="graph">The graph to use</param>
	private void CreateBlocks(Dictionary<string, Block> graph)
	{
		foreach(string baseBlock in graph[_table].edges)
		{
			GameObject newTower = Instantiate(_towerPrefab, _towers.transform);
			string nextBlock = baseBlock;
			do
			{
				GameObject newBlock = Instantiate(_blockPrefab, newTower.transform);
				Image blockBackground = newBlock.GetComponentInChildren<Image>();
				TMP_Text blockTitle = newBlock.GetComponentInChildren<TMP_Text>();

				if(blockBackground != null && blockTitle != null)
				{
					blockBackground.color = ColorExtensions.RandomSaturatedColor();
					blockTitle.text = nextBlock;
				}

				nextBlock = graph[nextBlock].GetFirstEdge();

			} while (nextBlock != null);
		}
	}

	/// <summary>
	/// Removes all blocks
	/// </summary>
	private void ClearBlocks()
	{
		foreach (Transform child in _towers.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
	}

	/// <summary>
	/// Generate a graph like dictionary from a list of list fril command terms
	/// </summary>
	/// <param name="cleanCode">list of list fril command terms</param>
	/// <returns>A dictionary that acts like an adjacenty list</returns>
	private Dictionary<string, Block> CreateGraph(List<List<string>> cleanCode)
	{
		Dictionary<string, Block> outGraph = new Dictionary<string, Block>
		{
			[_table] = new Block(_table, true)
		};

		_encounteredError = "";
		bool atleast1Clear = false;
		foreach (List<string> stringCode in cleanCode)
		{
			bool wasValid = false;
			if (stringCode.Count == 2)
			{
				if (stringCode[0] == _isClear && !outGraph.ContainsKey(stringCode[1]))
					outGraph[stringCode[1]] = new Block(stringCode[1]);

				if(stringCode[0] == _isClear)
				{
					wasValid = outGraph[stringCode[1]].MakeClear();
					if(wasValid)
						atleast1Clear = true;
				}
			}
			else if(stringCode.Count == 3 && stringCode[0] == _onTable)
			{
				if (!outGraph.ContainsKey(stringCode[1]))
					outGraph[stringCode[1]] = new Block(stringCode[1]);

				if (!outGraph.ContainsKey(stringCode[2]))
					outGraph[stringCode[2]] = new Block(stringCode[2]);

				// Depending on what isOn mode user is using the edge direction will switch
				int[] indexs = _xOnYMode ? new int[2] { 2, 1 } : new int[2] { 1, 2 };
				wasValid = outGraph[stringCode[indexs[0]]].AddEdge(outGraph[stringCode[indexs[1]]]);
			}

			if(!wasValid)
			{
				_encounteredError = "Error: Invalid Graph!";
			}
		}

		if(!atleast1Clear)
		{
			_encounteredError = "Error: Invalid Graph!";
		}
		
		return outGraph;
	}

	/// <summary>
	/// Determines if a string has unbalanced parentheses or not
	/// </summary>
	/// <param name="inString">String to determine if</param>
	/// <returns>True if string contains balanced parentheses</returns>
	private bool HasBalancedParentheses(string inString)
	{
		int totalOffset = 0;
		foreach(char c in inString)
		{
			if (c == '(')
				totalOffset++;
			if (c == ')')
				totalOffset--;
		}

		return totalOffset == 0;
	}

	/// <summary>
	/// Converts fril string into a string array
	/// </summary>
	/// <param name="unparsedFril"></param>
	/// <returns>string array of fril commands</returns>
	private string[] FrilToString(string unparsedFril)
	{
		// Min fril statement is ((c a)) 
		if (unparsedFril.Length < (4 + Mathf.Min(_onTable.Length, _table.Length, _isClear.Length)))
		{
			_encounteredError = "Error: Not a complete blocks-world fril statement";
			return null;
		}
		
		if (!HasBalancedParentheses(unparsedFril))
		{
			_encounteredError = "Error: Fril statement contains unbalanced parentheses";
			return null;
		}

		// Dirty way to remove double parenthsies
		unparsedFril = unparsedFril.Remove(0, 1);
		unparsedFril = unparsedFril.Remove(unparsedFril.Length - 1 , 1);
		
		// Group each command
		string pattern = @"\(([^)]*)\)";
		string[] slpitString = Regex.Split(unparsedFril, pattern);

		List<string> areValid = new List<string>();
		_frilPhrasesParsed = new Dictionary<string, string>();
		foreach (string s in slpitString)
		{
			if(s.ContainsInfo())
			{
				if(_frilPhrasesParsed.ContainsKey(s))
				{
					_encounteredError = "Error: Fril statement contains duplicate statements";
					return null;
				}
				_frilPhrasesParsed[s] = s;
				areValid.Add(s);
			}
				
		}

		return areValid.Count > 0 ? areValid.ToArray() : null;
	}

	/// <summary>
	/// Converts a string array of fril commands to a list of list fril command terms
	/// </summary>
	/// <param name="stringArray">Fril array of commands</param>
	/// <returns>List of list fril command terms</returns>
	private List<List<string>> FrilCommandsToListList(string[] stringArray)
	{
		List<List<string>> masterList = new List<List<string>>();
		if (stringArray != null && stringArray.Length > 0)
		{
			foreach(string s in stringArray)
			{
				string[] subList = s.Split(' ');
				List<string> newSubList = new List<string>();

				foreach(string s2 in subList)
				{
					if (s2.ContainsInfo())
					{
						string newString = s2.Replace(" ", "");
						newString = newString.Replace("\n", "");
						newSubList.Add(newString);
					}
				}

				if(newSubList.Count > 0)
					masterList.Add(newSubList);
			}
		}

		return masterList;
	}

	/// <summary>
	/// Updates the titles of the input fields every time a user changes a predicate
	/// </summary>
	public void UpdateTexts()
	{
		// TODO: Make each input field have a seperate function to save recomputations
		ValidateInputField(ref _onPredicate);
		ValidateInputField(ref _tableVarible);
		ValidateInputField(ref _clearPredicate);

		SetPredicateAndVarNames();
		string isOnSwitch = _xOnYMode ? "X" + _isOn + "Y:" : "Y" + _isOn + "X:";
		_isOnText.text = "(" + _onTable + " X Y) " + isOnSwitch;
		_clearText.text = "(" + _isClear + " X)" + _nothingStacked;
		_frilInputPh.text = _xOnYMode ? _xIsOnYPlaceholder : _yIsOnXPlaceholder;
		GenerateBlocks();
	}

	/// <summary>
	/// Makes sure an input field contains a valid string
	/// </summary>
	/// <param name="inputF"></param>
	private void ValidateInputField(ref TMP_InputField inputF)
	{
		// TODO: This should be done within TMP input validation exstention
		string input = inputF.text;
		if(string.IsNullOrWhiteSpace(input))
		{
			inputF.text = "";
		}

		string newInput = input.Trim(new Char[] { '(', ')', ' ' });
		if(input != newInput)
		{
			inputF.text = newInput;
		}
	}

	/// <summary>
	/// Toggles the way 'isOn' is evauated
	/// </summary>
	public void ToggleIsOnState()
	{
		_xOnYMode = !_xOnYMode;
		UpdateTexts();
		GenerateBlocks();
	}

	public void ExitApplication()
	{

		#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
		#endif
		
		Application.Quit();

	}



	// Test cases:
	// Should pass:
	// ((on a b)(clear a)(on b Table))
	// ((clear a)(on a b)(on b Table))
	// ((on b Table)(clear a)(on a b))
	// ((on b Table)(clear b))
	// ((on a Table)(on b Table)(clear a)(clear b))
	// ((on a Table)(on b Table)(clear c)(on c b))
	// ((on a Table)(clear a))
	// ((onto at table)(onto best table)(clearing cute)(onto cute best))
	// ((on a Table)(on b a)(on c b)(on d c)(on e d)(on f e)(on g f)(clear g))




	// Should fail:
	// (())
	// ((on a Table)(on b Table)(on b Table)(on c b))
	// ((on b a)(on a b)(clear a)(clear b))
	// ((clear a)(clear b))

}
