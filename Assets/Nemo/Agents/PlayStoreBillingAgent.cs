#undef ENABLE_PLUGIN

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class PlayStoreBillingAgent : NemoAgent 
{
	static PlayStoreBillingAgent	_instance;
	public static PlayStoreBillingAgent		instacne { get { return _instance; }}
	
#if UNITY_ANDROID && ENABLE_PLUGIN
	public static string	ReservedID_Purchased { get { return "android.test.purchased"; } }
	public static string	ReservedID_Canceled { get { return "android.test.canceled"; } }
	public static string	ReservedID_Refunded { get { return "android.test.refunded"; } }
	public static string	ReservedID_ItemUnavailable { get { return "android.test.item_unavailable";} }

	private static string	ObjectClassPath = "com.nemogames.PlayStoreBillingAgent";
	AndroidJavaObject 		android, arraylist;
	System.IntPtr			ptrMethodAdd, ptrMethodClear;
	private void		_QueryAvailableProducts(string[] productIds)
	{
		__fillArrayList(productIds);
		android.Call("QueryAvailableItems", arraylist);
	}
	private void		_QueryPurchasedProducts()
	{
		android.Call("QueryPurchasedProducts");
	}
	private void		_PurchaseProduct(string productId)
	{
		android.Call("PurchaseProduct", productId);
	}
	private void		_ConsumePurchase(string token)
	{
		android.Call("ConsumePurchase", token);
	}
	
	private void		__fillArrayList(string[] ids)
	{
		AndroidJNI.CallVoidMethod(arraylist.GetRawObject(), ptrMethodClear, AndroidJNIHelper.CreateJNIArgArray(new object[0]));
		object[] args = new object[1];
		foreach (string id in ids)
		{
			using (AndroidJavaObject jstring = new AndroidJavaObject("java.lang.String", id))
			{
				args[0] = jstring;
				AndroidJNI.CallBooleanMethod(arraylist.GetRawObject(), ptrMethodAdd,
					AndroidJNIHelper.CreateJNIArgArray(args));
			}
		}
	}
#else
	private void		_QueryAvailableProducts(string[] productIds) {}
	private void		_QueryPurchasedProducts() {}
	private void		_PurchaseProduct(string productId) {}
 	private void		_ConsumePurchase(string token) {}
	private void		__fillArrayList(string[] ids) {}
#endif
	
	public enum PlayStoreBillingEvent
	{
		OnQueryProductsSuccess = 1,
		OnQueryProductsFailed = 2,
		OnPurchaseSuccess = 3,
		OnPurchaseCancel = 4,
		OnPurchaseFailed = 5,
		OnQueryPurchasedSuccess = 6,
		OnQueryPurchasedFailed = 7,
		OnConsumptionCompleted = 8
	}
	
	public struct PurchaseData
	{
		public string		ProductId,
							OrderId,
							PurchaseTime,
							PurchaseToken;
	}
	public struct ProductData
	{
		public string		ProductId, Price;
	}
	
	public delegate void		HandleBillingEvent<T>(PlayStoreBillingEvent e, T data);
	public event HandleBillingEvent<PurchaseData>		OnPurchaseEvent;
	public event HandleBillingEvent<PurchaseData[]>		OnQueryPuchases;
	public event HandleBillingEvent<ProductData[]>		OnQueryProducts;
	public event HandleBillingEvent<int>				OnConsumptionCompleted;
	
	void		Awake()
	{
		_instance = this;
#if UNITY_ANDROID && ENABLE_PLUGIN
		android = new AndroidJavaObject(ObjectClassPath);
		android.Call("init", gameObject.name, "PlayStoreReceiveAgentEvent");
		
		arraylist = new AndroidJavaObject("java.util.ArrayList");
		ptrMethodAdd = AndroidJNI.GetMethodID(arraylist.GetRawClass(), "add", "(Ljava/lang/Object;)Z");
		ptrMethodClear = AndroidJNI.GetMethodID(arraylist.GetRawClass(), "clear", "()V");
#endif
	}
	private string	lastErrorString = "";
	
	public void		QueryAvailableProducts(string[] productIds) { _QueryAvailableProducts(productIds); }	
	public void		QueryPurchasedProducts() { _QueryPurchasedProducts(); }
	public void		PurchaseProduct(string productId) { _PurchaseProduct(productId); }
	public void		ConsumePurchase(string token) { _ConsumePurchase(token); }
	public string	LastError { get { return lastErrorString; } }
	
#region IAgentEventListener implementation
	public void		PlayStoreReceiveAgentEvent(string data)
	{
		Debug.Log("Event: " + data);
	}
#endregion
	
	#region NemoAgent implementation
#if ENABLE_PLUGIN
	public override bool		isEnable { get { return true; } }
#else
	public override bool		isEnable { get { return false; } }	
#endif
	#endregion
}
