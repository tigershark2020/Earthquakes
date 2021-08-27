namespace Earthquakes
{
    class Program
    {
        private struct Coordinates
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }
        private struct UserSite
        {
            public string siteName { get; set; }
            public Coordinates coordinates { get; set; }
        }
        private class Metadata
        {
            public long generated { get; set; }
            public string url { get; set; }
            public string title { get; set; }
            public int status { get; set; }
            public string api { get; set; }
            public int count { get; set; }
        }
        private class Properties
        {
            public double mag { get; set; }
            public string place { get; set; }
            public long time { get; set; }
            public long updated { get; set; }
            // public int tz { get; set; }
            public string url { get; set; }
            public string detail { get; set; }
            // public string felt { get; set; }
            // public string cdi { get; set; }
            public string mmi { get; set; }
            public string alert { get; set; }
            public string status { get; set; }
            public int tsunami { get; set; }
            public int sig { get; set; }
            public string net { get; set; }
            public string code { get; set; }
            public string ids { get; set; }
            public string sources { get; set; }
            public string types { get; set; }
            // public int nst { get; set; }
            // public double dmin { get; set; }
            // public double rms { get; set; }
            // public double gap { get; set; }
            public string magType { get; set; }
            public string type { get; set; }
            public string title { get; set; }
        }
        private class Geometry
        {
            public string type { get; set; }
            public System.Collections.Generic.List<double> coordinates { get; set; }
        }
        private class FeaturesItem
        {
            public string type { get; set; }
            public Properties properties { get; set; }
            public Geometry geometry { get; set; }
            public string id { get; set; }
        }
        private class EarthquakesResult
        {
            public string type { get; set; }
            public Metadata metadata { get; set; }
            public System.Collections.Generic.List<FeaturesItem> features { get; set; }
            public System.Collections.Generic.List<double> bbox { get; set; }
        }
        private double GetDistance(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            var d1 = latitude1 * (System.Math.PI / 180.0);
            var num1 = longitude1 * (System.Math.PI / 180.0);
            var d2 = latitude2 * (System.Math.PI / 180.0);
            var num2 = longitude2 * (System.Math.PI / 180.0) - num1;
            var d3 = System.Math.Pow(System.Math.Sin((d2 - d1) / 2.0), 2.0) + System.Math.Cos(d1) * System.Math.Cos(d2) * System.Math.Pow(System.Math.Sin(num2 / 2.0), 2.0);

            return 3961 * (2.0 * System.Math.Atan2(System.Math.Sqrt(d3), System.Math.Sqrt(1.0 - d3)));
        }
        private string Get_Earthquakes_JSON()
        {
            string earthquakeJSON = string.Empty;
            // string url = @"https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_hour.geojson";
            // string url = @"https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_day.geojson";
            string url = @"https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_week.geojson";

            try
            {

                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
                using (System.IO.Stream stream = response.GetResponseStream())
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    earthquakeJSON = reader.ReadToEnd();
                }
            }
            catch (System.Exception e)
            {

            }

            return earthquakeJSON;
        }
        private bool Is_Large_US_Earthquake(double magnitude, string place, string[] stateList)
        {
            bool isLargeUSEarthquake = false;

            foreach (string stateName in stateList)
            {
                if (place.Contains(stateName) && magnitude >= 5)
                {
                    return true;
                }
            }

            return isLargeUSEarthquake;
        }
        private void Process_Applicable_Notification(Notifications.NotificationsGenerator.ConfigFiles jsonConfigPaths, FeaturesItem earthquake)
        {
            Notifications.NotificationsGenerator notificationsGenerator = new Notifications.NotificationsGenerator();

            string[] stateList = { "Alaska", "Nevada", "Hawaii", "Oklahoma", "Montana", "Idaho", "Washington", "California", "Utah", "Texas", "Oregon", "New Mexico", "North Carolina", "Wyoming", "Tennessee", "Kansas", "Missouri", "Arkansas", "Virginia", "Colorado", "Arizona", "Georgia", "South Carolina", "Illinois", "Maine", "Kentucky", "Alabama", "New York", "Mississippi", "Nebraska", "Ohio", "New Jersey", "New Hampshire", "Maryland", "Florida", "South Dakota", "Massachusetts", "Louisiana", "Indiana", "Michigan", "Connecticut", "Pennsylvania", "West Virginia", "Minnesota", "Rhode Island", "Vermont", "Delaware", "Wisconsin", "North Dakota", "District of Columbia", "Iowa" };

            System.Collections.Generic.List<Notifications.NotificationsGenerator.Event_Notification> eventNotificationList = new System.Collections.Generic.List<Notifications.NotificationsGenerator.Event_Notification>();
            System.Collections.Generic.List<UserSite> userSiteList = new System.Collections.Generic.List<UserSite>();

            string json = System.IO.File.ReadAllText(jsonConfigPaths.userSites_JsonFile);
            userSiteList = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<UserSite>>(json);

            foreach (UserSite user_site in userSiteList)
            {
                double earthquake_latitude = earthquake.geometry.coordinates[1];
                double earthquake_longitude = earthquake.geometry.coordinates[0];

                double hq_distance = GetDistance(earthquake_latitude, earthquake_longitude, user_site.coordinates.latitude, user_site.coordinates.longitude);

                double magnitude = earthquake.properties.mag;

                if ((Is_Large_US_Earthquake(magnitude, earthquake.properties.place, stateList)) || (earthquake.properties.title.Contains("Explosion")) || ((hq_distance < 50) || (earthquake.properties.tsunami.Equals(1)) || (magnitude >= 2 && hq_distance < 100) || (magnitude >= 3 && hq_distance < 250) || (magnitude >= 4 && hq_distance < 500) || (magnitude >= 5 && hq_distance < 2000) || (magnitude >= 6)))
                {
                    if (earthquake.properties.tsunami.Equals(1))
                    {
                        string notify_place = earthquake.properties.place + " Tsunami Evaluation Available";
                        earthquake.properties.place = notify_place;
                    }

                    Notifications.NotificationsGenerator.Event_Notification eventNotification = new Notifications.NotificationsGenerator.Event_Notification();
                    eventNotification.eventNotification_Agency = "48941";
                    eventNotification.eventNotification_Title = earthquake.properties.title;
                    eventNotification.eventNotification_URL = earthquake.properties.url;
                    eventNotification.eventNotification_DatetimeEpoch = (earthquake.properties.time / 1000);
                    eventNotification.eventNotification_Category = "Earthquake";
                    eventNotification.eventNotification_Type = "Earthquake";
                    eventNotification.eventNotification_UniqueID = earthquake.id;
                    eventNotification.eventNotification_Latitude = earthquake.geometry.coordinates[1];
                    eventNotification.eventNotification_Longitude = earthquake.geometry.coordinates[0];
                    eventNotification.eventNotification_ImageURL = null;
                    notificationsGenerator.Add_Event_Notification(jsonConfigPaths, eventNotification);
                    notificationsGenerator.Send_Blynk_Notification("USGS " + eventNotification.eventNotification_Title);
                }
            }
        }
        private void Add_New_Earthquakes_To_Database(Notifications.NotificationsGenerator.ConfigFiles jsonConfigPaths, System.Collections.Generic.List<FeaturesItem> newEarthquakesList)
        {
            string json = System.IO.File.ReadAllText(jsonConfigPaths.mysqlCredentialsInsert_JsonFile);
            MySql.Data.MySqlClient.MySqlConnectionStringBuilder conn_string_builder = Newtonsoft.Json.JsonConvert.DeserializeObject<MySql.Data.MySqlClient.MySqlConnectionStringBuilder>(json);

            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(conn_string_builder.ToString());
            try
            {
                conn.Open();
            }
            catch (System.Exception erro)
            {
                System.Console.WriteLine(erro);
            }

            foreach (FeaturesItem earthquake in newEarthquakesList)
            {
                try
                {
                    MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand();
                    cmd.Connection = conn;

                    System.Console.WriteLine(earthquake.id + "\t" + earthquake.properties.title);
                    cmd.CommandText = "INSERT INTO `event_data`.`earthquakes` (`geo_quake_id`, `geo_quake_title`, `geo_quake_place`, `geo_quake_epoch`, `geo_quake_latitude`, `geo_quake_longitude`, `geo_quake_magntiude`, `geo_quake_depth`, `geo_quake_tsunami_alert`) VALUES (@quake_id, @title, @place, @epoch, @latitude, @longitude, @magnitude, @depth, @tsunami);";

                    cmd.Parameters.AddWithValue("@quake_id", earthquake.id);
                    cmd.Parameters.AddWithValue("@title", earthquake.properties.title);
                    cmd.Parameters.AddWithValue("@place", earthquake.properties.place);
                    cmd.Parameters.AddWithValue("@epoch", earthquake.properties.time / 1000);
                    cmd.Parameters.AddWithValue("@latitude", earthquake.geometry.coordinates[1]);
                    cmd.Parameters.AddWithValue("@longitude", earthquake.geometry.coordinates[0]);
                    cmd.Parameters.AddWithValue("@magnitude", earthquake.properties.mag);
                    cmd.Parameters.AddWithValue("@depth", earthquake.geometry.coordinates[2]);
                    cmd.Parameters.AddWithValue("@tsunami", earthquake.properties.tsunami);

                    int insert_status = cmd.ExecuteNonQuery();

                    if (insert_status == 1)
                    {
                        Process_Applicable_Notification(jsonConfigPaths, earthquake);
                    }

                    cmd.Dispose();
                }
                catch (MySql.Data.MySqlClient.MySqlException sql_exception)
                {
                    int errorCode = sql_exception.ErrorCode;
                    if (errorCode != 1062)
                    {
                        System.Console.WriteLine("Quake Add Error:\t" + sql_exception.Message);
                    }
                }
            }


            conn.Close();
            conn.Dispose();

        }
        private System.Collections.Generic.List<string> Get_Recent_Existing_Earthquake_IDs(Notifications.NotificationsGenerator.ConfigFiles jsonConfigPaths)
        {
            System.Collections.Generic.List<string> existingEarthquakeList = new System.Collections.Generic.List<string>();

            string json = System.IO.File.ReadAllText(jsonConfigPaths.mysqlCredentialsSelect_JsonFile);
            MySql.Data.MySqlClient.MySqlConnectionStringBuilder conn_string_builder = Newtonsoft.Json.JsonConvert.DeserializeObject<MySql.Data.MySqlClient.MySqlConnectionStringBuilder>(json);

            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(conn_string_builder.ToString());

            try
            {
                conn.Open();
            }
            catch (System.Exception erro)
            {
                System.Console.WriteLine(erro);
            }

            MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT `geo_quake_id` FROM `event_data`.`earthquakes` WHERE FROM_UNIXTIME(`geo_quake_epoch`) > DATE_SUB(NOW(),INTERVAL 8 DAY);";

            try
            {
                MySql.Data.MySqlClient.MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string geo_quake_id = reader[0].ToString();
                    existingEarthquakeList.Add(geo_quake_id);
                }

                conn.Close();
                conn.Dispose();
            }
            catch (System.Exception erro)
            {
                System.Console.WriteLine(erro);
            }

            return existingEarthquakeList;
        }
        private bool Is_New_Earthquake(string earthquakeID, System.Collections.Generic.List<string> recentEarthquakesList)
        {
            bool newEarthquake = true;
            foreach (string recentEarthquakeId in recentEarthquakesList)
            {
                if (earthquakeID.Equals(recentEarthquakeId).Equals(true))
                {
                    newEarthquake = false;
                }
            }

            return newEarthquake;
        }
        static void Main(string[] args)
        {
            Program Earthquakes = new Program();

            System.Console.WriteLine("Earthquake Feed Reader");

            string configFilePaths = @"C:\Users\windowsusername\Documents\Credentials\Events\filePaths.json";
            bool exists = System.IO.File.Exists(configFilePaths);
            string json = null;

            try
            {
                json = System.IO.File.ReadAllText(configFilePaths, System.Text.Encoding.UTF8);
            }
            catch (System.Exception json_read)
            {
                System.Console.WriteLine(json_read.Message);
            }

            if (json != null) // Check That JSON String Read Above From File Contains Data
            {

                Notifications.NotificationsGenerator.ConfigFiles jsonConfigPaths = new Notifications.NotificationsGenerator.ConfigFiles();
                jsonConfigPaths = Newtonsoft.Json.JsonConvert.DeserializeObject<Notifications.NotificationsGenerator.ConfigFiles>(json);

                System.Collections.Generic.List<string> existingEarthquakesList = Earthquakes.Get_Recent_Existing_Earthquake_IDs(jsonConfigPaths);

                string earthquakes_json = Earthquakes.Get_Earthquakes_JSON();
                var settings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore
                };

                EarthquakesResult recent_earthquakes = Newtonsoft.Json.JsonConvert.DeserializeObject<EarthquakesResult>(earthquakes_json, settings);

                System.Collections.Generic.List<FeaturesItem> newEarthquakesList = new System.Collections.Generic.List<FeaturesItem>();
                foreach (FeaturesItem earthquake in recent_earthquakes.features)
                {
                    bool newEarthquake = Earthquakes.Is_New_Earthquake(earthquake.id, existingEarthquakesList);
                    if (newEarthquake == true)
                    {
                        newEarthquakesList.Add(earthquake);
                    }

                }
                Earthquakes.Add_New_Earthquakes_To_Database(jsonConfigPaths, newEarthquakesList);
                Notifications.NotificationsGenerator notificationsGenerator = new Notifications.NotificationsGenerator();
                notificationsGenerator.Generate_Event_Notification_JSON();
            }


        }
    }
}
