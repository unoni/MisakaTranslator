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

            string googleTransUrl = "https://translate.google.cn/translate_a/single?client=gtx&dt=t&sl=" + srcLang + "&tl=" + desLang + "&q=" + Uri.EscapeDataString(sourceText);

            try
            {
                var ResultHtml = await GetResultHtml(googleTransUrl);

                using var doc = JsonDocument.Parse(ResultHtml);

                var TempResult = doc.RootElement;

                if(TempResult.GetArrayLength() > 0)
                    return string.Join("", TempResult[0].EnumerateArray().Select(x => x[0]));
                else
                    return "";
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

        public void TranslatorInit(string param1 = "", string param2 = "") { }

        public async Task<string> GetResultHtml(string url)
        {

            var hc = CommonFunction.GetHttpClient();
            return await hc.GetStringAsync(url);
        }
    }
}
