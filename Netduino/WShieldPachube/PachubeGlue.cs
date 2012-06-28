using System;
using System.Net;
using System.Text;
using System.IO;
using Microsoft.SPOT;

namespace WShieldPachube
{
    class PachubeGlue
    {
        private string URL = "";
        private string keyAPI = "";

        public PachubeGlue(string URL, string keyAPI)
        {
            this.URL = URL;
            this.keyAPI = keyAPI;
        }

        public bool sendMeasures(float pressure, float temperature, float humidity)
        {
            bool result = false;

            WebRequest rq = WebRequest.Create(URL);

            rq.Timeout = 3000;
            string query = "Temperature," + toString(temperature) + "\nPressure," + toString(pressure) + "\nHumidity," + toString(humidity);
            byte[] buffer = Encoding.UTF8.GetBytes(query);
            rq.ContentLength = buffer.Length;
            rq.Method = "PUT";
            rq.ContentType = "text/csv";
            rq.Headers.Add("X-PachubeApiKey", keyAPI);

            try
            {
                Stream stm = rq.GetRequestStream();
                stm.Write(buffer, 0, buffer.Length);
                stm.Close();
                HttpWebResponse hwr = (HttpWebResponse)rq.GetResponse();
                hwr.Close();

                result = (hwr.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
            }

            //rq.Dispose();

            return result;
        }

        private string toString(float number)
        {
            string result = number.ToString();

            int dotPos = result.IndexOf(".");
            if (dotPos == -1)
                result = result + ".00";
            else
            {
                int lastPos = ((dotPos + 3) > result.Length) ? result.Length : dotPos + 3;
                result = result.Substring(0, dotPos) + result.Substring(dotPos, lastPos - dotPos);
            }

            return result;
        }
    }
}
