using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WindowsServiceApiCaller.Models;

namespace WindowsServiceApiCaller
{
    public partial class WindowsApiCallService : ServiceBase
    {
        Timer timer = new Timer();
        DateTime scheduleDateTime;

        enum MyDistricts
        {
            Pune = 363,
            Akola,
            Nagpur,
            Amravati,
            Buldhana,
            Yavatmal,
            Washim,
            Bhandara,
            Ratnagiri,
            Sangli,
            Sindhudurg,
            Solapur,
            Satara,
            Wardha,
            Gondia,
            Gadchiroli,
            Chandrapur,
            Osmanabad,
            Nanded,
            Latur,
            Beed,
            Parbhani,
            Hingoli,
            Nandurbar,
            Nashik,
            Jalgaon,
            Ahmednagar,
            Thane,
            Raigad,
            Palghar,
            Mumbai,
            Jalna,
            Aurangabad
        }
        public WindowsApiCallService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteLogFile("Service is started");
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            scheduleDateTime = DateTime.Today.AddHours(16).AddMinutes(10);
            var scheduleInterval = scheduleDateTime.Subtract(DateTime.Now).TotalSeconds * 1000;
            if (scheduleInterval < 0)
            {
                scheduleInterval += new TimeSpan(24, 0, 0).TotalSeconds * 1000;
            }
            timer.Interval = 120 * 1000;
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteLogFile("Service is stopped");
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            string currdate = DateTime.Today.ToString("dd-MM-yyyy");
            string urlAmt = "https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/calendarByDistrict?district_id=" + (int)MyDistricts.Amravati + "&date=" + currdate;
            string ApiData = new WebClient().DownloadString(urlAmt);
            if (timer.Interval != 90 * 1000)
            {
                timer.Interval = 90 * 1000; //Reset the timer    
            }
            CheckForAmravati(ApiData);
        }

        public void CheckForAmravati(string apiData)
        {
            try
            {
                string message = "";
                bool msgSentAmt = false;
                //to get below credentials for your own number, 
                //create a account on "https://www.thetexting.com" and put below keys in config files
                string api_secret = ConfigurationManager.AppSettings["api_secret"];
                string api_key = ConfigurationManager.AppSettings["api_key"];
                string mobnunber = ConfigurationManager.AppSettings["mobnunber"];

                //var ApiDatajsonApiData = JsonConvert.DeserializeObject(ApiData);
                Root myrootObj = JsonConvert.DeserializeObject<Root>(apiData);
                foreach (Center itm in myrootObj.centers)
                {
                    foreach (var sesnItm in itm.sessions)
                    {
                        if (sesnItm.available_capacity > 0 && msgSentAmt == false && itm.block_name != "Dharni" && sesnItm.min_age_limit == 18)//&& itm.block_name != "Dharni"
                        {
                            message = $"{sesnItm.available_capacity} - {sesnItm.vaccine} available at {itm.block_name} for {sesnItm.min_age_limit} age. Name- {itm.name} & Address- {itm.address}, {itm.pincode}";
                            WriteLogFile(message);

                            var client = new RestClient("https://www.thetexting.com/rest/sms/json/message/send");
                            var request = new RestRequest(Method.POST);
                            request.AddHeader("content-type", "application/x-www-form-urlencoded");
                            request.AddHeader("cache-control", "no-cache");
                            request.AddParameter("application/x-www-form-urlencoded", "api_secret=" + api_secret + "&api_key=" + api_key + "&from=test&to=" + mobnunber + "&text=" + message + "&type=text", ParameterType.RequestBody);
                            IRestResponse response = client.Execute(request);

                            msgSentAmt = true;
                            WriteLogFile($"Data is done with fetching - {response.ResponseStatus}");
                        }
                        else
                        {
                            WriteLogFile($"Message sent one time Amravati");
                            WriteLogFile($"{sesnItm.available_capacity} - {sesnItm.vaccine} available at {itm.block_name} for {sesnItm.min_age_limit} age. Name- {itm.name} & Address- {itm.address}, {itm.pincode}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WriteLogFile($"Data is not fetched with exception- {e.StackTrace}");
                throw;
            }
        }

        public void WriteLogFile(string message)
        {
            StreamWriter sw = null;
            sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile.txt", true);
            sw.WriteLine($"{DateTime.Now.ToString()} : {message}");
            sw.Flush();
            sw.Close();
        }
    }
}
