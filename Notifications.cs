namespace Notifications
{
    class NotificationsGenerator
    {
        public class Blynk_Notification
        {
            public string body { get; set; }
        }
        public struct ConfigFiles
        {
            public string mysqlCredentialsSelect_JsonFile { get; set; }
            public string mysqlCredentialsInsert_JsonFile { get; set; }
            public string userSites_JsonFile { get; set; }
        }
        public class Event_Notification
        {
            public string id { get; set; }
            public string eventNotification_Agency { get; set; }
            public string eventNotification_Title { get; set; }
            public string eventNotification_Description { get; set; }
            public string eventNotification_URL { get; set; }
            public string eventNotification_ImageURL { get; set; }
            public long eventNotification_DatetimeEpoch { get; set; }
            public string eventNotification_Category { get; set; }
            public string eventNotification_Type { get; set; }
            public string eventNotification_UniqueID { get; set; }
            public double eventNotification_Latitude { get; set; }
            public double eventNotification_Longitude { get; set; }
        }
        public class Event_Notification_Response
        {
            public System.Collections.Generic.List<Event_Notification> Notifications { get; set; }
        }
        public int Add_Event_Notification(ConfigFiles jsonConfigPaths, Event_Notification eventNotification)
        {

            string json = System.IO.File.ReadAllText(jsonConfigPaths.mysqlCredentialsInsert_JsonFile);
            MySql.Data.MySqlClient.MySqlConnectionStringBuilder conn_string_builder = Newtonsoft.Json.JsonConvert.DeserializeObject<MySql.Data.MySqlClient.MySqlConnectionStringBuilder>(json);

            int result = 0;

            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(conn_string_builder.ToString());
            try
            {
                conn.Open();
            }
            catch (System.Exception erro)
            {
                System.Console.WriteLine(erro);
            }

            try
            {
                MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;

                cmd.CommandText = "INSERT INTO `event_data`.`geo_events` (`geo_event_agency`,`geo_event_title`,`geo_event_url`,`geo_event_endtime`,`geo_event_category`,`geo_event_type`,`geo_event_ident`,`geo_event_location_latitude`,`geo_event_location_longitude`,`geo_event_notify`,`geo_event_image_url`) VALUES (@event_notification_agency,@event_notification_title,@event_notification_url,FROM_UNIXTIME(@event_notification_datetime),@event_notification_category,@event_notification_type,@event_notification_ident,@event_notification_latitude,@event_notification_longitude,1,@event_notification_image_url);";
                cmd.Parameters.AddWithValue("@event_notification_agency", eventNotification.eventNotification_Agency);
                cmd.Parameters.AddWithValue("@event_notification_title", eventNotification.eventNotification_Title);
                cmd.Parameters.AddWithValue("@event_notification_url", eventNotification.eventNotification_URL);
                cmd.Parameters.AddWithValue("@event_notification_datetime", eventNotification.eventNotification_DatetimeEpoch);
                cmd.Parameters.AddWithValue("@event_notification_category", eventNotification.eventNotification_Category);
                cmd.Parameters.AddWithValue("@event_notification_type", eventNotification.eventNotification_Type);
                cmd.Parameters.AddWithValue("@event_notification_ident", eventNotification.eventNotification_UniqueID);
                cmd.Parameters.AddWithValue("@event_notification_latitude", eventNotification.eventNotification_Latitude);
                cmd.Parameters.AddWithValue("@event_notification_longitude", eventNotification.eventNotification_Longitude);
                cmd.Parameters.AddWithValue("@event_notification_image_url", eventNotification.eventNotification_ImageURL);

                result = cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                int errorcode = ex.Number;
                if (errorcode != 1062)
                {
                    System.Console.WriteLine("Notification Error:\t" + ex.Message);
                }

            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
            }

            conn.Close();
            conn.Dispose();

            return result;
        }
        public System.Collections.Generic.List<Event_Notification> Get_Event_Notifications_From_Database()
        {
            System.Collections.Generic.List<Event_Notification> eventNotification_List = new System.Collections.Generic.List<Event_Notification>();

            try
            {
                string configFilePaths = @"C:\Users\windowsusername\Documents\Credentials\Events\filePaths.json";
                bool exists = System.IO.File.Exists(configFilePaths);
                string filesJson = null;
                ConfigFiles jsonConfigPaths = new ConfigFiles();

                try
                {
                    filesJson = System.IO.File.ReadAllText(configFilePaths, System.Text.Encoding.UTF8);
                }
                catch (System.Exception json_read)
                {
                    System.Console.WriteLine(json_read.Message);
                }

                if (filesJson != null) // Check That JSON String Read Above From File Contains Data
                {

                    jsonConfigPaths = new ConfigFiles();
                    jsonConfigPaths = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigFiles>(filesJson);
                }

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
                cmd.CommandText = "SELECT `geo_id`,`geo_event_agency`,`geo_event_title`,UNIX_TIMESTAMP(`geo_event_starttime`) AS `geo_event_starttime`,`geo_event_category`,`geo_event_url`,`geo_event_image_url` FROM (SELECT `geo_id`,`geo_event_agency`,`geo_event_title`,`geo_event_starttime`,`geo_event_category`,`geo_event_url`,`geo_event_image_url` FROM `event_data`.`geo_events` WHERE (`geo_event_starttime` >= DATE_SUB(NOW(), INTERVAL 24 HOUR) AND `geo_event_starttime` <= DATE_ADD(NOW(), INTERVAL 2048 HOUR)) AND (`geo_event_notify` = 1 OR (`geo_event_agency` IN (\"NCAA\")) AND `geo_event_location_id` IS NULL) AND `geo_event_notified` IS NULL UNION SELECT `package_tracking_db_id` AS `geo_id`, CASE WHEN `package_tracking_carrier` IN (\"USPS\") THEN \"73943\" WHEN `package_tracking_carrier` IN (\"FDX\") THEN \"2238\" WHEN `package_tracking_carrier` IN (\"UPS\") THEN \"57374\" END AS `geo_event_agency`,`package_tracking_latest_status` AS `geo_event_title`,`package_tracking_latest_status_date` AS `geo_event_starttime`,`package_tracking_carrier` AS `geo_event_category`, CASE WHEN `package_tracking_carrier` IN (\"FDX\") THEN CONCAT('https://www.fedex.com/fedextrack/?trknbr=',`package_tracking_number`) WHEN `package_tracking_carrier` IN (\"UPS\") THEN CONCAT('https://wwwapps.ups.com/WebTracking/processInputRequest?InquiryNumber1=',`package_tracking_number`) WHEN `package_tracking_carrier` IN (\"USPS\") THEN CONCAT('https://tools.usps.com/go/TrackConfirmAction_input?origTrackNum=',`package_tracking_number`) END AS `geo_url`,CONCAT('https://multimedia.home/images/Products/',`inventory_product_id`,'.jpg') AS `geo_event_image_url` FROM `resource_data`.`package_tracking` WHERE package_tracking_latest_status NOT LIKE \"%Delivered%\" OR package_tracking_latest_status_date >= DATE_SUB(NOW(), INTERVAL 24 HOUR) AND package_tracking_number_received_scan IN (0)) a ORDER BY `geo_event_starttime` DESC, `geo_event_title` ASC;";

                MySql.Data.MySqlClient.MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Event_Notification eventNotification = new Event_Notification();
                    eventNotification.id = reader["geo_id"].ToString();
                    eventNotification.eventNotification_Agency = reader["geo_event_agency"].ToString();
                    eventNotification.eventNotification_Title = reader["geo_event_title"].ToString();

                    eventNotification.eventNotification_DatetimeEpoch = reader.GetInt32(reader.GetOrdinal("geo_event_starttime"));
                    eventNotification.eventNotification_URL = reader["geo_event_url"].ToString();
                    eventNotification.eventNotification_ImageURL = reader["geo_event_image_url"].ToString();
                    eventNotification_List.Add(eventNotification);
                }

                reader.Close();
                reader.Dispose();
                conn.Close();
                conn.Dispose();
            }
            catch (System.Exception erro)
            {
                System.Console.WriteLine(erro);
            }

            return eventNotification_List;
        }
        public void Generate_Event_Notification_JSON()
        {
            System.Collections.Generic.List<Event_Notification> eventNotification_List = Get_Event_Notifications_From_Database();
            Event_Notification_Response eventNotificationResponse = new Event_Notification_Response();
            eventNotificationResponse.Notifications = eventNotification_List;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(eventNotificationResponse);
            string folderPath = @"C:\Users\windowsusername\Documents\Data\NotificationData\";
            string file_name = "notifications.json";

            string file_path = folderPath + file_name;

            System.IO.File.WriteAllText(file_path, json);

        }
        public string Send_Blynk_Notification(string message)
        {
            Blynk_Notification blynkNotification = new Blynk_Notification();
            blynkNotification.body = message;

            string requestContent = Newtonsoft.Json.JsonConvert.SerializeObject(blynkNotification);

            string responseJsonString = null;
            string Blynk_Auth_Key = "s2MShdbpDm97ZnDi96R4vhBONXebnblx";

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };

            string blynk_notification_url = "https://blynk-cloud.com/" + Blynk_Auth_Key + "/notify";

            try
            {
                var httpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(blynk_notification_url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream()))
                {

                    streamWriter.Write(requestContent);
                }

                var httpResponse = (System.Net.HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    responseJsonString = streamReader.ReadToEnd();
                }
            }
            catch (System.Exception requestException)
            {
                responseJsonString = requestException.Message;
            }

            return responseJsonString;

        }

    }
}
