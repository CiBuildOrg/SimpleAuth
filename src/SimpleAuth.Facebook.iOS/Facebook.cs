﻿using System;
using fb = Facebook;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using UIKit;
using Foundation;
namespace SimpleAuth.Providers
{
	public static class Facebook
	{
		static Foundation.NSObject invoker = new Foundation.NSObject();
		public static void Init(UIKit.UIApplication app, Foundation.NSDictionary launchOptions)
		{
			FacebookApi.IsUsingNative = true;
			fb.CoreKit.ApplicationDelegate.SharedInstance.FinishedLaunching(app, launchOptions);
			FacebookApi.ShowFacebookAuthenticator = (a) => invoker.BeginInvokeOnMainThread(() => Login(a));
			Native.RegisterCallBack ("facebook",(UIApplication appplication, NSUrl url, NSDictionary options) => OpenUrl(appplication,url,options as NSDictionary<NSString, NSObject>));
		}
		public static bool OpenUrl(UIApplication app, NSUrl url, string sourceApp, NSObject annotation)
		{
			return fb.CoreKit.ApplicationDelegate.SharedInstance.OpenUrl(app, url, sourceApp, annotation);
		}

		public static bool OpenUrl (UIApplication app, NSUrl url, NSDictionary<NSString,NSObject> options)
		{
			return fb.CoreKit.ApplicationDelegate.SharedInstance.OpenUrl (app, url, options);
		}

		public static async void Login (WebAuthenticator authenticator)
		{
			var fbAuth = authenticator as FacebookAuthenticator;
			try
			{
				fb.CoreKit.Settings.AppID = fbAuth.ClientId;
				var loginManager = new fb.LoginKit.LoginManager();
				var window = UIKit.UIApplication.SharedApplication.KeyWindow;
				var root = window.RootViewController;
				if (root != null)
				{
					var current = root;
					while (current.PresentedViewController != null)
					{
						current = current.PresentedViewController;
					}

					var resp = await loginManager.LogInWithReadPermissionsAsync(authenticator.Scope.ToArray(),current);
					if (resp.IsCancelled)
					{
						authenticator.OnCancelled();
						return;
					}
					var date = (DateTime)resp.Token.ExpirationDate;
					var expiresIn = (long)(date - DateTime.Now).TotalSeconds;
					fbAuth.OnRecievedAuthCode(resp.Token.TokenString,expiresIn);
				}
			}
			catch (Exception ex)
			{
				authenticator.OnError(ex.Message);
			}
		}
	}
}

