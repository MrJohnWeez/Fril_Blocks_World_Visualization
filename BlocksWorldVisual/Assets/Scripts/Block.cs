using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Represents a block in the blocks world problem.
/// </summary>
public class Block
{
	public string self = null;

	// A block can only have two edges
	public List<string> edges = new List<string>();

	private bool isTable = false;
	public bool hasVisited = false;

	public Block()
	{

	}

	public Block(string selfName, bool isBlockATable = false)
	{
		self = selfName;
		isTable = isBlockATable;
	}

	public bool AddEdge(Block neigborBlock)
	{
		if(isTable || (!isTable && edges.Count < 1))
		{
			edges.Add(neigborBlock.self);
			return true;
		}
		return false;
	}

	public string GetFirstEdge()
	{
		if (edges.Count > 0)
			return edges[0];
		return null;
	}
}
