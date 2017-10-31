using UnityEngine;
using System.Collections;

/// <summary>
/// Base packet data class
/// </summary>
[System.Serializable]
public class PacketData {
	public string cmd {get; set;}
	public bool success {get; set;}	
}


