using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace TranslatorLibrary
{
    public class GoogleCNTranslator : ITranslator
    {
        private string errorInfo;//错误信息

        Jint.Native.JsValue ng; // Jint engine，执行JS代码

        public string GetLastError()
        {
            return errorInfo;
        }

        public async Task<string> TranslateAsync(string sourceText, string desLang, string srcLang)
        {
            if (desLang == "zh")
                desLang = "zh-cn";
            if (srcLang == "zh")
                srcLang = "zh-cn";

            if (desLang == "jp")
                desLang = "ja";
            if (srcLang == "jp")
                srcLang = "ja";

            if (desLang == "kr")
                desLang = "ko";
            if (srcLang == "kr")
                srcLang = "ko";

            var tk = ng.Invoke(sourceText);

            string googleTransUrl = "https://translate.google.cn/translate_a/single?client=webapp&sl=" + srcLang + "&tl=" + desLang + "&hl=zh-CN&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&ie=UTF-8&oe=UTF-8&clearbtn=1&otf=1&pc=1&srcrom=0&ssel=0&tsel=0&kc=2&tk=" + tk + "&q=" + Uri.EscapeDataString(sourceText);

            try
            {
                var ResultHtml = await GetResultHtml(googleTransUrl);

                using var doc = JsonDocument.Parse(ResultHtml);

                var TempResult = doc.RootElement;

                if(TempResult.GetArrayLength() > 0)
                    return string.Join("", TempResult[0].EnumerateArray().Select(x => x[0]));
                else
                    return "";

                // string ResultText = "";

                // if (TempResult != null)
                // {

                //     for (int i = 0; i < TempResult[0].Count; i++)
                //     {
                //         if (TempResult[0][i] != null)
                //         {
                //             ResultText += TempResult[0][i][0];
                //         }
                //     }
                // }

                // return ResultText;
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                errorInfo = ex.Message;
                return null;
            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                errorInfo = ex.Message;
                return null;
            }
        }

        public void TranslatorInit(string param1 = "", string param2 = "")
        {
            string TkkJS = File.ReadAllText($"{Environment.CurrentDirectory}\\lib\\GoogleJS.js");
            ng = new Jint.Engine().Execute(TkkJS).GetValue("TL");
        }

        /// <summary>
        /// 访问页面
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cc"></param>
        /// <param name="refer"></param>
        /// <returns></returns>
        public async Task<string> GetResultHtml(string url)
        {

            var hc = CommonFunction.GetHttpClient();
            return await hc.GetStringAsync(url);

            // string usergant = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
            //webRequest.Referer = "https://translate.google.cn/";
            //webRequest.Headers.Add("X-Requested-With:XMLHttpRequest");
            //webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
        }
    }
}
