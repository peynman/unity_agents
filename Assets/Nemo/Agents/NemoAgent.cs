using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class NemoAgent : MonoBehaviour
{
	public abstract bool		isEnable { get; }
	
	public static void		LogDisabledAgentCall()
	{
		Debug.Log("You are trying to call a method from an agent while its disanled.");
	}
}
