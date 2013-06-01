using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayStoreBillingSample : AgentUISample 
{
	public override string Title { get { return "Play Store Billing"; } }
	
	public string[]		SampleProducts;
	public string		SamplePurchasID;
	
	void Start()
	{
		PlayStoreBillingAgent.instacne.OnQueryProducts += 
		delegate(PlayStoreBillingAgent.PlayStoreBillingEvent e, PlayStoreBillingAgent.ProductData[] ps)
		{
			if (e == PlayStoreBillingAgent.PlayStoreBillingEvent.OnQueryProductsSuccess)
			{
				Debug.Log("Products List:");
				for (int i = 0; i < ps.Length; i++)
					Debug.Log("Product: " + ps[i].ProductId + " Price: " + ps[i].Price);
			} else Debug.Log(PlayStoreBillingAgent.instacne.LastError);
		};
		
		PlayStoreBillingAgent.instacne.OnPurchaseEvent +=
		delegate(PlayStoreBillingAgent.PlayStoreBillingEvent e, PlayStoreBillingAgent.PurchaseData p)
		{
			if (e == PlayStoreBillingAgent.PlayStoreBillingEvent.OnPurchaseSuccess)
			{
				Debug.Log("You purchased " + p.ProductId + ". real smart chioce :)");
			} else Debug.Log(PlayStoreBillingAgent.instacne.LastError);
		};
		
		PlayStoreBillingAgent.instacne.OnQueryPuchases +=
		delegate(PlayStoreBillingAgent.PlayStoreBillingEvent e, PlayStoreBillingAgent.PurchaseData[] ps)
		{
			if (e == PlayStoreBillingAgent.PlayStoreBillingEvent.OnQueryPurchasedSuccess)
			{
				Debug.Log("You already have:");
				for (int i = 0; i < ps.Length; i++)
					Debug.Log("Product: " + ps[i].ProductId);
			} else Debug.Log(PlayStoreBillingAgent.instacne.LastError);
		};
	}
	
	public override void DrawAgentGUI (Samples s)
	{
		if (GUILayout.Button("Query Available Products", s.ButtonStyle1))
		{
			PlayStoreBillingAgent.instacne.QueryAvailableProducts(SampleProducts);
		}
		if (GUILayout.Button("Query Purchased", s.ButtonStyle1))
		{
			PlayStoreBillingAgent.instacne.QueryPurchasedProducts();
		}
		if (GUILayout.Button("Static Purchase Success", s.ButtonStyle1))
		{
			PlayStoreBillingAgent.instacne.PurchaseProduct(PlayStoreBillingAgent.ReservedID_Purchased);
		}
		if (GUILayout.Button("Static Purchase Failed", s.ButtonStyle1))
		{
			PlayStoreBillingAgent.instacne.PurchaseProduct(PlayStoreBillingAgent.ReservedID_ItemUnavailable);
		}
		if (GUILayout.Button("Make Real Purchase", s.ButtonStyle1))
		{
			PlayStoreBillingAgent.instacne.PurchaseProduct(SamplePurchasID);
		}
	}
}
