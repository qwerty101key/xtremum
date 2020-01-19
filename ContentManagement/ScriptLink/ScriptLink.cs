using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace ContentManagement
{
    public class ScriptLink : IHtmlString
    {
        public enum LoadingMode
        {
            Async,
            Defer
        }

        private class ScriptLinkAuxilliarySettings
        {
            public LoadingMode? AsyncMode { get; set; }
            public ICacher Cacher { get; set; }
        }

        private readonly string template = "<script src=\"{0}?hash={1}\" type=\"text/javascript\"{2}></script>";

        public string FileUrl { get; private set; }
        public LoadingMode? AsyncMode { get; private set; }
        public IHasher Hasher { get; private set; }
        public ICacher Cacher { get; private set; }

        private ScriptLink(string fileUrl, IHasher hasher, ScriptLinkAuxilliarySettings auxilliarySettings = null)
        {
            FileUrl = fileUrl;
            Hasher = hasher;
            AsyncMode = auxilliarySettings?.AsyncMode;
            Cacher = auxilliarySettings?.Cacher;
        }        

        public static IHtmlString Render(string fileUrl, LoadingMode? asyncMode = null, bool caching = false)
        {
            /* Реализация хэширователя */
            
            //IHasher hasher = new MD5Hasher();
            IHasher hasher = new TimerHasher();

            /* Реализация кеширователя */

            ICacher cacher = new DefaultCacher(TimeSpan.FromMinutes(3), new List<string>()
            {
                HttpContext.Current.Server.MapPath(fileUrl)
            });

            ScriptLink scriptLink = new ScriptLink(fileUrl, hasher, new ScriptLinkAuxilliarySettings
            {
                AsyncMode = asyncMode,
                Cacher = cacher
            });

            return new HtmlString(scriptLink.ToHtmlString());                     
        }

        private string GetHtmlString(string fileUrl, string mode, string hash) => string.Format(template, fileUrl, hash, mode);
        
        private string GetAsyncModeString()
        {
            if (AsyncMode == LoadingMode.Async)
            {
                return " async";
            }
            else if (AsyncMode == LoadingMode.Defer)
            {
                return " defer";
            }
            else
            {
                return string.Empty;
            }
        }

        public string ToHtmlString()
        {
            string asyncModeString = GetAsyncModeString();

            try
            {
                if (Cacher == null)
                {
                    /* Считываем содержимое файла */

                    string newFileContent = File.ReadAllText(
                        HttpContext.Current.Server.MapPath(FileUrl));

                    /* Вычисляем хеш содержимого файла */
                    string hash = Hasher.Hash(newFileContent);

                    return GetHtmlString(FileUrl, asyncModeString, hash);
                }
                else
                {
                    string existingValue = Cacher.GetValue(FileUrl) as string;

                    if (string.IsNullOrWhiteSpace(existingValue))
                    {
                        /* Кэш хеша не найден */

                        /* Считываем содержимое файла */

                        string newFileContent = File.ReadAllText(
                            HttpContext.Current.Server.MapPath(FileUrl));

                        /* Вычисляем хеш содержимого файла */
                        string hash = Hasher.Hash(newFileContent);

                        /* Сохраняем новое значение хэша */

                        Cacher.SetValue(FileUrl, hash);

                        return GetHtmlString(FileUrl, asyncModeString, hash);
                    }
                    else
                    {
                        /* Кэш хеша найден */
                        return GetHtmlString(FileUrl, asyncModeString, existingValue);
                    }
                }                
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}