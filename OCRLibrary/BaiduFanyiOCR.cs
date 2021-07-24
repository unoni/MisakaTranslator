using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace OCRLibrary
{
    public class BaiduFanyiOCR : OCREngine
    {
        public string appId;
        public string secretKey;
        const string salt = "123456";
        private string langCode;

        public override string OCRProcess(Bitmap img)
        {
            if (img == null || langCode == null || langCode == "") {
                errorInfo = "Param Missing";
                return null;
            }

            string base64 = ImageProcFunc.GetFileBase64(img);
            string sign = CommonFunction.EncryptString(appid + CommonFunction.EncryptString(base64) + salt + "APICUIDmac" + secretKey);
            string host = "https://fanyi-api.baidu.com/api/trans/sdk/picture?cuid=APICUID&mac=mac&salt=" + salt + "&appid=" + appId+"&sign="+sign;

            HttpWebRequest request = WebRequest.CreateHttp(host);
            request.Method = "POST";
            request.ContentType = "multipart/form-data";
            String str = "language_type=" + langCode + "&image=" + WebUtility.UrlEncode(base64);
            byte[] buffer = Encoding.Default.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string result = reader.ReadToEnd();
            response.Close();

            return null;
        }

        public override bool OCR_Init(string param1, string param2)
        {
            appId = param1;
            secretKey = param2;
            return true;
        }

        /// <summary>
        /// 百度OCRAPI申请地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_allpyAPI()
        {
            return "https://fanyi-api.baidu.com/";
        }

        /// <summary>
        /// 百度OCRAPI额度查询地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_bill()
        {
            return "https://fanyi-api.baidu.com/api/trans/product/desktop";
        }

        /// <summary>
        /// 百度翻译OCRAPI文档地址
        /// </summary>
        /// <returns></returns>
        public static string GetUrl_Doc()
        {
            return "https://fanyi-api.baidu.com/doc/26";
        }

        public override void SetOCRSourceLang(string lang)
        {
            if (lang == "jpn")
            {
                lang = "jap";
            }

            langCode = lang.ToUpper();
        }
    }

    // class BaiduTokenOutInfo
    // {
    //     public string access_token { get; set; }
    //     public int expires_in { get; set; }
    //     public string error { get; set; }
    //     public string error_description { get; set; }

    //     public string refresh_token { get; set; }
    //     public string scope { get; set; }
    //     public string session_key { get; set; }
    //     public string session_secret { get; set; }
    // }

    // class BaiduOCRresOutInfo
    // {
    //     public long log_id { get; set; }
    //     public List<BaiduOCRresDataOutInfo> words_result { get; set; }
    //     public int words_result_num { get; set; }

    // }

    // class BaiduOCRresDataOutInfo
    // {
    //     public string words { get; set; }
    // }

    // class BaiduOCRErrorInfo
    // {
    //     public short error_code{ get; set; }
    //     public string error_msg{ get; set; }
    // }

}
