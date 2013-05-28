using UnityEngine;
using System.Collections;

public abstract class NemoAgent : MonoBehaviour
{
	public abstract string		Title { get; }
	public abstract bool		isEnable { get; }
	public abstract string		Description { get; }
	public abstract string		VersionName { get; }
#if UNITY_ANDROID
	public abstract string		ElementName { get; }
#endif
}
