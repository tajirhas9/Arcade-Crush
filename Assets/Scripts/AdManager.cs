using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
	public BannerView bannerView;
	public InterstitialAd interstitialAd;



    void Start()
    {
     	MobileAds.Initialize(initStatus => { });  

     	this.RequestAd(); 
    }

    public void RequestAd() {
    	#if UNITY_ANDROID
    		string bannerAdUnitId = "ca-app-pub-3464311937927215/1616112084";
    		string interstitialAdUnitId = "ca-app-pub-3464311937927215/8814170736";
    		string rewardedAdUnitId = "ca-app-pub-3464311937927215/6907026842";
    	#else
    		string bannerAdUnitId = "unexpected_platform";
    		string interstitialAdUnitId = "unexpected_platform";
    		string rewardedAdUnitId = "unexpected_platform";
    	#endif

    	this.bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
    	this.interstitialAd = new InterstitialAd(interstitialAdUnitId);

    	// Create an empty ad request.
        AdRequest bannerRequest = new AdRequest.Builder().Build();
        AdRequest interstitialRequest = new AdRequest.Builder().Build();

        // Load the banner with the request.
        this.bannerView.LoadAd(bannerRequest);
        this.interstitialAd.LoadAd(interstitialRequest);
    }

    public void ShowInterstitialAd() {
    	if (this.interstitialAd.IsLoaded()) {
			this.interstitialAd.Show();
		}
    }

    public void DestroyInterstitialAd() {

        if (interstitialAd != null) {
            interstitialAd.Destroy();
        }
    }

    public void DestroyBannerView() {

    	if(this.bannerView != null)
    	this.bannerView.Destroy();
    }
}
