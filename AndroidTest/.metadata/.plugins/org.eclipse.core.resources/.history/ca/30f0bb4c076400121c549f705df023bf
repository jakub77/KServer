package com.example.androidtest;

import java.io.InputStreamReader;
import java.io.Reader;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.DefaultHttpClient;
import org.json.JSONObject;

import android.os.AsyncTask;
import android.os.Bundle;
import android.app.Activity;
import android.content.Intent;
import android.view.Menu;
import android.view.View;
import android.widget.EditText;

public class MainActivity extends Activity {
	public final static String EXTRA_MESSAGE = "com.example.myfirstapp.MESSAGE";
	public final static String URI = "http://sunny.eng.utah.edu:81/Mobile.svc/";
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.activity_main, menu);
		return true;
	}
	
	class NetworkThread extends AsyncTask<String, Void, String>
	{
		@SuppressWarnings("unused")
		private Exception ex;
		protected String doInBackground(String... params) {			
			try{
				DefaultHttpClient client = new DefaultHttpClient();
				HttpGet request = new HttpGet(params[0]);
				
				request.setHeader("Accept", "application/json");
				request.setHeader("Content-type", "application/json");
				
				HttpResponse response = client.execute(request);
				Reader stringReader = new InputStreamReader(response.getEntity().getContent());
				char[] buffer = new char[(int)response.getEntity().getContentLength()];
				stringReader.read(buffer);
				stringReader.close();
				return new String(buffer);
			}
			catch(Exception e)
			{
				this.ex = e;
				return null;				
			}
		}
	}

	public void sendMessage(View view)
	{
		/*
		Intent intent = new Intent(this, DisplayMessageActivity.class);
		EditText editText = (EditText) findViewById(R.id.edit_message);
		String message = editText.getText().toString();
		intent.putExtra(EXTRA_MESSAGE, message);
		startActivity(intent);
		*/
		
		EditText editText = (EditText) findViewById(R.id.edit_message);
		
		try{
			String st = URI + "A/" + editText.getText().toString();
			NetworkThread n = new NetworkThread();
			n.execute(st);
			//String x = new NetworkThread().execute(st);
			String message = n.get();
			
			//JSONObject s = new JSONObject(new String(buffer));
			
			Intent intent = new Intent(this, DisplayMessageActivity.class);
			//String message = new String(buffer);
			intent.putExtra(EXTRA_MESSAGE, message);
			startActivity(intent);
			
			
		}
		catch(Exception e)
		{
			Intent intent = new Intent(this, DisplayMessageActivity.class);
			String message = e.toString();
			intent.putExtra(EXTRA_MESSAGE, message);
			startActivity(intent);		
		}
		
	}
}
