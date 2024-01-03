using System.Net;

namespace HttpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string prefix = "http://localhost:8000/";
            string baseDirectory = "./"; // ここに静的ファイルがあるディレクトリを指定

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            Console.WriteLine($"HTTPサーバーが {prefix} で起動しています...");

            while (true)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    await ProcessRequestAsync(context, baseDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("リクエスト処理中にエラーが発生しました: " + ex.Message);
                }
            }
        }

        private static async Task ProcessRequestAsync(HttpListenerContext context, string baseDirectory)
        {
            string filePath = context.Request.Url.AbsolutePath.TrimStart('/');
            if (string.IsNullOrEmpty(filePath) || Directory.Exists(Path.Combine(baseDirectory, filePath)))
            {
                filePath = Path.Combine(filePath, "index.html"); // デフォルトのファイルを指定
            }

            string filename = Path.Combine(baseDirectory, filePath);
            if (!File.Exists(filename))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
                return;
            }

            try
            {
                using (Stream input = new FileStream(filename, FileMode.Open))
                {
                    context.Response.ContentType = GetMimeType(filename);
                    context.Response.ContentLength64 = input.Length;
                    await input.CopyToAsync(context.Response.OutputStream);
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
                Console.WriteLine("ファイル提供中にエラーが発生しました: " + ex.Message);
            }
        }
        
        private static string GetMimeType(string filename)
        {
            string extension = Path.GetExtension(filename).ToLowerInvariant();
            return extension switch
            {
                ".html" => "text/html",
                ".css"  => "text/css",
                ".js"   => "application/javascript",
                ".json" => "application/json",
                ".png"  => "image/png",
                ".jpg"  => "image/jpeg",
                ".gif"  => "image/gif",
                // その他のファイルタイプに対しては適切なMIMEタイプを追加
                _ => "application/octet-stream",
            };
        }

    }
}
