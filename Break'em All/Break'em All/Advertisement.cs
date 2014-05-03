using Microsoft.Advertising.Mobile.Xna;
using Microsoft.Xna.Framework;
using System;
using System.Device.Location;
using System.Diagnostics;

namespace Break_em_All
{
    class Advertisement
    {
        //// We will use this to find the device location for better ad targeting.
        private GeoCoordinateWatcher gcw = null;

        private DrawableAd bannerAd;

        public Advertisement(Rectangle adUnitRect, string applicationId, string adUnitId)
        {
            this.bannerAd = AdGameComponent.Current.CreateAd(adUnitId, adUnitRect, true);
            this.bannerAd.Visible = false; // Initially ad will not visible. Call AdvertisementObj.setVisible(bool) to show the ad.

            // Set some visual properties (optional).
            this.bannerAd.BorderEnabled = true;
            this.bannerAd.BorderColor = Color.Black;
            this.bannerAd.DropShadowEnabled = true;

            // Add handlers for events (optional).
            bannerAd.ErrorOccurred += new EventHandler<Microsoft.Advertising.AdErrorEventArgs>(bannerAd_ErrorOccurred);
            bannerAd.AdRefreshed += new EventHandler(bannerAd_AdRefreshed);
            bannerAd.VisibleChanged += new EventHandler(bannerAd_VisibleChanged);
            bannerAd.EngagedChanged += new EventHandler(bannerAd_EngagedChanged);

            // Provide the location to the ad for better targeting (optional).
            // This is done by starting a GeoCoordinateWatcher and waiting for the location to be available.
            // The callback will set the location into the ad. 
            // Note: The location may not be available in time for the first ad request.
            AdGameComponent.Current.Enabled = false;
            this.gcw = new GeoCoordinateWatcher();
            this.gcw.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(gcw_PositionChanged);
            this.gcw.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(gcw_StatusChanged);
            this.gcw.Start();
        }

        private void gcw_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            // Stop the GeoCoordinateWatcher now that we have the device location.
            this.gcw.Stop();

            bannerAd.LocationLatitude = e.Position.Location.Latitude;
            bannerAd.LocationLongitude = e.Position.Location.Longitude;

            AdGameComponent.Current.Enabled = true;

            Debug.WriteLine("Device lat/long: " + e.Position.Location.Latitude + ", " + e.Position.Location.Longitude);
        }

        private void gcw_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Disabled || e.Status == GeoPositionStatus.NoData)
            {
                // in the case that location services are not enabled or there is no data
                // enable ads anyway
                AdGameComponent.Current.Enabled = true;
                Debug.WriteLine("GeoCoordinateWatcher Status :" + e.Status);
            }
        }

        /// <summary>
        /// This is called whenever ad is engaged by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bannerAd_EngagedChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Ad is now visible.");
        }

        /// <summary>
        /// This is called whenever visible property of bannerAdObj is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bannerAd_VisibleChanged(object sender, EventArgs e)
        {
            if (this.bannerAd.Visible)
            {
                Debug.WriteLine("Ad is now visible.");
            }
            else
            {
                Debug.WriteLine("Ad is now hidden.");
            }
        }

        /// <summary>
        /// This is called whenever a new ad is received by the ad client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bannerAd_AdRefreshed(object sender, EventArgs e)
        {
            Debug.WriteLine("Ad received successfully");
        }

        /// <summary>
        /// This is called when an error occurs during the retrieval of an ad.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Contains the Error that occurred.</param>
        private void bannerAd_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Debug.WriteLine("Ad error: " + e.Error.Message);
        }

        /// <summary>
        /// Clean up the GeoCoordinateWatcher
        /// </summary>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.gcw != null)
                {
                    this.gcw.Dispose();
                    this.gcw = null;
                }
            }
        }

        public void setVisible(bool isVisible)
        {
            this.bannerAd.Visible = isVisible;
        }

        public static void Initialize(Game game, string applicationId)
        {
            // Initialize the AdGameComponent with your ApplicationId and add it to the game.
            AdGameComponent.Initialize(game, applicationId);
            game.Components.Add(AdGameComponent.Current);
        }

        public bool isUserEngaged()
        {
            return bannerAd.Engaged;
        }
    }
}
