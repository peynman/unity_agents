using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System;

public class WebAgent : NemoAgent
{
	public delegate void	HandleResponse(string content, CookieContainer cookies);
	
	public static void	RequestPost(string url, HandleResponse callback, CookieContainer cookies, params string[] post_data_pair)
	{
		if (post_data_pair != null && post_data_pair.Length%2!=0)
		{
			Debug.LogError("Post data params shoud be an array of name and value pairs");
			return;
		}
		// create request
		HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
		req.Method = "POST"; 
        req.ContentType = "application/x-www-form-urlencoded";
		req.AllowWriteStreamBuffering = true;
		req.KeepAlive = true;
		req.Timeout = 4000;
		// append cookies
		if (cookies == null)
			req.CookieContainer = new CookieContainer();
		else
		{
			req.CookieContainer = cookies;
		}
		// append post data
		byte[] data = null;
		if (post_data_pair != null && post_data_pair.Length>0)
		{
			string post_data = "";
			for (int i = 0; i < post_data_pair.Length; i+=2)
				post_data += post_data_pair[i]+"="+post_data_pair[i+1]+((i==post_data_pair.Length-2)? "":"&");
			ASCIIEncoding encoding = new ASCIIEncoding();
			data = encoding.GetBytes(post_data);
			Debug.Log(post_data);
		}
		if (data != null)
		{
			req.ContentLength = data.Length;
			using (Stream stream = req.GetRequestStream())
			{
				stream.Write(data, 0, data.Length);
				stream.Close();
			}
		} else
		{
			req.ContentLength = 0;
		}
		// get response
		HttpWebResponse response = (HttpWebResponse)req.GetResponse();
		Stream streamResponse = response.GetResponseStream();
        StreamReader streamRead = new StreamReader(streamResponse);
        string responseString = streamRead.ReadToEnd();
		if (callback != null) callback(responseString, req.CookieContainer);
        streamResponse.Close();
        streamRead.Close();
		/*
		req.BeginGetResponse(
		delegate(System.IAsyncResult asynchronousResult) 
		{
        	HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream streamResponse = response.GetResponseStream();
	        StreamReader streamRead = new StreamReader(streamResponse);
	        string responseString = streamRead.ReadToEnd();
			if (callback != null) callback(responseString, request.CookieContainer);
	        streamResponse.Close();
	        streamRead.Close();
	        response.Close();
		}, req);*/
	}
	
	public static void	RequestGet(string url, HandleResponse callback, CookieContainer cookies, params string[] post_data_pair)
	{
		if (post_data_pair != null && post_data_pair.Length%2!=0)
		{
			Debug.LogError("Post data params shoud be an array of name and value pairs");
			return;
		}
		// create get data
		string post_data = "";
		for (int i = 0; i < post_data_pair.Length; i+=2)
			post_data += post_data_pair[i]+"="+post_data_pair[i+1]+((i==post_data_pair.Length-2)? "":"&");
		if (post_data != "") url += "?"+post_data;
		// create request
		HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
		req.Method = "GET";
        req.ContentType = "application/x-www-form-urlencoded";
		req.AllowWriteStreamBuffering = true;
		req.KeepAlive = true;
		req.Timeout = 4000;
		// append cookies
		if (cookies == null)
			req.CookieContainer = new CookieContainer();
		else
		{
			req.CookieContainer = cookies;
		}
		// get response
		HttpWebResponse response = (HttpWebResponse)req.GetResponse();
		Stream streamResponse = response.GetResponseStream();
        StreamReader streamRead = new StreamReader(streamResponse);
        string responseString = streamRead.ReadToEnd();
		if (callback != null) callback(responseString, req.CookieContainer);
        streamResponse.Close();
        streamRead.Close();
		/*
		req.BeginGetResponse(
		delegate(System.IAsyncResult asynchronousResult) 
		{
        	HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream streamResponse = response.GetResponseStream();
	        StreamReader streamRead = new StreamReader(streamResponse);
	        string responseString = streamRead.ReadToEnd();
			if (callback != null) callback(responseString, request.CookieContainer);
	        streamResponse.Close();
	        streamRead.Close();
	        response.Close();
		}, req);*/
	}
	
	#region NemoAgent implementation
#if ENABLE_PLUGIN
	public override bool		isEnable { get { return true; } }
#else
	public override bool		isEnable { get { return false; } }	
#endif
	#endregion
}
